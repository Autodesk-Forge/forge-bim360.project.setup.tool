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

using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Autodesk.Forge.BIM360.Serialization
{
  public class Item
    {
        #region Properties

        //[JsonProperty("type")]
        //public string type { get; set; }

        //[JsonProperty("id")]
        //public string id { get; set; }

        [JsonProperty("jsonapi")]
        public JsonApi jsonapi { get; set; }

        //[JsonProperty("attributes")]
        //public Attributes attributes { get; set; }

        [JsonProperty("data")]
        public Data data { get; set; }

        [JsonProperty("links")]
        public Links links { get; set; }

        [JsonProperty("included")]
        public List<Included> included { get; set; }

        #endregion Properties

        #region Constructors

        [DebuggerStepThrough]
        public Item()
        {
        } // constructor

        public Item(string fileName, string itemId, int iVersion, string parentFolderId)
        {
            jsonapi = new JsonApi
            {
                version = "1.0"
            };

            data = new Data
            {
                type = "items",
                attributes = new Attributes
                {
                    displayName = fileName,
                    extension = new Extension
                    {
                        //type = "items:autodesk.core:File",
                        type = "items:autodesk.bim360:File",
                        version = "1.0"
                    } // new Extension
                },
                relationships = new Relationships
                {
                    tip = new Tip
                    {
                        data = new Data
                        {
                            type = "versions",
                            id = iVersion.ToString()
                        } // new Data
                    },
                    parent = new Parent
                    {
                        data = new Data
                        {
                            type = "folders",
                            id = parentFolderId
                        } // new Data
                    } // new Parent
                } // new Relationships
            };

            included = new List<Included>()
            {
              new Included
              {
                type = "versions",
                id = iVersion.ToString(),
                attributes = new Attributes
                  {
                    name = fileName,
                    extension = new Extension
                    {
                      //type = "versions:autodesk.core:File",
                      type = "versions:autodesk.bim360:File",
                      version = "1.0"
                    } // new Extension
                  },
                relationships = new Relationships
                  {
                    storage = new Storage
                    {
                      data = new Data
                      {
                        type = "objects",
                        id = itemId
                      } // new Data
                    } // new Storage
                  } // new Relationships
                } // new Included
            }; // new List<Included>
        } // constructor

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Returns an item request body string that can be used as RequestRequest parameter.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="itemId"></param>
        /// <param name="iVersion"></param>
        /// <param name="parentFolderId"></param>
        /// <returns>Request body string</returns>
        public static string Request(string fileName, string itemId, int iVersion, string parentFolderId)
        {
            string sRequest = string.Empty;

            Item item = new Item();

            item.jsonapi = new JsonApi
            {
                version = "1.0"
            };

            item.data = new Data
            {
                type = "items",
                attributes = new Attributes
                {
                    displayName = fileName,
                    extension = new Extension
                    {
                        //type = "items:autodesk.core:File",
                        type = "items:autodesk.bim360:File",
                        version = "1.0"
                    } // new Extension
                },
                relationships = new Relationships
                {
                    tip = new Tip
                    {
                        data = new Data
                        {
                            type = "versions",
                            id = iVersion.ToString()
                        } // new Data
                    },
                    parent = new Parent
                    {
                        data = new Data
                        {
                            type = "folders",
                            id = parentFolderId
                        } // new Data
                    } // new Parent
                } // new Relationships
            };

            item.included = new List<Included>()
            {
              new Included
              {
                type = "versions",
                id = iVersion.ToString(),
                attributes = new Attributes
                {
                  name = fileName,
                  extension = new Extension
                  {
                    //type = "versions:autodesk.core:File",
                    type = "versions:autodesk.bim360:File",
                    version = "1.0"
                  } // new Extension
                },
                relationships = new Relationships
                {
                  storage = new Storage
                  {
                    data = new Data
                    {
                      type = "objects",
                      id = itemId
                    } // new Data
                  } // new Storage
                } // new Relationships
              } // new Included
            };

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.NullValueHandling = NullValueHandling.Ignore;
            sRequest = JsonConvert.SerializeObject(item, jss);

            return sRequest;
        } // Request

        public static string Request(string fileName, string storageId, string itemId)
        {
            string sRequest = string.Empty;

            Item item = new Item();

            item.jsonapi = new JsonApi
            {
                version = "1.0"
            };

            item.data = new Data
            {
                type = "versions",
                attributes = new Attributes
                {
                    name = fileName,
                    extension = new Extension
                    {
                        //type = "versions:autodesk.core:File",
                        type = "versions:autodesk.bim360:File",
                        version = "1.0"
                    }
                },
                relationships = new Relationships
                {
                    item = new Item
                    {
                        data = new Data
                        {
                            type = "items",
                            id = itemId
                        }
                    },
                    storage = new Storage
                    {
                        data = new Data
                        {
                            type = "objects",
                            id = storageId
                        }
                    }
                }
            };

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.NullValueHandling = NullValueHandling.Ignore;
            sRequest = JsonConvert.SerializeObject(item, jss);

            return sRequest;
        } // Request()

        public static string DeleteRequest(string fileName, string itemId)
        {
            string sRequest = string.Empty;

            Item item = new Item();

            item.jsonapi = new JsonApi
            {
                version = "1.0"
            };

            item.data = new Data
            {
                type = "versions",
                attributes = new Attributes
                {
                    name = fileName,
                    extension = new Extension
                    {
                        type = "versions:autodesk.core:Deleted",
                        version = "1.0"
                    }
                },
                relationships = new Relationships
                {
                    item = new Item
                    {
                        data = new Data
                        {
                            type = "items",
                            id = itemId
                        }
                    }
                }
            };

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.NullValueHandling = NullValueHandling.Ignore;
            sRequest = JsonConvert.SerializeObject(item, jss);

            return sRequest;
        } // DeleteRequest()

        #endregion Methods

    } // class
} // namespace
