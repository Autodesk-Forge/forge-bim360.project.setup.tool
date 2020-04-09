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
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Autodesk.Forge.BIM360.Serialization
{
  public class Storage
    {
        #region Properties

        [JsonProperty("jsonapi")]
        public JsonApi jsonapi { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("data")]
        public Data data { get; set; }

        [JsonProperty("meta")]
        public Meta meta { get; set; }

        [JsonProperty("relationships")]
        public Relationships relationships { get; set; }

        #endregion Properties

        #region Constructors

        [DebuggerStepThrough]
        public Storage()
        {
        } // constructor

        public Storage(string fileName, string folderId)
        {
            jsonapi = new JsonApi
            {
                version = "1.0",
            };

            data = new Data
            {
                type = "objects",
                attributes = new Attributes
                {
                    name = fileName
                },
                relationships = new Relationships
                {
                    target = new Target
                    {
                        data = new Data
                        {
                            type = "folders",
                            id = folderId
                        }
                    }
                }
            };
        } // constructor

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Returns a storage request body string that can be used as RequestRequest parameter.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folderId"></param>
        /// <returns>Request body string</returns>
        public static string Request(string fileName, string folderId)
        {
            string sRequest = string.Empty;

            Storage storage = new Storage();
            storage.jsonapi = new JsonApi
            {
                version = "1.0",
            };

            storage.data = new Data
            {
                type = "objects",
                attributes = new Attributes
                {
                    name = fileName
                },
                relationships = new Relationships
                {
                    target = new Target
                    {
                        data = new Data
                        {
                            type = "folders",
                            id = folderId
                        }
                    }
                }
            };

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.NullValueHandling = NullValueHandling.Ignore;
            sRequest = JsonConvert.SerializeObject(storage, jss);

            return sRequest;
        } // Request()

        #endregion Methods
    } // class
} // namespace
