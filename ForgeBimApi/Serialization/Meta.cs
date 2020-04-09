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
  public class Meta
    {
        #region Properties

        [JsonProperty("link")]
        public Link link { get; set; }

        [JsonProperty("refType")]
        public string refType { get; set; }

        [JsonProperty("fromId")]
        public string fromId { get; set; }

        [JsonProperty("fromType")]
        public string fromType { get; set; }

        [JsonProperty("toId")]
        public string toId { get; set; }

        [JsonProperty("toType")]
        public string toType { get; set; }

        [JsonProperty("direction")]
        public string direction { get; set; }

        [JsonProperty("extension")]
        public Extension extension { get; set; }


        #endregion Properties

        #region Constructors

        [DebuggerStepThrough]
        public Meta()
        {
        } // constructor

        #endregion Constructors

    } // class
} // namespace

