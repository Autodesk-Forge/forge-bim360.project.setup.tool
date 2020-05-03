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

using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Autodesk.Forge.BIM360
{
    // pointer to a function that provides a valid token
    public delegate string Token();

    public interface ApplicationOptions
    {
        string ForgeClientId { get; }
        string ForgeClientSecret { get; }

        string ForgeBimAccountId { get; }
        string BaseUrl { get; }
        string FilePath { get; }
        string ServiceFilePath { get; }

        string HqAdmin { get; }

        char Separator { get; }

        Encoding Encoding { get; }
        string FormatPattern { get; }

        bool TrialRun { get; }

        string AccountRegion { get; }
    }

    public class ForgeApi
    {
        internal static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();      
        internal ApplicationOptions options;
        internal Token GetToken;

        private int[] retryStatusCodes = { 429, 503, 504 };

        internal string Token
        {
            get
            {
                return GetToken();
            }
        }
        internal RestClient Client { get; set; }

        public string ContentType { get; set; }

        private string BaseUrl
        {
            set
            {
                if (Client != null)
                {
                    Client.BaseUrl = new System.Uri(value);
                }
            }
        }

        public Dictionary<string, string> Urls { get; private set; }

        public ForgeApi(Token token, ApplicationOptions options)
        {
            Client = new RestClient();
            BaseUrl = string.IsNullOrWhiteSpace(options.BaseUrl) == false ? options.BaseUrl : "https://developer.api.autodesk.com";

            this.options = options;
            this.GetToken = token;

            this.setUrls(options.AccountRegion);
        }

        private void setUrls(string accountRegion)
        {
            string regionBasedUrl = "";
            Urls = new Dictionary<string, string>();
            switch (accountRegion)
            {
                case "US":
                    break;

                case "EU":
                    regionBasedUrl = "regions/eu/";
                    break;
                default:
                    break;
            }

            Urls["projects"] = "hq/v1/" + regionBasedUrl + "accounts/{AccountId}/projects";
            Urls["projects_projectId"] = "hq/v1/" + regionBasedUrl + "accounts/{AccountId}/projects/{ProjectId}";
            Urls["projects_projectId_users"] = "hq/v1/" + regionBasedUrl + "accounts/{AccountId}/projects/{ProjectId}/users";
            Urls["projects_projectId_users_import"] = "hq/v2/" + regionBasedUrl + "accounts/{AccountId}/projects/{ProjectId}/users/import";
            Urls["projects_projectId_industryRoles"] = "hq/v2/" + regionBasedUrl + "accounts/{AccountId}/projects/{ProjectId}/industry_roles";
            Urls["companies"] = "hq/v1/" + regionBasedUrl + "accounts/{AccountId}/companies";
            Urls["companies_import"] = "hq/v1/" + regionBasedUrl + "accounts/{AccountId}/companies/import";
            Urls["users"] = "hq/v1/" + regionBasedUrl + "accounts/{AccountId}/users";
            Urls["users_import"] = "hq/v1/" + regionBasedUrl + "accounts/{AccountId}/users/import";
            Urls["businessUnitsStructure"] = "hq/v1/" + regionBasedUrl + "accounts/{AccountId}/business_units_structure";
            Urls["folders_folder_contents"] = "data/v1/projects/{ProjectId}/folders/{FolderId}/contents";
            Urls["hubs"] = "project/v1/hubs";
            Urls["hubs_hubId"] = "project/v1/hubs/{HubId}";
            Urls["hubs_topfolders"] = "project/v1/hubs/{HubId}/projects/{ProjectId}/topFolders";
            Urls["folder_permission"] = "bim360/docs/v1/projects/{ProjectId}/folders/{FolderId}/permissions";
            Urls["folder_permission_create"] = "bim360/docs/v1/projects/{ProjectId}/folders/{FolderId}/permissions:batch-create";
        }

        public IRestResponse ExecuteRequest(RestRequest req)
        {
            int sleepTime, retryAfter = 20;
            int tryCount = 1;
            IRestResponse response = null;
            do
            {
                Log.Debug($"Executing {req.Method} request {tryCount}. attempt to resource: {req.Resource}");
                response = Client.Execute(req);
                Log.Debug($"Response Status {response.StatusCode.ToString()}");
                if (retryStatusCodes.Contains((int)response.StatusCode))
                {
                    Parameter param = response.Headers.FirstOrDefault(p => string.Equals(p.Name, "Retry-After"));
                    if (param != null)
                    {
                        int.TryParse(param.Value.ToString(), out retryAfter);
                        //return is in seconds, convert to milliseconds and add butter 3 sec 
                    }
                    sleepTime = retryAfter * 1000 + 3000;
                    Log.Info($"Request not successfull - Waiting { sleepTime / 1000 } seconds until rate limit is reset..");
                    System.Threading.Thread.Sleep(sleepTime);
                    tryCount++;
                }
                else
                {
                    break;
                }

            } while (tryCount < 5);            
            Log.Trace(response.Content.ToString());

            return response;
        }
    }
}
