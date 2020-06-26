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
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Autodesk.Forge.BIM360.Serialization;

namespace Autodesk.Forge.BIM360
{
    public class BimProjectFoldersApi : ForgeApi
    {
        //private Token _token;
        public BimProjectFoldersApi(Token token, ApplicationOptions options) : base(token, options)
        {
            ContentType = "application/json";
            //_token = token;
        }

        public IRestResponse GetFolderContents(string projectId, string folderId)
        {
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }

            var request = new RestRequest(Method.GET);
            request.Resource = Urls["folders_folder_contents"];
            string hubId = options.ForgeBimAccountId;
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }
            request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);
            request.AddParameter("FolderId", folderId, ParameterType.UrlSegment);
            request.AddHeader("authorization", $"Bearer {Token}");
            request.AddHeader("Cache-Control", "no-cache");

            IRestResponse response = ExecuteRequest(request);

            return response;
        }

        #region Methods
        /// <summary>
        ///  Returns a collection of items and folders within a folder.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public IList<Folder> GetSubFolders(string projectId, string folderId)
        {
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }

            try
            {
                var request = new RestRequest(Method.GET);

                // "data/v1/projects/" + projectId + "/folders/" + folderId + "/contents"
                request.Resource = string.Format("{0}{1}/folders/{2}/contents",
                                                 "data/v1/projects/",
                                                 projectId,
                                                 folderId);
                request.Resource += "?filter[type]=folders"; // add query string

                request.AddHeader("Authorization", "Bearer " + Token);

                IRestResponse response = ExecuteRequest(request);

                JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.NullValueHandling = NullValueHandling.Ignore;

                IList<Folder> folders = JsonConvert.DeserializeObject<JsonApiResponse<IList<Folder>>>(response.Content, jss).data;

                return folders;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        } // GetSubFolders()

        public IList<Data> GetItems(string projectId, string folderId)
        {
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }

            try
            {
                var request = new RestRequest(Method.GET);

                // "data/v1/projects/" + projectId + "/folders/" + folderId + "/contents"
                request.Resource = string.Format("{0}{1}/folders/{2}/contents",
                                                 "data/v1/projects/",
                                                 projectId,
                                                 folderId);
                request.Resource += "?filter[type]=items"; // add query string
                request.AddHeader("Authorization", "Bearer " + Token);

                IRestResponse response = ExecuteRequest(request);

                JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.NullValueHandling = NullValueHandling.Ignore;

                IList<Data> items = JsonConvert.DeserializeObject<JsonApiResponse<IList<Data>>>(response.Content, jss).data;

                return items;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        } // GetSubFolders()

        /// <summary>
        /// Get a list of all nested subfolder ids found within a specified folder
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public IList<string> GetNestedSubFolderIds(string projectId, string folderId)
        {
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }

            try
            {
                //instantiate the list of ids
                IList<string> folderIds = new List<string>();

                //get the subfolders within the folder
                IList<Folder> subFolders = GetSubFolders(projectId, folderId);

                //check for null
                if (subFolders != null)
                {
                    //process each subfolder
                    foreach (Folder folder in subFolders)
                    {
                        ////clean the id
                        //string id = Uri.EscapeDataString(folder.id);
                        //add it to the list of subfolder ids
                        if (!folderIds.Contains(folder.id))
                        {
                            folderIds.Add(folder.id);
                        }
                        //now process the sub-subfolders within this subfolder
                        IList<string> subSubfolders = GetNestedSubFolderIds(projectId, folder.id);

                        //process each sub-subfolder
                        foreach (string subId in subSubfolders)
                        {
                            ////clean the id
                            //string sid = Uri.EscapeDataString(subId);
                            //add it to the list of subfolder ids
                            if (!folderIds.Contains(subId))
                            {
                                folderIds.Add(subId);
                            }
                        }
                    }
                }

                return folderIds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        /// <summary>
        /// Get a list of all nested subfolder ids found within a specified folder
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public IList<Folder> GetNestedSubFolder(string projectId, string folderId)
        {
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }

            try
            {
                //instantiate the list of ids
                IList<Folder> folders = new List<Folder>();

                //get the subfolders within the folder
                IList<Folder> subFolders = GetSubFolders(projectId, folderId);

                //check for null
                if (subFolders != null)
                {
                    //process each subfolder
                    foreach (Folder folder in subFolders)
                    {
                        ////clean the id
                        //string id = Uri.EscapeDataString(folder.id);
                        //add it to the list of subfolder ids
                        if (!folders.Contains(folder))
                        {
                            folders.Add(folder);
                        }
                        //now process the sub-subfolders within this subfolder
                        IList<Folder> subSubfolders = GetNestedSubFolder(projectId, folder.id);

                        //process each sub-subfolder
                        foreach (Folder subFolder in subSubfolders)
                        {
                            ////clean the id
                            //string sid = Uri.EscapeDataString(subId);
                            //add it to the list of subfolder ids
                            if (!folders.Contains(subFolder))
                            {
                                folders.Add(subFolder);
                            }
                        }
                    }
                }

                return folders;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public NestedFolder GetFoldersHierarchy(string projectId, NestedFolder thisFolder)
        {
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }

            try
            {
                // keep the current folder permission
                List<FolderPermission> folderPermissions = new List<FolderPermission>();

                IRestResponse res = GetFolderPermissions(projectId, thisFolder.id, out folderPermissions);
                if( res.StatusCode == System.Net.HttpStatusCode.OK )
                {
                    thisFolder.permissions.AddRange(folderPermissions);
                }

                IList<NestedFolder> output = new List<NestedFolder>();

                //get the subfolders and items residing in the folder
                IList<Folder> subFolders = GetSubFolders(projectId, thisFolder.id);
                IList<Data> items = GetItems(projectId, thisFolder.id);

                if (subFolders != null)
                {
                    //process each subfolder
                    foreach (Folder folder in subFolders)
                    {
                        NestedFolder subFolder = new NestedFolder(folder.attributes.name, folder.id, thisFolder);

                        //now process the sub-subfolders within this subfolder
                        NestedFolder subFolderWithChildren = GetFoldersHierarchy(projectId, subFolder);
                        thisFolder.childrenFolders.Add(subFolderWithChildren);
                    }
                }

                if(items != null)
                {
                    thisFolder.items = items;
                }

                return thisFolder;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Get a list of permissions for a specified folder
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="folderId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public IRestResponse GetFolderPermissions(string projectId, string folderId, out List<FolderPermission> result)
        {
            result = new List<FolderPermission>();

            if (projectId.StartsWith("b.") == true)
            {
                projectId = projectId.Remove(0,2);
            }

            var request = new RestRequest(Method.GET);
            request.Resource = Urls["folder_permission"];
  
            request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);
            request.AddParameter("FolderId", folderId, ParameterType.UrlSegment);
            request.AddHeader("authorization", $"Bearer {Token}");
            request.AddHeader("Cache-Control", "no-cache");

            IRestResponse response = ExecuteRequest(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                List<FolderPermission> roles = JsonConvert.DeserializeObject<List<FolderPermission>>(response.Content, settings);
                result.AddRange(roles);
            }
            return response;
        }
        /// <summary>
        /// Iterate through a passed in path 
        /// </summary>
        /// <param name="hubId"></param>
        /// <param name="projectId"></param>
        /// <param name="parentFolderId"></param>
        /// <param name="fullPath"></param>
        /// <param name="currentPath"></param>
        /// <returns></returns>
        public Folder GetBranchFolder(string hubId, string projectId, string parentFolderId, string fullPath, string currentPath)
        {
            if (hubId.StartsWith("b.") == false)
            {
                hubId = "b." + hubId;
            }
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }


            Folder childFolder = new Folder();

            //get a list of subfolders in the parent folder
            IList<Folder> subfolders = GetSubFolders(projectId, parentFolderId);

            //check each of the subfolders
            foreach (Folder f in subfolders)
            {
                if (fullPath.Contains(currentPath + "/" + f.attributes.displayName))
                {
                    //check for an exact match
                    if (fullPath.Equals(currentPath + "/" + f.attributes.displayName))
                    {
                        return f;
                    }
                    //no exact match, so keep going
                    childFolder = f;
                    break;
                }
            }

            return GetBranchFolder(hubId, projectId, childFolder.id, fullPath, currentPath + "/" + childFolder.attributes.displayName);
        }
        public string CustomCreateFolder(string projectId, string parentFolderId, string newFolderName)
        {
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }

            var request = new RestRequest(Method.POST);

            // "data/v1/projects/" + projectId + "/folders"
            request.Resource = string.Format("{0}{1}/folders",
                                              "data/v1/projects/",
                                              projectId);

            request.AddHeader("Authorization", "Bearer " + Token);
            request.AddHeader("Content-Type", "application/vnd.api+json");
            request.AddParameter("application/vnd.api+json", createFolderJsonBody(newFolderName, parentFolderId), ParameterType.RequestBody);

            IRestResponse response = ExecuteRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                return "error";
            }

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.NullValueHandling = NullValueHandling.Ignore;

            CreateFolderResponse jsonResponse =
                JsonConvert.DeserializeObject<CreateFolderResponse>(response.Content, jss);

            string newFolderId = jsonResponse.data.id;

            return newFolderId;
        }
        /// <summary>
        /// Creates a new folder
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <param name="parentFolderId"></param>
        /// <param name="newFolderName"></param>
        /// <returns></returns>
        public string CreateFolder(string projectId, string userId, string parentFolderId, string newFolderName)
        {
            if (projectId.StartsWith("b.") == false)
            {
                projectId = "b." + projectId;
            }

            var request = new RestRequest(Method.POST);

            // "data/v1/projects/" + projectId + "/folders"
            request.Resource = string.Format("{0}{1}/folders",
                                              "data/v1/projects/",
                                              projectId);

            request.AddHeader("Authorization", "Bearer " + Token);
            request.AddHeader("Content-Type", "application/vnd.api+json");
            request.AddHeader("x-user-id", userId);
            request.AddParameter("application/vnd.api+json", createFolderJsonBody(newFolderName, parentFolderId), ParameterType.RequestBody);

            IRestResponse response = ExecuteRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                return "error";
            }

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.NullValueHandling = NullValueHandling.Ignore;

            CreateFolderResponse jsonResponse =
                JsonConvert.DeserializeObject<CreateFolderResponse>(response.Content, jss);

            string newFolderId = jsonResponse.data.id;

            return newFolderId;
        }
        public bool CustomAssignPermission(string projectId, string folderId, List<FolderPermission> folderPermissions)
        {
            if (projectId.StartsWith("b.") == true)
            {
                projectId = projectId.Remove(0, 2);
            }

            var request = new RestRequest(Method.POST);
            request.Resource = Urls["folder_permission_create"];
            request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);
            request.AddParameter("FolderId", folderId, ParameterType.UrlSegment);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            string permissionsString = JsonConvert.SerializeObject(folderPermissions, settings);
            request.AddParameter("application/json", permissionsString, ParameterType.RequestBody);

            request.AddHeader("Authorization", "Bearer " + Token);
            request.AddHeader("Content-Type", "application/json");

            IRestResponse response = ExecuteRequest(request);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        /// <summary>
        /// Assign only role permission to a specified folder
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="folderId"></param>
        /// <param name="folderPermissions"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool AssignPermission(string projectId,  string folderId, List<FolderPermission> folderPermissions,  string userId )
        {
            List<FolderPermission> rolePermissions = folderPermissions.Where(permission => permission.subjectType == "ROLE" && permission.actions.Count > 0).ToList();
            
            // no need to do anything is there is no role permission for this folder
            if (rolePermissions.Count == 0)
                return true;

            if (projectId.StartsWith("b.") == true)
            {
                projectId = projectId.Remove(0, 2);
            }

            var request = new RestRequest(Method.POST);
            request.Resource = Urls["folder_permission_create"];
            request.AddParameter("ProjectId", projectId, ParameterType.UrlSegment);
            request.AddParameter("FolderId", folderId, ParameterType.UrlSegment);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            string permissionsString = JsonConvert.SerializeObject(rolePermissions, settings);
            request.AddParameter("application/json", permissionsString, ParameterType.RequestBody);

            request.AddHeader("Authorization", "Bearer " + Token);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("x-user-id", userId);

            IRestResponse response = ExecuteRequest(request);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }



        /// <summary>
        /// This provides the JSON body content for the CreateFolder command.  Since only two paramters change and the
        /// development team says that the schema is not fixed yet, it didn't seem worth the effort to deserialize this to
        /// classes and parameters.  Later when more commands are used in Forge, then it might make sense to deserialize.
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private string createFolderJsonBody(string folderName, string parentId)
        {
            string jsonBody = @"
            { 
                ""jsonapi"": {
                    ""version"": ""1.0""
                },
                ""data"": {
		            ""type"": ""folders"",
		            ""attributes"": {
			            ""name"": """ + folderName + @""",
                        ""extension"": {
                            ""type"": ""folders:autodesk.bim360:Folder"",
                            ""version"": ""1.0""
                        }
		            },
		            ""relationships"": {
                        ""parent"": {
                            ""data"": {
                                ""type"": ""folders"",
                                ""id"": """ + parentId + @"""
                            }
                        }
		            }
                },
            }";

            string jsonBodyWoutControlChars = new string(jsonBody.Where(c => !char.IsControl(c)).ToArray());
            return jsonBodyWoutControlChars;
        }

        #endregion Methods
    }
}
