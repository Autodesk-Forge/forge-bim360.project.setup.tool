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
using System.Collections.Generic;
using System.Threading;

using NLog;
using System.Linq;
using RestSharp;
using Newtonsoft.Json;

using BimProjectSetupCommon.Helpers;
using Autodesk.Forge.BIM360;
using Autodesk.Forge.BIM360.Serialization;
using System.Reflection;

namespace BimProjectSetupCommon
{
    /// <summary>
    /// Static class that controls and stores data from CSV file and BIM 360
    /// </summary>
    internal static class DataController
    {
        internal static AppOptions _options = null;

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private static string _token = null;
        private static DateTime StartAuth { get; set; }

        #region DataTables
        public static DataTable _projectTable;
        public static DataTable _serviceTable;
        public static DataTable _projcetUserTable;
        public static DataTable _accountUserTable;
        public static DataTable _companyTable;
        #endregion

        #region Properties
        private static List<BimProject> _AllProjects = null;
        public static List<BimProject> AllProjects
        {
            get
            {
                InitializeAllProjects();
                return _AllProjects;
            }
        }
        private static List<BimCompany> _Companies = null;
        public static List<BimCompany> Companies
        {
            get
            {
                InitializeCompanies();
                return _Companies.OrderBy(x => x.name).ToList();
            }
        }
        private static List<HqUser> _AccountUsers = null;
        public static List<HqUser> AccountUsers
        {
            get
            {
                InitializeAccountUsers();
                if (_AccountUsers != null) _AccountUsers = _AccountUsers.OrderBy(x => x.full_name).ToList();
                return _AccountUsers;
            }
        }
        private static List<BusinessUnit> _BusinessUnits = null;
        public static List<BusinessUnit> BusinessUnits
        {
            get
            {
                if (_BusinessUnits == null || _BusinessUnits.Count < 1)
                {
                    _BusinessUnits = GetBusinessUnits();
                }
                return _BusinessUnits;
            }
        }
        private static List<Hub> _Hubs = null;
        public static List<Hub> Hubs
        {
            get
            {
                if (_Hubs == null || _Hubs.Count < 1)
                {
                    _Hubs = GetHubs();
                }
                return _Hubs;
            }
        }
        #endregion

        #region Methods
        public static BimProject GetProjectWithServiceById(string projectId)
        {
            BimProject result = null;
            result = GetProject(projectId);
            return (result != null ? result : null);
        }
        public static BimProject GetProjectById(string projectId)
        {
            BimProject result = null;
            result = AllProjects.FirstOrDefault(p => p.id.Equals(projectId));
            return (result != null ? result : null);
        }
        public static string GetProjectIdByName(string projectName)
        {
            BimProject result = null;
            result = AllProjects.FirstOrDefault(p => p.name != null && p.name.Equals(projectName));
            return (result != null ? result.id : null);
        }
        #endregion

        #region Data Initializer
        internal static void InitializeAllProjects()
        {
            if (_AllProjects == null || _AllProjects.Count < 1)
            {
                _AllProjects = GetProjects();
            }
        }
        internal static void InitializeCompanies()
        {
            if (_Companies == null || _Companies.Count < 1)
            {
                _Companies = GetCompanies();
            }
        }
        internal static void InitializeAccountUsers()
        {
            if (_AccountUsers == null || _AccountUsers.Count < 1)
            {
                _AccountUsers = GetAccountUsers();
            }
        }
        internal static void InitializeHubs()
        {
            if (_Hubs == null || _Hubs.Count < 1)
            {
                _Hubs = GetHubs();
            }
        }
        #endregion

