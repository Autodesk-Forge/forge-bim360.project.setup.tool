/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM 'AS IS' AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using BimProjectSetupCommon.Helpers;
using Autodesk.Forge.BIM360;
using Autodesk.Forge.BIM360.Serialization;


namespace BimProjectSetupCommon.Workflow
{
    public class FolderWorkflow : BaseWorkflow
    {
        private int retryCounter = 0;
        private HubsApi _hubsApi = null;
        private BimProjectsApi _projectApi = null;
        private BimProjectFoldersApi _foldersApi = null;
        private Dictionary<string, List<NestedFolder>> folderStructures = new Dictionary<string, List<NestedFolder>>();
        private Dictionary<string, List<IndustryRole>> _projectToRolesDict = new Dictionary<string, List<IndustryRole>>();

        public FolderWorkflow(AppOptions options) : base(options)
        {
            _hubsApi = new HubsApi(GetToken, options);
            _projectApi = new BimProjectsApi(GetToken, options);
            _foldersApi = new BimProjectFoldersApi(GetToken, options);
            DataController.InitializeAllProjects();
            DataController.InitializeCompanies();
            DataController.InitializeAccountUsers();
            DataController.InitializeHubs();
        }
        public List<NestedFolder> CustomGetFolderStructure(BimProject project)
        {
            return CustomExtractFolderStructure(project);
        }
        public string CustomCreateFolder(string projId, string parentFolderId, string newFolderName)
        {
            return _foldersApi.CustomCreateFolder(projId, parentFolderId, newFolderName);
        }
        public bool CustomAssignPermissionToFolder(string projectId, string folderId, List<FolderPermission> folderPermissions)
        {
            return _foldersApi.CustomAssignPermission(projectId, folderId, folderPermissions);
        }


        public void CopyFoldersProcess()
        {
            CsvReader.ReadDataFromProjectCSV();
            if (DataController._projectTable.Rows.Count > 0)
            {
                foreach (BimProject proj in DataController.GetBimProjects())
                {
                    if (string.IsNullOrWhiteSpace(proj.folders_created_from))
                    {
                        Log.Error($"Project name to copy folders is not provided");
                        return;
                    }

                    BimProject orgProj = DataController.AllProjects.Find(x => x.name == proj.folders_created_from);
                    if (string.IsNullOrWhiteSpace(proj.service_types))
                    {
                        orgProj = DataController.GetProjectWithServiceById(orgProj.id);
                        if (orgProj.service_types.Contains("insight"))
                        {
                            proj.service_types = orgProj.service_types.Replace("insight,", "");
                        }
                    }

                    proj.include_name_to_request_body = true;
                    CopyFoldersProcess(orgProj, proj, _options.HqAdmin);
                }
            }
        }


        public void CopyFoldersProcess(string orgProjName, string newProjName, string adminEmail, List<string> roles)
        {
            if (orgProjName == "" || orgProjName == null)
            {
                Log.Error($"Project name to copy folders is not provided");
                return;
            }

            // create project using existing project data
            BimProject orgProj = DataController.AllProjects.Find(x => x.name == orgProjName);
            BimProject newProj = DataController.GetProjectWithServiceById(orgProj.id);
            if (newProj.service_types.Contains("insight"))
            {
                newProj.service_types = newProj.service_types.Replace("insight,", "");
            }
            newProj.name = newProjName;
            //newProj.updateName = true;

            CopyFoldersProcess(orgProj, newProj, adminEmail, roles);
        }

        private bool CheckRequiredParams(BimProject proj)
        {
            bool isNull = string.IsNullOrEmpty(proj.name) || string.IsNullOrEmpty(proj.project_type) || string.IsNullOrEmpty(proj.value)
                || string.IsNullOrEmpty(proj.currency);

            return !isNull;
        }

