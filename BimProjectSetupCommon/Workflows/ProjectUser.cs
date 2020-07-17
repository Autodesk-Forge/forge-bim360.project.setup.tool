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
using System.Windows.Forms;

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
        public List<HqUser> CustomGetAllProjectUsers(string projectId)
        {
            List<HqUser> result = new List<HqUser>();
            _projectsApi.CustomGetProjectUsers(out result, projectId);

            return result;
        }
        public List<HqUser> CustomUpdateProjectUsers(DataTable table, int startRow, List<BimCompany> companies, BimProject project, ProjectUserWorkflow projectUserProcess)
        {
            Util.LogInfo("\nRetrieving existing members from project.");
            List<HqUser>  projectUsers = projectUserProcess.CustomGetAllProjectUsers(project.id);

            Util.LogInfo("Adding members to project.");
            List<ProjectUser> _projectUsers = CustomGetUsers(table, projectUsers, companies, project.name, startRow);
            AddUsers(_projectUsers);

            // Sometimes the users are not fully added and no permission could be added
            if (_projectUsers.Count > 0)
            {
                for (int i = 3; i > 0; i--)
                {
                    Util.LogInfo($"Waiting for all users to be fully added... Time remaining: {i * 5} seconds...");
                    System.Threading.Thread.Sleep(5000);
                }
            }

            List<HqUser> updatedProjectUsers = projectUserProcess.CustomGetAllProjectUsers(project.id);
            return updatedProjectUsers;
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

        private static void CustomAddServicesFromIndustryRole(ProjectUser user, List<IndustryRole> roles)
        {
            user.services = new Services();
            foreach(IndustryRole role in roles)
            {
                if (role.services.document_management.access_level == RolesAccessLevel.user && user.services.document_management == null)
                {
                    user.services.document_management = new DocumentManagement() { access_level = AccessLevel.user };
                }

                if (role.services.project_administration.access_level == RolesAccessLevel.user && user.services.project_administration == null)
                {
                    user.services.project_administration = new ProjectAdministration() { access_level = AccessLevel.user };
                }

                if (role.services.document_management.access_level == RolesAccessLevel.admin)
                {
                    user.services.document_management = new DocumentManagement() { access_level = AccessLevel.admin };
                }

                if (role.services.project_administration.access_level == RolesAccessLevel.admin)
                {
                    user.services.project_administration = new ProjectAdministration() { access_level = AccessLevel.admin };
                }
            }
            
            if (user.services.project_administration != null && user.services.document_management != null)
            {
                if (user.services.project_administration.access_level == AccessLevel.admin && user.services.document_management.access_level == AccessLevel.user)
                {
                    Util.LogImportant($"Not possible to have admin access to 'Project Admin' and at the same time user access to 'Document Management'. " +
                        $"Document management access for user '{user.email}' upgraded to admin.");
                    user.services.document_management = new DocumentManagement() { access_level = AccessLevel.admin };
                }
            }

            if (user.services.project_administration == null && user.services.document_management != null)
            {
                if (user.services.document_management.access_level == AccessLevel.admin)
                {
                    Util.LogImportant($"Not possible to have admin access to 'Document Management' without having admin access to 'Project Admin'. " +
                    $"'Document Management' access for user {user.email} downgraded to user.");
                }
                
            }

            if (user.services.document_management == null && user.services.project_administration == null)
            {
                Util.LogImportant($"No document_management or project_administration for role. Default access_level for user '{user.email}': document_management: user");
                user.services.document_management = new DocumentManagement() { access_level = AccessLevel.user };
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
        private List<ProjectUser> CustomGetUsers(DataTable table, List<HqUser> projectUsers, List<BimCompany> companies, string projectName, int startRow)
        {
            List<ProjectUser> resultUsers = new List<ProjectUser>();

            List<string> allUserEmailsInCsv = new List<string>();
            List<string> existingUsers = GetUserEmails(projectUsers);

            List<string> existingCompanies = GetCompanyNames(companies);
            existingCompanies = existingCompanies.ConvertAll(d => d.ToLower());

            for (int row = startRow; row < table.Rows.Count; row++)
            {
                // Itterate until next project
                if (!string.IsNullOrEmpty(table.Rows[row]["project_name"].ToString()) && row != startRow)
                {
                    break;
                }
                // Continue if no user at this row
                if (string.IsNullOrEmpty(table.Rows[row]["user_email"].ToString()))
                {
                    continue;
                }

                // Check if user with same email already in csv
                if (!string.IsNullOrEmpty(table.Rows[row]["user_email"].ToString()) && allUserEmailsInCsv.Contains(table.Rows[row]["user_email"].ToString()))
                {
                    // Check if user with same email but with multiple rows with 'industry_role' or 'company'
                    if (!string.IsNullOrEmpty(table.Rows[row]["industry_role"].ToString()))
                    {
                        Util.LogImportant($"User with email '{table.Rows[row]["user_email"]}' already exists with other " +
                            $"'industry_role' values. Only the first values for each user will be taken. See row number {row + 2} in the CSV-File.");
                    }

                    // Check if user with same email but with multiple rows with 'industry_role' or 'company'
                    if (!string.IsNullOrEmpty(table.Rows[row]["company"].ToString()))
                    {
                        Util.LogImportant($"User with email '{table.Rows[row]["user_email"]}' already exists with other " +
                            $"'company' values. Only the first values for each user will be taken. See row number {row + 2} in the CSV-File.");
                    }
                }

                if (!allUserEmailsInCsv.Contains(table.Rows[row]["user_email"].ToString()))
                {
                    allUserEmailsInCsv.Add(table.Rows[row]["user_email"].ToString());
                }

                // Continue if user with the same email already exists
                if (!string.IsNullOrEmpty(table.Rows[row]["user_email"].ToString()) && existingUsers.Contains(table.Rows[row]["user_email"].ToString()))
                {
                    continue;
                }

                // Check if company exists
                string companyName = table.Rows[row]["company"].ToString();
                if (!string.IsNullOrEmpty(companyName) && !existingCompanies.Contains(companyName.ToLower()))
                {
                    Util.LogImportant($"Something went wrong. Company with name: "
                        + companyName + $" was not found. User with email: {table.Rows[row]["user_email"]} should be assigned to this company.");
                }

                BimCompany company = companies.Find(x => x.name.ToLower() == companyName.ToLower());

                // Check if user with same email has already been added
                bool isUserAdded = false;
                ProjectUser projectUser = resultUsers.Find(x => x.email == table.Rows[row]["user_email"].ToString());
                if(projectUser != null)
                {
                    isUserAdded = true;
                    continue;
                }

                ProjectUser user = CustomGetUserForRow(table.Rows[row], projectName, company);

                // Add only if user had not been added already
                if (user != null && !isUserAdded) resultUsers.Add(user);
            }
            return resultUsers;
        }

        private List<string> GetUserEmails(List<HqUser> projectUsers)
        {
            List<string> existingUsers = new List<string>();
            foreach (HqUser existingUser in projectUsers)
            {
                existingUsers.Add(existingUser.email);
            }

            return existingUsers;
        }

        private List<string> GetCompanyNames(List<BimCompany> companies)
        {
            List<string> existingCompanies = new List<string>();
            foreach (BimCompany existingCompany in companies)
            {
                existingCompanies.Add(existingCompany.name);
            }

            return existingCompanies;
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
        private ProjectUser CustomGetUserForRow(DataRow row, string projectName, BimCompany company)
        {
            
            ProjectUser user = new ProjectUser();

            user.project_name = projectName;
            user.email = Util.GetStringOrNull(row["user_email"]);
            if(company != null)
            {
                user.company_id = company.id;
            }
            else
            {
                Util.LogImportant($"No company assigned to user with email: {user.email}.");
            }

            List<IndustryRole> currRole = new List<IndustryRole>();

            if (Util.GetStringOrNull(row["industry_role"]) != null)
            {
                user.industry_roles = GetIndustryRoleIds(projectName, Util.GetStringOrNull(row["industry_role"]));
                List<IndustryRole> roles = GetRolesForProject(projectName);

                List<string> industryRoles = GetIndustryRoleNames(projectName, Util.GetStringOrNull(row["industry_role"]));
                foreach(string userRole in industryRoles)
                {
                    IndustryRole role = roles.Find(x => x.name.ToLower() == userRole.ToLower());
                    if (role != null)
                    {
                        currRole.Add(role);
                    }
                    else
                    {
                        Util.LogImportant($"Industry role '{userRole}' was not found in project! Skipping this role for user '{user.email}'.");
                    }
                }   
            }

            CustomAddServicesFromIndustryRole(user, currRole);

            if (string.IsNullOrWhiteSpace(user.email))
            {
                Util.LogImportant($"No email available for user - something went wrong!");
            }
            return user;
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
        public List<IndustryRole> GetRolesForProject(string projectName)
        {
            List<IndustryRole> result = null;
            if (false == _projectToRolesDict.TryGetValue(projectName, out result))
            {
                BimProject project = DataController.AllProjects.FirstOrDefault(p => p.name != null && p.name.Equals(projectName));
                if (project == null)
                {
                    throw new ApplicationException($"No project found for name '{projectName}'");
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
        private List<string> GetIndustryRoleNames(string projectName, object userRoles)
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
                    result.Add(name);
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
