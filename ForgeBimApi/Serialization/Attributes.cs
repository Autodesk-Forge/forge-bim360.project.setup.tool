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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Autodesk.Forge.BIM360.Serialization
{
  public class Attributes
    {
        #region Properties

        [JsonProperty("displayName")]
        public string displayName { get; set; }

        [JsonProperty("createTime")]
        public string createTime { get; set; }

        [JsonProperty("createUserId")]
        public string createUserId { get; set; }

        [JsonProperty("createUserName")]
        public string createUserName { get; set; }

        [JsonProperty("lastModifiedTime")]
        public string lastModifiedTime { get; set; }

        [JsonProperty("lastModifiedUserId")]
        public string lastModifiedUserId { get; set; }

        [JsonProperty("lastModifiedUserName")]
        public string lastModifiedUserName { get; set; }

        [JsonProperty("versionNumber")]
        public string versionNumber { get; set; }

        [JsonProperty("fileType")]
        public string fileType { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("mimeType")]
        public string mimeType { get; set; }

        [JsonProperty("storageSize")]
        public object storageSize { get; set; }

        [JsonProperty("extension")]
        public Extension extension { get; set; }

        [JsonProperty("hidden")]
        public bool hidden { get; set; }

        #endregion Properties

        #region Constructors
        [DebuggerStepThrough]
        public Attributes()
        {
        } // constructor
        #endregion Constructors

    } // class
} // namespace
