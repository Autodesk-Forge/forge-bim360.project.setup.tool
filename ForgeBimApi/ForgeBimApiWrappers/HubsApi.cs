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
using System.Collections.Generic;

namespace Autodesk.Forge.BIM360
{
    public class HubsApi : ForgeApi
    {
        public HubsApi(Token token, ApplicationOptions options) : base(token, options)
        {
            ContentType = "application/json";
        }

        public IRestResponse GetHub()
        {
            var request = new RestRequest(Method.GET);
            request.Resource = Urls["hubs_hub"];
            string hubId = options.ForgeBimAccountId;
            if (hubId.StartsWith("b.") == false)
            {
                hubId = "b." + hubId;
            }
            request.AddParameter("HubId", hubId, ParameterType.UrlSegment);
            request.AddHeader("authorization", $"Bearer {Token}");
            request.AddHeader("Cache-Control", "no-cache");

            IRestResponse response = ExecuteRequest(request);

            return response;
        }


        public IRestResponse GetHubs()
        {
            var request = new RestRequest(Method.GET);
            request.Resource = Urls["hubs"];
            request.AddHeader("authorization", $"Bearer {Token}");
            request.AddHeader("Cache-Control", "no-cache");

            IRestResponse response = ExecuteRequest(request);

            return response;
        }

        public IRestResponse GetTopFolders(string projectId)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = Urls["hubs_topfolders"];
            string hubId = options.ForgeBimAccountId;
            if (hubId.StartsWith("b.") == false)
            {
                hubId = "b." + hubId;
            }
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }
            request.AddParameter("HubId", hubId, ParameterType.UrlSegment);
            request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);
            request.AddHeader("authorization", $"Bearer {Token}");
            request.AddHeader("Cache-Control", "no-cache");

            IRestResponse response = ExecuteRequest(request);

            return response;
        }
    }
}
