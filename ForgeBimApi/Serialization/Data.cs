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
  public class Data
    {
        #region Properties

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("attributes")]
        public Attributes attributes { get; set; }

        [JsonProperty("links")]
        public Links links { get; set; }

        [JsonProperty("relationships")]
        public Relationships relationships { get; set; }

        [JsonProperty("meta")]
        public Meta meta { get; set; }

        #endregion Properties

        #region Constructors

        [DebuggerStepThrough]
        public Data()
        {
        } // constructor
        #endregion Constructors
    } // class
} // namespace