        #region Convert tables to objects
        public static List<BimProject> GetBimProjects()
        {
            List<BimProject> projects = new List<BimProject>();
            int i = 1;

            foreach (DataRow row in _projectTable.Rows)
            {
                i++;
                BimProject project = new BimProject();
                try
                {
                    string name = Util.GetStringOrNull(row["name"]);
                    if (false == Util.IsNameValid(name))
                    {
                        Log.Error($"- row {i} skipped. name is missing or invalid.");
                        continue;
                    }
                    project.name = name;
                    /// Code to use existing project as a template(only works for BIM 360 field related data)
                    //string template_project_name = Utility.GetStringOrNull(row["template_project_name"]);
                    //if (null != template_project_name)
                    //{
                    //    // Check if the template project exists, and get the project id
                    //    TempContainer._template_project = TempContainer._projectsOfAccount.FirstOrDefault(p => p.name.Equals(template_project_name));
                    //    if (null != TempContainer._template_project)
                    //    {
                    //        // If it exists, use it as a teamplate
                    //        project.template_project_id = TempContainer._template_project.id;

                    //        // Check if two parameters(include_locations, include_companies) are also set
                    //        if (row["include_locations"].ToString() == "true")
                    //            project.include_locations = true;
                    //        else
                    //            project.include_locations = false;
                    //        if (row["include_companies"].ToString() == "true")
                    //            project.include_companies = true;
                    //        else
                    //            project.include_companies = false;
                    //    }
                    //}

                    project.service_types = Util.GetStringOrNull(row["service_types"]);
                    // Don't use service type in project creation, because this will restrict service activation via API
                    //project.service_types = "doc_manager";
                    DateTime? date = Util.GetDate(row["start_date"]); //new DateTime(2017, 11, 23);
                    if (date.HasValue) { project.start_date = date.Value; }
                    else
                    {
                        Log.Error($"- row {i} skipped. No parsable start_date provided. Check if field is empty and if it machtes the date pattern (-d option)");
                        continue;
                    }
                    DateTime? endDate = Util.GetDate(row["end_date"]);
                    if (endDate.HasValue) { project.end_date = endDate.Value; }
                    else
                    {
                        Log.Error($"- row {i} skipped. No parsable end_date provided. Check if field is empty and if it machtes the date pattern (-d option)");
                        continue;
                    }
                    project.project_type = Util.GetStringOrNull(row["project_type"]);
                    if (string.IsNullOrWhiteSpace(project.project_type))
                    {
                        Log.Error($"- row {i} skipped. Required column 'project_type' has not value.");
                        continue;
                    }
                    project.value = Util.GetStringOrNull(row["value"]);
                    if (string.IsNullOrWhiteSpace(project.value))
                    {
                        Log.Error($"- row {i} skipped. Required column 'value' has no value.");
                        continue;
                    }
                    project.currency = Util.GetStringOrNull(row["currency"]);
                    if (string.IsNullOrWhiteSpace(project.currency))
                    {
                        Log.Error($"- row {i} skipped. Required column 'currency' has not value.");
                        continue;
                    }
                    project.job_number = Util.GetStringOrNull(row["job_number"]);
                    project.address_line_1 = Util.GetStringOrNull(row["address_line_1"]);
                    project.address_line_2 = Util.GetStringOrNull(row["address_line_2"]);
                    project.city = Util.GetStringOrNull(row["city"]);
                    if (row.Table.Columns.Contains("state_or_province"))
                    {
                        project.state_or_province = Util.GetStringOrNull(row["state_or_province"]);
                    }
                    if (row.Table.Columns.Contains("province"))
                    {
                        project.state_or_province = Util.GetStringOrNull(row["province"]);
                    }
                    project.postal_code = Util.GetStringOrNull(row["postal_code"]);
                    project.country = Util.GetStringOrNull(row["country"]);
                    if (row.Table.Columns.Contains("business_unit_id"))
                    {
                        project.business_unit_id = GetBusinessUnitId(row["business_unit_id"]);
                    }
                    project.timezone = Util.GetStringOrNull(row["timezone"]);
                    if (row.Table.Columns.Contains("language"))
                    {
                        project.language = Util.GetStringOrNull(row["language"]);
                    }
                    project.construction_type = Util.GetStringOrNull(row["construction_type"]);
                    project.contract_type = Util.GetStringOrNull(row["contract_type"]);
                    project.id = Util.GetStringOrNull(row["id"]);
                    project.last_sign_in = Util.GetStringOrNull(row["last_sign_in"]);
                    project.folders_created_from = Util.GetStringOrNull(row["folders_created_from"]);
                }
                catch (Exception e)
                {
                    row["result"] = ResultCodes.ErrorParsing;
                    row["result_message"] = e.Message;
                    Log.Error(e, $"- error parsing row {i}");
                    continue;
                }
                projects.Add(project);
            }
            return projects;
        }
        internal static List<string> GetIndustryRoles()
        {
            List<string> industryRoles = new List<string>();
            industryRoles.Add("Engineer");
            industryRoles.Add("Architect");
            industryRoles.Add("BIM Manager");
            industryRoles.Add("Civil Engineer");
            industryRoles.Add("Commercial Manager");
            industryRoles.Add("Construction Manager");
            industryRoles.Add("Contract Manager");
            industryRoles.Add("Contractor");
            industryRoles.Add("Cost Engineer");
            industryRoles.Add("Cost Manager");
            industryRoles.Add("Designer");
            industryRoles.Add("Document Manager");
            industryRoles.Add("Drafter");
            industryRoles.Add("Electrical Engineer");
            industryRoles.Add("Estimator");
            industryRoles.Add("Executive");
            industryRoles.Add("Fire Safety Engineer");
            industryRoles.Add("Foreman");
            industryRoles.Add("HVAC Engineer");
            industryRoles.Add("Inspector");
            industryRoles.Add("Interior Designer");
            industryRoles.Add("IT");
            industryRoles.Add("Mechanical Engineer");
            industryRoles.Add("Owner");
            industryRoles.Add("Plumbing Engineer");
            industryRoles.Add("Project Engineer");
            industryRoles.Add("Project Manager");
            industryRoles.Add("Quality Manager");
            industryRoles.Add("Quantity Surveyor");
            industryRoles.Add("Safety Manager");
            industryRoles.Add("Scheduler");
            industryRoles.Add("Structural Engineer");
            industryRoles.Add("Subcontractor");
            industryRoles.Add("Superintendent");
            industryRoles.Add("Surveyor");
            industryRoles.Add("VDC Manager");

            return industryRoles;
        }
        public static Dictionary<int, ServiceActivation> GetServiceActivations()
        {
            Dictionary<int, ServiceActivation> serviceActivations = new Dictionary<int, ServiceActivation>();
            int i = 0;

            foreach (DataRow row in _serviceTable.Rows)
            {
                i++;
                // skip empty rows
                if (string.IsNullOrWhiteSpace(Convert.ToString(row))) continue;

                ServiceActivation service = new ServiceActivation();
                try
                {
                    service.project_name = Util.GetStringOrNull(row["project_name"]);
                    if (false == Util.IsNameValid(service.project_name))
                    {
                        Log.Error($"- row {i} skipped. project_name is missing or invalid.");
                        continue;
                    }
                    service.service_type = Util.GetStringOrNull(row["service_type"]);

                    if (string.IsNullOrWhiteSpace(service.service_type))
                    {
                        Log.Warn($"- row {i} skipped. service_type is missing.");
                        continue;
                    }
                    //else if (service.service_type.Contains(Config.secondDelimiter.ToString()))
                    //{
                    //    // Replace service_type delimeter to comma
                    //    service.service_type = service.service_type.Replace(Config.secondDelimiter, ',');
                    //}
                    service.email = Util.GetStringOrNull(row["email"]);
                    if (string.IsNullOrWhiteSpace(service.email))
                    {
                        Log.Warn($"- row {i} skipped. email is missing.");
                        continue;
                    }
                    service.company = Util.GetStringOrNull(row["company"]);
                    if (string.IsNullOrWhiteSpace(service.company))
                    {
                        Log.Warn($"- row {i} skipped. company is missing.");
                        continue;
                    }

                    BimCompany comp = Companies.FirstOrDefault(c => c.name != null && c.name.Equals(service.company));
                    if (comp != null) service.company_id = comp.id;
                }
                catch (Exception e)
                {
                    row["result"] = ResultCodes.ErrorParsing;
                    row["result_message"] = e.Message;
                    Log.Error(e, $"- error parsing row {i}");
                    continue;
                }
                serviceActivations.Add(i - 1, service);

            }
            return serviceActivations;
        }
        private static string GetBusinessUnitId(object bu)
        {
            ///TODO: this catches the first ID always, although the same name can be used multiple times in the structure
            /// need to find a delimier character "/" and "\" can be part of BU names...
            string buName = Convert.ToString(bu);
            string result = null;
            if (bu != null && BusinessUnits != null && BusinessUnits.Count > 0)
            {
                BusinessUnit unit = BusinessUnits.FirstOrDefault(b => b.name != null && b.name.Equals(buName, StringComparison.InvariantCultureIgnoreCase));
                if (unit != null) result = unit.id;
            }
            return result;
        }
        #endregion