        public void CopyFoldersProcess(BimProject orgProj, BimProject newProj, string adminEmail, List<string> roles = null)
        {
            List<string> roleIds = new List<string>();
            HqUser admin = new HqUser();

            Log.Info("");
            Log.Info("============================================================");
            Log.Info("FOLDER COPY WORKFLOW started for " + newProj.name);
            Log.Info("============================================================");
            Log.Info("");

            Log.Info($"Creating project by copying data from original project '{orgProj.name}'");

            try
            {
                if (false == CheckRequiredParams(newProj))
                {
                    Log.Error("One of the required parameters is missing. Required are: Name, Project Type, Value, Currency, Start date, End date");
                    Log.Error("End procesing for this project");
                }
                ExtractFolderStructure(orgProj);
                newProj.include_name_to_request_body = true;

                bool result = CreateProject(orgProj, newProj, out string newProjId);
                if (result)
                {
                    roleIds = GetIndustryRoleIds(newProj.name, roles);
                    admin = GetAdminUser(adminEmail);
                    if (true == ActivateProject(newProjId, admin, roleIds))
                    {
                        CopyProjectFolders(orgProj, newProjId, admin.uid);
                    }
                }
                Log.Info("");
                Log.Info("FOLDER COPY WORKFLOW finished for " + newProj.name);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private List<NestedFolder> CustomExtractFolderStructure(BimProject orgProj)
        {
            if (!folderStructures.ContainsKey(orgProj.name))
            {
                List<NestedFolder> existingFolderStructure = new List<NestedFolder>();
                Log.Info($"");
                Log.Info($"Folder structure extraction started");
                Log.Info("- retrieving top folders from " + orgProj.id);
                // call TopFolder to get the initial folder ids to start
                TopFolderResponse topFolderRes = GetTopFolders(orgProj.id);


                if (topFolderRes.data == null || topFolderRes.data.Count() == 0)
                {
                    Log.Warn("No top folders retrieved.");
                    // TODO: debug!
                    Log.Warn("Please start the program again.");
                    return existingFolderStructure;
                }

                Log.Info("- retrieving sub-folders for 'Plans' and 'Project Files' folder. This could take a while..");
                // Iterate root folders
                foreach (Folder folder in topFolderRes.data)
                {
                    string folderName = folder.attributes.name;
                    if (folderName == "Project Files" || folderName == "Plans")
                    {
                        NestedFolder rootFolder = new NestedFolder(folderName, folder.id);

                        // recursive calls to fetch folder structure
                        NestedFolder rootWithChildrenFolder = _foldersApi.GetFoldersHierarchy(orgProj.id, rootFolder);
                        existingFolderStructure.Add(rootWithChildrenFolder);
                    }
                }
                folderStructures[orgProj.name] = existingFolderStructure;

                return existingFolderStructure;

            } else
            {
                throw new ApplicationException($"");
            }
        }
        private void ExtractFolderStructure(BimProject orgProj)
        {
            if (folderStructures.ContainsKey(orgProj.name))
            {
                return;
            }
            else
            {
                List<NestedFolder> existingFolderStructure = new List<NestedFolder>();
                Log.Info($"");
                Log.Info($"Folder structure extraction started");
                Log.Info("- retrieving top folders from " + orgProj.id);
                // call TopFolder to get the initial folder ids to start
                TopFolderResponse topFolderRes = GetTopFolders(orgProj.id);


                if (topFolderRes.data == null || topFolderRes.data.Count() == 0)
                {
                    Log.Warn("No top folders retrieved.");
                    return;
                }

                Log.Info("- retrieving sub-folders for 'Plans' and 'Project Files' folder. This could take a while..");
                // Iterate root folders
                foreach (Folder folder in topFolderRes.data)
                {
                    string folderName = folder.attributes.name;
                    if (folderName == "Project Files" || folderName == "Plans")
                    {
                        NestedFolder rootFolder = new NestedFolder(folderName, folder.id);

                        // recursive calls to fetch folder structure
                        NestedFolder rootWithChildrenFolder = _foldersApi.GetFoldersHierarchy(orgProj.id, rootFolder);
                        existingFolderStructure.Add(rootWithChildrenFolder);
                    }
                }
                folderStructures[orgProj.name] = existingFolderStructure;

            }
        }
        private HqUser GetAdminUser(string email)
        {
            return DataController.AccountUsers.Find(x => x.email == email);
        }
        private TopFolderResponse GetTopFolders(string projId)
        {
            IRestResponse response = _hubsApi.GetTopFolders(projId);
            TopFolderResponse content = JsonConvert.DeserializeObject<TopFolderResponse>(response.Content);

            return content;
        }
        private bool CreateProject(BimProject orgProj, BimProject newProj, out string newProjId)
        {
            newProjId = DataController.AddProject(newProj);
            if (newProjId == "error")
            {
                Log.Error("- error occured during project creation");
                return false;
            }
            Log.Info("- new project id: " + newProjId);
            return true;
        }
        private bool ActivateProject(string newProjId, HqUser admin, List<string> roleIds = null, string accountId = null)
        {
            Log.Info("");
            Log.Info("Project activation process started.");

            List<ProjectUser> users = new List<ProjectUser>();
            ProjectUser user = new ProjectUser();

            user.services.document_management = new DocumentManagement();
            user.services.document_management.access_level = AccessLevel.admin;

            user.services.project_administration = new ProjectAdministration();
            user.services.project_administration.access_level = AccessLevel.admin;

            user.industry_roles = roleIds;
            user.company_id = admin.company_id;
            user.email = admin.email;

            users.Add(user);

            IRestResponse res = _projectApi.PostUsersImport(newProjId, admin.uid, users, accountId);

            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ProjectUserResponse r = JsonConvert.DeserializeObject<ProjectUserResponse>(res.Content);
                if (r != null)
                {
                    if (r.failure_items.Count() > 0)
                    {
                        Log.Error($"- roject activation failed.");
                        return false;
                    }
                }

                // If the function is not used for the hub 2 hub copy
                if (accountId == null)
                {
                    DataController.AllProjects.Find(x => x.id == newProjId).status = Status.active;
                }
                Log.Info($"- project activation succeed.");
                return true;
            }
            Log.Error($"- project activation failed. Code: {res.StatusCode}\t message: {res.ErrorMessage}");
            return false;
        }
        private void CopyProjectFolders(BimProject orgProj, string newProjId, string uid)
        {
            Log.Info("");
            Log.Info("Project copy process started.");

            // Get root folders
            TopFolderResponse newTopFoldersRes = null;
            Log.Info("- retrieving top folders from " + newProjId);

            // quick fix for potential endless loop - add retry count as break condition 
            int retry = 0;

            do
            {
                do
                {
                    newTopFoldersRes = GetTopFolders(newProjId);
                    Thread.Sleep(2000);
                    retry++;
                    if (retry > 20) break;
                } while (newTopFoldersRes.data == null);
                if (retry > 20) break;
            } while (newTopFoldersRes.data.Where(x => x.attributes.name == "Project Files").ToList().Count < 1);

            if (retry > 20)
            {
                Log.Warn("Couldn't get top folders of template project. No folders are copied.");
                return;
            }

            // Iterate root folders and match the name with original project folders
            Log.Info("- copying folders(including folder permission of role) to the new project.");
            foreach (Folder newRootFolder in newTopFoldersRes.data)
            {
                if (folderStructures.ContainsKey(orgProj.name))
                {
                    NestedFolder existingRootFolder = folderStructures[orgProj.name].Find(x => x.name == newRootFolder.attributes.name);
                    if (existingRootFolder != null)
                    {
                        // Without below, access to the project is forbidden...
                        Thread.Sleep(3000);

                        // assign permission to root folder first
                        Log.Info("- assigning role permissions to root folder: " + newRootFolder.attributes.name);
                        bool res = _foldersApi.AssignPermission(newProjId, newRootFolder.id, existingRootFolder.permissions, uid);
                        if (!res)
                            Log.Warn($"Failed to assgn role permissions to root folder: {newRootFolder.attributes.name}.");
   
                        Log.Info("- copying the subfolders(including folder permission of role) of root folder: " + newRootFolder.attributes.name);
                        foreach (NestedFolder childFolder in existingRootFolder.childrenFolders)
                        {
                            // Recursively create new child folders
                            RecursivelyCreateFolder(newProjId, uid, newRootFolder.id, childFolder);
                        }
                    }
                }
            }
        }
        private void RecursivelyCreateFolder(string projId, string uid, string parentFolderId, NestedFolder newFolder)
        {
            Log.Info("-- copying child folder: " + newFolder.name);

            // Try to create folders until it is actually created
            string newFolderId = _foldersApi.CreateFolder(projId, uid, parentFolderId, newFolder.name);
            if (newFolderId == "error")
            {
                if (retryCounter < 10)
                {
                    retryCounter++;
                    Log.Warn($"Error occured while creating the folder: {newFolder.name}. Retrying ({retryCounter.ToString()}/10)");
                    RecursivelyCreateFolder(projId, uid, parentFolderId, newFolder);
                }
                else
                {
                    Log.Warn($"Retry attempt for creating folder has reached the limit. Couldn't copy all folders.");
                    return;
                }
            }
            else
            {
                retryCounter = 0; // Reset the counter
                Log.Info("-- assigning role permission to folder: " + newFolder.name);
                // assign permission to the new created folder
                bool res = _foldersApi.AssignPermission(projId, newFolderId, newFolder.permissions, uid );
                if( !res )
                    Log.Warn($"Failed to assgn role permissions to the new created folder: {newFolder.name}.");
          
                if (newFolder.childrenFolders.Count > 0)
                {
                    foreach (NestedFolder childFolder in newFolder.childrenFolders)
                    {
                        RecursivelyCreateFolder(projId, uid, newFolderId, childFolder);
                    }
                }
            }
        }
        private List<IndustryRole> GetRolesForProject(string projectName)
        {
            List<IndustryRole> result = null;
            if (false == _projectToRolesDict.TryGetValue(projectName, out result))
            {
                BimProject project = DataController.AllProjects.FirstOrDefault(p => p.name != null && p.name.Equals(projectName));
                if (project == null)
                {
                    throw new ApplicationException($"No projects found for name '{projectName}'");
                }
                result = DataController.GetProjectRoles(project.id);
                _projectToRolesDict.Add(projectName, result);
            }
            return result;
        }
        private List<string> GetIndustryRoleIds(string projectName, List<string> roles)
        {
            if (roles == null)
            {
                roles = new List<string>();
                roles.Add(_options.AdminRole); // Default: VDC Manager. For CLI.
            }

            List<IndustryRole> rolesFromProj = GetRolesForProject(projectName);
            if (rolesFromProj == null || rolesFromProj.Any() == false)
            {
                Log.Warn($"- Couldn't get any project industry roles for project '{projectName}'");
                return null;
            }

            List<string> roleIds = new List<string>();
            foreach (string role in roles)
            {
                string roleId = rolesFromProj.Find(x => x.name == role).id;
                if (role != null)
                {
                    roleIds.Add(roleId);
                }
            }

            return roleIds != null ? roleIds : null;
        }
        private List<string> GetIndustryRoleIds(BimProject proj, List<string> roles, string accountId)
        {
            BimProjectsApi _projectsApi = new BimProjectsApi(GetToken, _options);
            List<IndustryRole> rolesFromProj = new List<IndustryRole>();
            List<string> roleIds = new List<string>();

            if (roles == null)
            {
                roles = new List<string>();
                roles.Add(_options.AdminRole); // Default: VDC Manager. For CLI.
            }

            _projectsApi.GetIndustryRoles(proj.id, out rolesFromProj, accountId);

            if (rolesFromProj == null || rolesFromProj.Any() == false)
            {
                Log.Warn($"- Couldn't get any project industry roles for project '{proj.name}'");
                return null;
            }

            foreach (string role in roles)
            {
                string roleId = rolesFromProj.Find(x => x.name == role).id;
                if (role != null)
                {
                    roleIds.Add(roleId);
                }
            }

            return roleIds != null ? roleIds : null;
        }

        #region Copy project hub to hub
        public void CopyProjectToTargetHubProcess(BimProject orgProj, string targetAccountId, string adminEmail, List<string> roles = null)
        {
            Log.Info("");
            Log.Info("============================================================");
            Log.Info("HUB2HUB PROJECT COPY WORKFLOW started for " + orgProj.name);
            Log.Info("============================================================");
            Log.Info("");

            Log.Info($"Creating project by copying data from original project '{orgProj.name}'");

            List<string> roleIds = new List<string>();
            HqUser admin = new HqUser();

            // Get service activated in the original project
            if (orgProj.service_types == null || orgProj.service_types == "")
            {
                orgProj = DataController.GetProjectWithServiceById(orgProj.id);
                if (orgProj.service_types.Contains("insight"))
                {
                    orgProj.service_types = orgProj.service_types.Replace("insight,", "");
                }
            }

            // Check if target hub supports the services
            // Make sure the target hub is allowing same service types.

            // Create project to the target hub
            try
            {
                ExtractFolderStructure(orgProj);
                bool result = CreateProjectFromTargetHub(orgProj, targetAccountId, out string newProjId);
                if (result)
                {
                    IRestResponse res = _projectApi.GetProject(newProjId, targetAccountId);
                    BimProject newProj = DataController.HandleGetProjectResponse(res);
                    roleIds = GetIndustryRoleIds(newProj, roles, targetAccountId);
                    admin = GetAdminUserFromTargetHub(adminEmail, targetAccountId);

                    bool status = false;
                    do
                    {
                        status = ActivateProject(newProjId, admin, roleIds, targetAccountId);
                    } while (status == false);
                    if (status)
                    {
                        if (folderStructures.ContainsKey(orgProj.name))
                        {
                            CopyProjectFolders(orgProj, newProjId, admin.uid);
                        }
                    }
                }
                Log.Info("");
                Log.Info("HUB2HUB PROJECT COPY WORKFLOW finished for " + orgProj.name);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        private HqUser GetAdminUserFromTargetHub(string email, string accountId)
        {
            if (_options == null)
            {
                return null;
            }

            AccountApi _userApi = new AccountApi(GetToken, _options);
            List<HqUser> result = new List<HqUser>();
            _userApi.GetAccountUsers(out result, accountId);
            return result.Find(x => x.email == email);
        }
        private bool CreateProjectFromTargetHub(BimProject proj, string accountId, out string newProjId)
        {
            newProjId = DataController.AddProject(proj, accountId);
            if (newProjId == "error")
            {
                Log.Error("- error occured during project creation");
                return false;
            }
            Log.Info("- new project id: " + newProjId);
            return true;
        }
        #endregion

        #region Response Handler
        internal static bool ActivateProjectResponseHandler(IRestResponse response, string id, int rowIndex = -1)
        {
            LogResponse(response);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ProjectUserResponse r = JsonConvert.DeserializeObject<ProjectUserResponse>(response.Content);
                if (r != null)
                {
                    if (r.failure_items.Count() > 0)
                    {
                        Log.Error($"- roject activation failed.");
                        return false;
                    }
                }
                // Update data in the data controller
                DataController.AllProjects.Find(x => x.id == r.success_items[0].project_id).status = Status.active;
                Log.Info($"- project activation succeed.");
                return true;
            }
            Log.Error($"- project activation failed. Code: {response.StatusCode}\t message: {response.ErrorMessage}");
            return false;
        }
        internal static void LogResponse(IRestResponse response)
        {
            Log.Info($"- status code: {response.StatusCode}");
            if (response.ErrorException != null)
            {
                Log.Error($"- error message: {response.ErrorMessage}");
                Log.Error($"- error exception: {response.ErrorException}");
            }
        }
        #endregion
    }
}
