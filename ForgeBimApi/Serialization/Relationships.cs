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
  public class Relationships
    {
        #region Properties

        [JsonProperty("jsonapi")]
        public JsonApi jsonapi { get; set; }

        [JsonProperty("links")]
        public Links links { get; set; }

        [JsonProperty("data")]
        public Data[] data { get; set; }

        [JsonProperty("included")]
        public Included[] included { get; set; }

        [JsonProperty("tip")]
        public Tip tip { get; set; }

        [JsonProperty("versions")]
        public Versions versions { get; set; }

        [JsonProperty("parent")]
        public Parent parent { get; set; }

        [JsonProperty("refs")]
        public Refs refs { get; set; }

        [JsonProperty("rootFolder")]
        public RootFolder rootFolder { get; set; }

        [JsonProperty("hub")]
        public Hub hub { get; set; }

        [JsonProperty("storage")]
        public Storage storage { get; set; }

        [JsonProperty("target")]
        public Target target { get; set; }

        [JsonProperty("projects")]
        public Projects projects { get; set; }

        [JsonProperty("derivatives")]
        public Derivatives derivatives { get; set; }

        [JsonProperty("downloadedFormats")]
        public DownloadedFormats downloadedFormats { get; set; }

        [JsonProperty("item")]
        public Item item { get; set; }

        [JsonProperty("resources")]
        public Resources resources { get; set; }

        [JsonProperty("created")]
        public Created created { get; set; }
        #endregion Properties

        #region Constructors
        [DebuggerStepThrough]
        public Relationships()
        {
        } // constructor

        #endregion Constructors

    } // class
} // namespace