        #region Forge API calls
        private static string GetToken()
        {
            if (_token == null || ((DateTime.Now - StartAuth) > TimeSpan.FromMinutes(30)))
            {
                _token = Authentication.Authenticate(_options);
                StartAuth = DateTime.Now;
                return _token;
            }
            else return _token;
        }
        private static List<BimProject> GetProjects( string sortProp = "updated_at", int limit = 100, int offset = 0)
        {
            if (_options == null)
            {
                return null;
            }

            BimProjectsApi _projectsApi = new BimProjectsApi(GetToken, _options);
            List<BimProject> projects = new List<BimProject>();
            _projectsApi.GetProjects(out projects, sortProp, limit, offset);
            return projects;
        }
        private static BimProject GetProject(string projId)
        {
            if (_options == null)
            {
                return null;
            }
            BimProjectsApi _projectsApi = new BimProjectsApi(GetToken, _options);
            IRestResponse response = _projectsApi.GetProject(projId);
            return HandleGetProjectResponse(response);
        }
        private static List<HqUser> GetAccountUsers()
        {
            if (_options == null)
            {
                return null;
            }

            AccountApi _userApi = new AccountApi(GetToken, _options);
            List<HqUser> result = new List<HqUser>();
            _userApi.GetAccountUsers(out result);
            return result;
        }
        internal static List<HqUser> GetAccountUsers(string accountId)
        {
            if (_options == null)
            {
                return null;
            }

            AccountApi _userApi = new AccountApi(GetToken, _options);
            List<HqUser> result = new List<HqUser>();
            _userApi.GetAccountUsers(out result, accountId);
            return result;
        }
        private static List<BimCompany> GetCompanies()
        {
            if (_options == null)
            {
                return null;
            }

            AccountApi _compApi = new AccountApi(GetToken, _options);
            List<BimCompany> result = new List<BimCompany>();
            _compApi.GetCompanies(out result);
            return result;
        }
        private static List<BusinessUnit> GetBusinessUnits()
        {
            if (_options == null)
            {
                return null;
            }

            AccountApi _businessUnitsApi = new AccountApi(GetToken, _options);
            List<BusinessUnit> result = new List<BusinessUnit>();
            _businessUnitsApi.GetBusinessUnits(out result);
            return result;
        }
        private static List<Hub> GetHubs()
        {
            if (_options == null)
            {
                return null;
            }

            HubsApi _hubsApi = new HubsApi(GetToken, _options);
            IRestResponse response = _hubsApi.GetHubs();
            List<Hub> result = HandleGetHubsResponse(response);
            return result;
        }
        public static List<IndustryRole> GetProjectRoles(string projId)
        {
            if (projId == null)
            {
                Log.Error("TempContainer.GetProjectRoles(): Project Id not provided.");
                return null;
            }
            if (_options == null)
            {
                Log.Error("TempContainer.GetProjectRoles(): Option not provided.");
                return null;
            }

            BimProjectsApi _projectsApi = new BimProjectsApi(GetToken, _options);
            List<IndustryRole> roles = new List<IndustryRole>();
            _projectsApi.GetIndustryRoles(projId, out roles);
            return roles;
        }
        public static List<IndustryRole> GetProjectRoles(string projId, string accountId)
        {
            if (projId == null)
            {
                Log.Error("TempContainer.GetProjectRoles(): Project Id not provided.");
                return null;
            }
            if (_options == null)
            {
                Log.Error("TempContainer.GetProjectRoles(): Option not provided.");
                return null;
            }

            BimProjectsApi _projectsApi = new BimProjectsApi(GetToken, _options);
            List<IndustryRole> roles = new List<IndustryRole>();
            _projectsApi.GetIndustryRoles(projId, out roles, accountId);
            return roles;
        }
        public static string AddProject(BimProject project, string accountId = null, int rowIndex = -1)
        {
            BimProjectsApi _projectsApi = new BimProjectsApi(GetToken, _options);
            project.include_name_to_request_body = true;

            // replace empty strings with null
            PropertyInfo[] properties = project.GetType().GetProperties();
            foreach (PropertyInfo propInfo in properties)
            {
                if (typeof(string) == propInfo.PropertyType)
                {
                    string s = propInfo.GetValue(project) as string;  //as string;
                    if (s != null && s.Equals(""))
                    {
                        propInfo.SetValue(project, null);
                    }
                }

            }

            bool success = false;
            BimProject newProject = null;
            IRestResponse response = _projectsApi.PostProject(project, accountId);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                newProject = JsonConvert.DeserializeObject<BimProject>(response.Content);
                success = true;
            }
            // In certain case, the BIM 360 backend takes more than 10 seconds to handle the request, 
            // this will result in 504 gateway timeout error, but the project should be already successfully 
            // created, add this check to fix this issue.
            if( response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout )
            {
                Thread.Sleep(3000);
                List<BimProject> projectList = GetProjects(@"-created_at");
                newProject = projectList.FirstOrDefault();
                success = newProject != null && newProject.name == project.name;
            }
            if( success )
            {
                if (_AllProjects == null)
                {
                    _AllProjects = GetProjects();
                }

                if (accountId == null)
                {
                    _AllProjects.Add(newProject);
                }

                if (rowIndex > -1)
                {
                    _projectTable.Rows[rowIndex]["id"] = newProject.id;
                    _projectTable.Rows[rowIndex]["result"] = ResultCodes.ProjectCreated;
                }
                Log.Info($"- project {newProject.name} created with ID {newProject.id}!");
                return newProject.id;
            }
            else
            {
                ResponseContent content = null;
                content = JsonConvert.DeserializeObject<ResponseContent>(response.Content);
                string msg = ((content != null && content.message != null) ? content.message : null);
                if (rowIndex > -1)
                {
                    _projectTable.Rows[rowIndex]["result"] = ResultCodes.Error;
                    _projectTable.Rows[rowIndex]["result_message"] = msg;
                }
                Log.Warn($"Status Code: {response.StatusCode.ToString()}\t Message: {msg}");
                return "error";
            }
        }

