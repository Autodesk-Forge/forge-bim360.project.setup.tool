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

using Autodesk.Forge.BIM360.Serialization;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Autodesk.Forge.BIM360
{
    public class BimProjectsApi: ForgeApi
    {          

        public BimProjectsApi(Token token, ApplicationOptions options) : base(token, options)
        {            
            ContentType = "application/json";
        }

        public IRestResponse PostProject(BimProject project, string accountId = null)
        {           
            var request = new RestRequest(Method.POST);
            //request.Resource = "hq/v1/accounts/{AccountId}/projects";
            request.Resource = Urls["projects"];

            if (accountId == null)
            {
                request.AddParameter("AccountId", options.ForgeBimAccountId, ParameterType.UrlSegment);
            }
            else
            {
                request.AddParameter("AccountId", accountId, ParameterType.UrlSegment);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.DateFormatString = "yyyy-MM-dd";
            settings.NullValueHandling = NullValueHandling.Ignore;
            string projectString = JsonConvert.SerializeObject(project, settings);
            request.AddParameter("application/json", projectString, ParameterType.RequestBody);

            request.AddHeader("content-type", ContentType);
            request.AddHeader("authorization", $"Bearer {Token}");
            
            IRestResponse response = ExecuteRequest(request);
            return response;
        }

        /// <summary>
        /// Assigns an admin user and services to a project. Returns an error if that user is already assigned
        /// To update projects and add new services use PatchProjects instead
        /// </summary>
        /// <param name="projectId">Id of the project</param>
        /// <param name="service">A ServiceActivation object</param>
        /// <returns>IRestResponse object</returns>
        public IRestResponse PostUserAndService(string projectId, ServiceActivation service)
        {
            var request = new RestRequest(Method.POST);
            //request.Resource = "hq/v1/accounts/{AccountId}/projects/{ProjectId}/users";
            request.Resource = Urls["projects_projectId_users"];
            request.AddParameter("AccountId", options.ForgeBimAccountId, ParameterType.UrlSegment);
            request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", $"Bearer {Token}");
            request.AddHeader("content-type", ContentType);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            string serviceString = JsonConvert.SerializeObject(service, settings);
            request.AddParameter("application/json", serviceString, ParameterType.RequestBody);

            IRestResponse response = ExecuteRequest(request); // Client.Execute(request);
            return response;
        }

        public IRestResponse PostUsersImport(string projectId, string userId, List<ProjectUser> users, string accountId = null)
        {
            var request = new RestRequest(Method.POST);
            //request.Resource = "hq/v2/accounts/{AccountId}/projects/{ProjectId}/users/import";
            request.Resource = Urls["projects_projectId_users_import"];
            if(accountId == null)
            {
                request.AddParameter("AccountId", options.ForgeBimAccountId, ParameterType.UrlSegment);
            }
            else
            {
                request.AddParameter("AccountId", accountId, ParameterType.UrlSegment);
            }

            request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            string serviceString = JsonConvert.SerializeObject(users, settings);
            request.AddParameter("application/json", serviceString, ParameterType.RequestBody);

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", $"Bearer {Token}");
            request.AddHeader("content-type", ContentType);
            request.AddHeader("x-user-id", userId);

            IRestResponse response = ExecuteRequest(request);
            return response;
        }

        public IRestResponse PatchUser(string projectId, string adminUserId, string userId, ProjectUser user, string accountId = null)
        {
            var request = new RestRequest(Method.PATCH);
            request.Resource = Urls["projects_projectId_user_patch"];
            if (accountId == null)
            {
                request.AddParameter("AccountId", options.ForgeBimAccountId, ParameterType.UrlSegment);
            }
            else
            {
                request.AddParameter("AccountId", accountId, ParameterType.UrlSegment);
            }

            request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);
            request.AddParameter("UserId", userId, ParameterType.UrlSegment);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            user.email = null;
            string serviceString = JsonConvert.SerializeObject(user, settings);
            request.AddParameter("application/json", serviceString, ParameterType.RequestBody);

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", $"Bearer {Token}");
            request.AddHeader("content-type", ContentType);
            request.AddHeader("x-user-id", adminUserId);

            IRestResponse response = ExecuteRequest(request);
            return response;
        }


        /// <summary>
        /// Update projects properties and services assigned to the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public IRestResponse PatchProjects(string projectId, BimProject project)
        {
            var request = new RestRequest(Method.PATCH);
            //request.Resource = "hq/v1/accounts/{AccountId}/projects/{ProjectId}";
            request.Resource = Urls["projects_projectId"];

            request.AddParameter("AccountId", options.ForgeBimAccountId, ParameterType.UrlSegment);
            request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.DateFormatString = "yyyy-MM-dd";
            settings.NullValueHandling = NullValueHandling.Ignore;
            string projectString = JsonConvert.SerializeObject(project, settings);
            request.AddParameter("application/json", projectString, ParameterType.RequestBody);

            request.AddHeader("content-type", ContentType);
            request.AddHeader("authorization", $"Bearer {Token}");

            IRestResponse response = ExecuteRequest(request);
            return response;
        }


        /// <summary>
        /// Get all projects of an account - this uses the paged GET request and collects all
        /// results in a list which is the out parameter of the method
        /// </summary>
        /// <param name="result">List of all BimProject objects</param>
        /// <returns>IRestResponse that indicates the status of the call</returns>
        public IRestResponse GetProjects(out List<BimProject> result, string sortProp = "updated_at", int limit = 100, int offset = 0 )
        {
            Log.Info($"Querying Projects from AccountID '{options.ForgeBimAccountId}'");
            result = new List<BimProject>();
            List<BimProject> projects;
            IRestResponse response = null;
            do
            {
                projects = null;
                try
                {
                    var request = new RestRequest(Method.GET);
                    //request.Resource = "hq/v1/accounts/{AccountId}/projects";
                    request.Resource = Urls["projects"];
                    request.AddParameter("AccountId", options.ForgeBimAccountId, ParameterType.UrlSegment);
                    request.AddHeader("authorization", $"Bearer {Token}");
                    request.AddParameter("sort", sortProp, ParameterType.QueryString);
                    request.AddParameter("limit", limit, ParameterType.QueryString);
                    request.AddParameter("offset", offset, ParameterType.QueryString);

                    response = ExecuteRequest(request);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        JsonSerializerSettings settings = new JsonSerializerSettings();
                        settings.NullValueHandling = NullValueHandling.Ignore;
                        projects = JsonConvert.DeserializeObject<List<BimProject>>(response.Content, settings);
                        result.AddRange(projects);
                        offset += limit;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    throw ex;
                }
            }
            while (projects != null && projects.Count == limit);

            return response;
        }




        /// <summary>
        /// Get all projects of an account - this uses the paged GET request and collects all
        /// results in a list which is the out parameter of the method
        /// </summary>
        /// <param name="result">List of all BimProject objects</param>
        /// <returns>IRestResponse that indicates the status of the call</returns>
        public IRestResponse GetProject(string projectId, string accountId = null)
        {
            Log.Info($"Querying Projects from AccountID '{options.ForgeBimAccountId}'");
            IRestResponse response = null;
            try
            {
                var request = new RestRequest(Method.GET);
                //request.Resource = "hq/v1/accounts/{AccountId}/projects/{projectId}";
                request.Resource = Urls["projects_projectId"];
                if(accountId == null)
                {
                    request.AddParameter("AccountId", options.ForgeBimAccountId, ParameterType.UrlSegment);
                }
                else
                {
                    request.AddParameter("AccountId", accountId, ParameterType.UrlSegment);
                }

                request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);
                request.AddHeader("authorization", $"Bearer {Token}");

                response = ExecuteRequest(request);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }

            return response;
        }

        
        public IRestResponse GetIndustryRoles(string projectId, out List<IndustryRole> result, string accountId = null)
        {
            Log.Info($"Querying industry roles from project '{projectId}'");
            result = new List<IndustryRole>();
            try
            {
                var request = new RestRequest(Method.GET);
                //request.Resource = "hq/v2/accounts/{AccountId}/projects/{ProjectId}/industry_roles";
                request.Resource = Urls["projects_projectId_industryRoles"];
                if(accountId == null)
                {
                    request.AddParameter("AccountId", options.ForgeBimAccountId, ParameterType.UrlSegment);
                }
                else
                {
                    request.AddParameter("AccountId", accountId, ParameterType.UrlSegment);
                }

                request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);
                request.AddHeader("authorization", $"Bearer {Token}");

                IRestResponse response = ExecuteRequest(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    List<IndustryRole> roles = JsonConvert.DeserializeObject<List<IndustryRole>>(response.Content, settings);
                    result.AddRange(roles);
                }
                return response;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }

    }
}
