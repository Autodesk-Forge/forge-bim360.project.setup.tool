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
using System.Collections.Generic;
using System.Linq;
using System.Data;
using RestSharp;
using Newtonsoft.Json;
using Autodesk.Forge.BIM360;
using Autodesk.Forge.BIM360.Serialization;
using BimProjectSetupCommon.Helpers;

namespace BimProjectSetupCommon.Workflow
{
    public class ProjectUserWorkflow : BaseWorkflow
    {
        private BimProjectsApi _projectsApi = null;

        private Dictionary<string, List<IndustryRole>> _projectToRolesDict = new Dictionary<string, List<IndustryRole>>();
        private Dictionary<string, BimProject> _nameToProjectMap = new Dictionary<string, BimProject>();

        public ProjectUserWorkflow(AppOptions options) : base(options)
        {
            _projectsApi = new BimProjectsApi(GetToken, _options);
            DataController.InitializeAllProjects();
            DataController.InitializeAccountUsers();
        }

        public void AddProjectUsersFromCsvProcess()
        {
            try
            {
                DataController._projcetUserTable = CsvReader.ReadDataFromCSV(DataController._projcetUserTable, DataController._options.ProjectUserFilePath);

                if (false == _options.TrialRun)
                {
                    List<ProjectUser> _projectUsers = GetUsers(DataController._projcetUserTable);
                    AddUsers(_projectUsers);
                }
                else
                {
                    Log.Info("-Trial run (-r option is true) - no further processing");
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        public void AddProjectUsersProcess(List<ProjectUser> _projectUsers)
        {
            try
            {
                if (_options.TrialRun)
                {
                    Log.Info("-Trial run (-r option is true) - no further processing");
                    return;
                }

                foreach (ProjectUser user in _projectUsers)
                {
                    if (user.project_name == null)
                    {
                        return;
                    }
                    user.pm_access = GetAccessLevel(user.pm_access);
                    user.docs_access = GetAccessLevel(user.docs_access);
                    if (user.company_id == null)
                    {
                        user.company_id = GetCompanyId(user.company);
                    }

                    user.industry_roles = GetIndustryRoleIds(user.project_name, user.industry_roles);
                    AddServices(user);
                }

                AddUsers(_projectUsers);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public void UpdateProjectUsersFromCsvProcess()
        {
            try
            {
                DataController._projcetUserTable = CsvReader.ReadDataFromCSV(DataController._projcetUserTable, DataController._options.ProjectUserFilePath);

                if (false == _options.TrialRun)
                {
                    List<ProjectUser> _projectUsers = GetUsers(DataController._projcetUserTable);
                    PatchUser(_projectUsers);
                }
                else
                {
                    Log.Info("-Trial run (-r option is true) - no further processing");
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void AddUsers(List<ProjectUser> _projectUsers)
        {
            if (_projectUsers == null || _projectUsers.Count < 1)
            {
                return;
            }

            HqUser hqAdmin = new HqUser();
            if (!CheckAdminEmail(_options.HqAdmin, out hqAdmin))
            {
                return;
            }

            Log.Info($"");
            Log.Info($"Start adding users to projects");
            var projectNames = _projectUsers.Select(u => u.project_name).Distinct();
            foreach (string name in projectNames)
            {
                Log.Info($"- add users to project {name}");
                var users = _projectUsers.Where(u => u.project_name.Equals(name));
                if (users == null || users.Any() == false)
                {
                    Log.Warn($"- no valid users found for project {name} - skipping this project and continue with next");
                    continue;
                }
                var project = DataController.AllProjects.FirstOrDefault(p => p.name != null && p.name.Equals(name));

                if (users.Count() < 50)
                {
                    IRestResponse response = _projectsApi.PostUsersImport(project.id, hqAdmin.uid, users.ToList());
                    ProjectUserResponseHandler(response);
                }
                else if (users.Count() >= 50)
                {
                    List<IRestResponse> responses = new List<IRestResponse>();
                    IEnumerable<List<ProjectUser>> chunks = Util.SplitList(users.ToList());
                    foreach (List<ProjectUser> list in chunks)
                    {
                        IRestResponse response = _projectsApi.PostUsersImport(project.id, hqAdmin.uid, list);
                        ProjectUserResponseHandler(response);
                    }
                }
            }
        }

        private void PatchUser(List<ProjectUser> _projectUsers)
        {
            if (_projectUsers == null || _projectUsers.Count < 1)
            {
                return;
            }

            HqUser hqAdmin = new HqUser();
            if (!CheckAdminEmail(_options.HqAdmin, out hqAdmin))
            {
                return;
            }

            Log.Info($"");
            Log.Info($"Start updating users in projects");
            var projectNames = _projectUsers.Select(u => u.project_name).Distinct();
            foreach (string name in projectNames)
            {
                Log.Info($"- update users to project {name}");
                var users = _projectUsers.Where(u => u.project_name.Equals(name));
                if (users == null || users.Any() == false)
                {
                    Log.Warn($"- no valid users found for project {name} - skipping this project and continue with next");
                    continue;
                }
                var project = DataController.AllProjects.FirstOrDefault(p => p.name != null && p.name.Equals(name));
                foreach (ProjectUser projectUser in users)
                {
                    HqUser hqUser = new HqUser();
                    if (CheckUserId(projectUser.email, out hqUser))
                    {
                        Log.Info($"- updating user {projectUser.email}");
                        IRestResponse response = _projectsApi.PatchUser(project.id, hqAdmin.uid, hqUser.id, projectUser);
                        ProjectUserPatchResponseHandler(response);
                    }
                    else
                    {
                        Log.Error($"User {projectUser.email} not in account. Add them to a project first.");
                    }
                }
            }
        }
        private static void AddServices(ProjectUser user)
        {
            // normal user
            user.services = new Services();
            user.services.document_management = new DocumentManagement() { access_level = AccessLevel.user };

            // admin user?
            if (user.docs_access != null && user.docs_access.Equals("admin", StringComparison.InvariantCultureIgnoreCase))
            {
                user.services.document_management = new DocumentManagement() { access_level = AccessLevel.admin };
            }
            if (user.pm_access != null && user.pm_access.Equals("admin", StringComparison.InvariantCultureIgnoreCase))
            {
                user.services.project_administration = new ProjectAdministration() { access_level = AccessLevel.admin };
            }
        }
        private static string GetCompanyId(object v)
        {
            string compId = null;
            string compName = Convert.ToString(v);
            if (string.IsNullOrEmpty(compName) == false)
            {
                IEnumerable<BimCompany> comps = DataController.Companies.Where(c => c.name.Equals(compName));
                if (comps != null && comps.Any())
                {
                    if (comps.Count() > 1)
                    {
                        throw new ApplicationException($"Multiple companies found for name {compName}. Cannot identify company id!");
                    }
                    if (comps.Count() == 1)
                    {
                        compId = comps.First().id;
                    }
                }
                else
                {
                    throw new ApplicationException($"No company ID could be found for company name {compName}");
                }
            }
            return compId;
        }
        private List<ProjectUser> GetUsers(DataTable table)
        {
            if (table == null)
            {
                return null;
            }

            // sort data table by project_name
            DataView view = table.DefaultView;
            view.Sort = "project_name desc";
            DataTable sorted = view.ToTable();

            List<ProjectUser> users = new List<ProjectUser>();
            int i = 0;

            // Validate the data and convert
            foreach (DataRow row in sorted.Rows)
            {
                i++;
                string projectName = Util.GetStringOrNull(row["project_name"]);

                if (string.IsNullOrWhiteSpace(projectName))
                {
                    Log.Warn($"No project name provided for row {i} - skipping this line!");
                    continue;
                }

                var user = GetUserForRow(row, projectName);
                if (user != null) users.Add(user);
            }
            return users;
        }

        private ProjectUser GetUserForRow(DataRow row, string projectName)
        {
            ProjectUser user = new ProjectUser();

            user.project_name = projectName;
            user.email = Util.GetStringOrNull(row["email"]);
            user.pm_access = GetAccessLevel(row["pm_access"]);
            user.docs_access = GetAccessLevel(row["docs_access"]);
            user.company_id = GetCompanyId(row["company_name"]);
            user.industry_roles = GetIndustryRoleIds(projectName, row["industry_roles"]);
            AddServices(user);

            if (string.IsNullOrWhiteSpace(user.email))
            {
                throw new ApplicationException($"No email available for user - check CSV files!");
            }
            return user;
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
        private List<string> GetIndustryRoleIds(string projectName, List<string> userRoles)
        {
            List<IndustryRole> roles = GetRolesForProject(projectName);
            if (roles == null || roles.Any() == false)
            {
                Log.Warn($"Couldn't get any project industry roles for project '{projectName}'");
            }

            List<string> result = new List<string>();
            foreach (string userRole in userRoles)
            {
                IndustryRole role = roles.Find(x => x.name == userRole);
                if (role != null)
                {
                    result.Add(role.id);
                }

            }

            return result != null ? result : null;
        }
        private List<string> GetIndustryRoleIds(string projectName, object userRoles)
        {
            List<IndustryRole> roles = GetRolesForProject(projectName);
            if (roles == null || roles.Any() == false)
            {
                Log.Warn($"Couldn't get any project industry roles for project '{projectName}'");
            }

            List<string> result = new List<string>();
            string s = Convert.ToString(userRoles);
            if (roles != null && false == string.IsNullOrWhiteSpace(s))
            {
                string[] roleNames = Array.ConvertAll(s.Split(','), p => p.Trim());
                foreach (string name in roleNames)
                {
                    string n = name.Trim();
                    IndustryRole role = roles.FirstOrDefault(r => r.name != null && r.name.Equals(name));
                    if (role != null)
                    {
                        result.Add(role.id);
                    }
                }
            }

            return result != null ? result : null;
        }
        private static string GetAccessLevel(object v)
        {
            string s = Util.GetStringOrNull(v);
            if (s != null)
            {
                s = s.ToLower();
                if (!s.Equals("admin") && !s.Equals("user"))
                {
                    throw new ApplicationException($"Invalid input data for access provided: '{s}' Only allowed values: 'admin' or 'user' ");
                }
            }
            return s;
        }
        private bool CheckAdminEmail(string adminEmail, out HqUser HqAdmin)
        {
            HqAdmin = DataController.AccountUsers.FirstOrDefault(u => u.email != null && u.email.Equals(adminEmail, StringComparison.InvariantCultureIgnoreCase));
            if (HqAdmin == null)
            {
                Log.Error($"Error initializing account admin user {adminEmail}");
                return false;
            }
            return true;
        }

        private bool CheckUserId(string email, out HqUser HqUser)
        {
            HqUser = DataController.AccountUsers.FirstOrDefault(u =>
                u.email != null && u.email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            if (HqUser == null)
            {
                Log.Error($"Error initializing account user {email}");
                return false;
            }

            return true;
        }

        #region Response Handler
        internal static void ProjectUsersResponseHandler(List<IRestResponse> responses)
        {
            foreach (IRestResponse response in responses) ProjectUserResponseHandler(response);
        }
        internal static void ProjectUserResponseHandler(IRestResponse response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ProjectUserResponse r = JsonConvert.DeserializeObject<ProjectUserResponse>(response.Content);
                if (r != null)
                {
                    Log.Info($"Success: {r.success}");
                    Log.Error($"Failures: {r.failure}");
                    foreach (ProjectUserFailureItem f in r.failure_items)
                    {
                        Log.Error($"Errors for user with email {f.email}:");
                        foreach (var e in f.errors)
                        {
                            Log.Error($"code:{e.code}  message:{e.message}");
                        }
                    }

                    foreach (ProjectUserSuccessItem si in r.success_items)
                    {
                        if (si.status == Status.active)
                        {
                            DataController.AllProjects.FirstOrDefault(p => p.id == si.project_id).status = Status.active;
                        }
                    }
                }
            }
            else
            {
                LogResponse(response);
            }
        }
        internal static void ProjectUserPatchResponseHandler(IRestResponse response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ProjectUserPatchResponse r = JsonConvert.DeserializeObject<ProjectUserPatchResponse>(response.Content);
                if (r != null)
                {
                    if (r.error == null)
                    {
                        Log.Info($"Successfully updated user: {r.email}");
                    }
                    else
                    {
                        Log.Error($"Error when updating project user");
                        foreach (var e in r.error)
                        {
                            Log.Error($"code:{e.code}  message:{e.message}");
                        }
                    }
                }
            }
            else
            {
                LogResponse(response);
            }
        }
        internal static void HandleError(Exception e)
        {
            Log.Error(e.Message);
            Log.Error(e);
        }
        internal static void LogResponse(IRestResponse response)
        {
            Log.Info($"- status Code: {response.StatusCode}");
            if (response.ErrorException != null)
            {
                Log.Error($"- error Message: {response.ErrorMessage}");
                Log.Error($"- error Exception: {response.ErrorException}");
            }
        }
        #endregion

        #region CSV Export
        public void ExportUsersCsv()
        {
            CsvExporter.ExportUsersCsvTemplate();
        }
        #endregion
    }
}
