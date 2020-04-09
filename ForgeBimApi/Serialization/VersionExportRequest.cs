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

namespace Autodesk.Forge.BIM360.Serialization
{
  public class VersionExportRequest
    {
        public VersionExportRequest(string encodedVersionId, string urlType, string baseUrl, string containerId)
        {
            input = new Input
            {
                urn = encodedVersionId,
                dmExtensionType = "versions:autodesk.bim360:Document"                
            };

            output = new Output
            {
                formats = new Format[]
                {
                   new Format
                   {
                       type = "pdf",
                       advanced = new Advanced
                       {
                           includeCallouts = true,
                           includeMarkups = true,
                           includeMarkupLinks = true,
                           combineInputs = false,
                           urlType = urlType,
                           baseUrl = baseUrl,
                           container = containerId
                       }
                   }
                }
            };
        }

        public Input input { get; set; }
        public Output output { get; set; }

        public class Input
        {
            public string urn { get; set; }
            public string dmExtensionType { get; set; }

        }

        public class Output
        {
            public Format[] formats { get; set; }
        }

        public class Format
        {
            public string type { get; set; }
            public Advanced advanced { get; set; }
        }

        public class Advanced
        {
            public bool includeCallouts { get; set; }
            public bool includeMarkups { get; set; }
            public bool includeMarkupLinks { get; set; }
            public bool combineInputs { get; set; }
            public string urlType { get; set; }
            public string baseUrl { get; set; }
            public string container { get; set; }
        }
    } // class
} // namespace