        public static void UpdateProject(BimProject project, int rowIndex = -1)
        {
            BimProjectsApi _projectsApi = new BimProjectsApi(GetToken, _options);
            IRestResponse response = _projectsApi.PatchProjects(project.id, project);
            HandleUpdateProjectResponse(response, project, rowIndex);
        }

        public static void ArchiveProject(BimProject project)
        {
            BimProjectsApi _projectsApi = new BimProjectsApi(GetToken, _options);
            project.status = Status.archived;
            project.include_name_to_request_body = false;
            project.service_types = "";
            IRestResponse response = _projectsApi.PatchProjects(project.id, project);
            HandleUpdateProjectResponse(response, project);
        }
        public static void AddCompanies(List<BimCompany> companies)
        {
            AccountApi _companiesApi = new AccountApi(GetToken, _options);
            IRestResponse response = _companiesApi.PostCompanies(companies);
            HandleAddCompaniesResponse(response);
        }
        public static void AddAccountUsers(List<HqUser> users)
        {
            AccountApi _usersApi = new AccountApi(GetToken, _options);
            IRestResponse response = _usersApi.PostUsers(users);
            HandleAddUsersResponse(response);
        }
        #endregion

        #region Response Handler
        internal static List<Hub> HandleGetHubsResponse(IRestResponse response)
        {
            List<Hub> hubs = null;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                JsonApiResponse<List<Hub>> result = JsonConvert.DeserializeObject<JsonApiResponse<List<Hub>>>(response.Content, settings);
                hubs = result.data;
            }
            return hubs;
        }
        internal static void HandleAddCompaniesResponse(IRestResponse response)
        {
            BimCompaniesResponse content = JsonConvert.DeserializeObject<BimCompaniesResponse>(response.Content);

            if (content.failure > 0)
            {
                Log.Error($"- Failed to add following companies:");
                foreach (BimCompaniesFailureItem item in content.failure_items)
                {
                    Log.Error($"- {item.item.name}");
                }
            }

            // Update the list box with newly added companies
            if (content.success > 0)
            {
                Log.Info($"- Succeed to add following companies:");

                foreach (BimCompany company in content.success_items)
                {
                    Log.Info($"- {company.name}");
                    _Companies.Add(company);
                }
            }
        }
        internal static void HandleAddUsersResponse(IRestResponse response)
        {
            HqUserResponse content = JsonConvert.DeserializeObject<HqUserResponse>(response.Content);

            if (content.failure > 0)
            {
                Log.Info($"Failed to add following users:");
                foreach (HqUserFailureItem item in content.failure_items)
                {
                    Log.Error($"- {item.item.full_name}");
                }
            }

            // Update the list box with newly added companies
            if (content.success > 0)
            {
                Log.Info($"Succeed to add following users:");

                foreach (HqUser user in content.success_items)
                {
                    Log.Info($"- {user.full_name}");
                    _AccountUsers.Add(user);
                }
                _AccountUsers = _AccountUsers.OrderBy(x => x.full_name).ToList();
            }
        }
        internal static BimProject HandleGetProjectResponse(IRestResponse response)
        {
            BimProject project = null;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                project = JsonConvert.DeserializeObject<BimProject>(response.Content, settings);
            }
            return project;
        }
        internal static void HandleUpdateProjectResponse(IRestResponse response, BimProject project, int rowIndex = -1)
        {
            LogResponse(response);
            ResponseContent content = null;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                BimProject projectResponse = JsonConvert.DeserializeObject<BimProject>(response.Content);
                BimProject updatedProject = AllProjects.FirstOrDefault(p => p.id == projectResponse.id);
                _AllProjects.Remove(updatedProject);
                _AllProjects.Add(projectResponse);
                if (rowIndex > -1)
                {
                    _projectTable.Rows[rowIndex]["id"] = projectResponse.id;
                    _projectTable.Rows[rowIndex]["result"] = ResultCodes.ProjectUpdated;
                }
                Log.Info($"- project {projectResponse.name} updated. (original Name: {updatedProject.name})");
            }
            else
            {
                content = JsonConvert.DeserializeObject<ResponseContent>(response.Content);
                string msg = ((content != null && content.message != null) ? content.message : null);
                if (rowIndex > -1)
                {
                    _projectTable.Rows[rowIndex]["result"] = ResultCodes.Error;
                    _projectTable.Rows[rowIndex]["result_message"] = msg;
                }
                Log.Warn($"Status Code: {response.StatusCode.ToString()}\t Message: {msg}");
            }
        }
        internal static string HandleCreateProjectResponse(IRestResponse response, string accountId, int rowIndex)
        {
            ResponseContent content = null;
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                BimProject projectResponse = JsonConvert.DeserializeObject<BimProject>(response.Content);
                if (_AllProjects == null)
                {
                    _AllProjects = GetProjects();
                }

                if (accountId == null)
                {
                    _AllProjects.Add(projectResponse);
                }

                if (rowIndex > -1)
                {
                    _projectTable.Rows[rowIndex]["id"] = projectResponse.id;
                    _projectTable.Rows[rowIndex]["result"] = ResultCodes.ProjectCreated;
                }
                Log.Info($"- project {projectResponse.name} created with ID {projectResponse.id}!");
                return projectResponse.id;
            }
            else
            {
                content = JsonConvert.DeserializeObject<ResponseContent>(response.Content);
                string msg = ((content != null && content.message != null) ? content.message : null);
                if (rowIndex > -1)
                {
                    _projectTable.Rows[rowIndex]["result"] = ResultCodes.Error;
                    _projectTable.Rows[rowIndex]["result_message"] = msg;
                }
                Log.Warn($"Status Code: {response.StatusCode.ToString()}\t Message: {msg}");
                return "error";
            }
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
