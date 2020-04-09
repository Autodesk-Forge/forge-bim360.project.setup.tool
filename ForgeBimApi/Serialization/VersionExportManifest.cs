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
  public class VersionExportManifest
  {
        public string guid { get; set; }
        public string success { get; set; }
        public string hasThumbnail { get; set; }
        public string progress { get; set; }
        public string urn { get; set; }
        public string status { get; set; }
        public string startedAt { get; set; }
        public string region { get; set; }
        public string owner { get; set; }
        public string type { get; set; }
        public Child[] children { get; set; }
        
        public class Child
        {
            public string guid { get; set; }
            public string name { get; set; }
            public string success { get; set; }
            public string hasThumbnail { get; set; }
            public string role { get; set; }
            public string version { get; set; }
            public string progress { get; set; }
            public string urn { get; set; }
            public string status { get; set; }
            public string type { get; set; }
            public Child1[] children { get; set; }
        }

        public class Child1
        {
            public string guid { get; set; }
            public bool export_includeMarkups { get; set; }
            public string name { get; set; }
            public string success { get; set; }
            public string hasThumbnail { get; set; }
            public string export_urlType { get; set; }
            public string export_baseUrl { get; set; }
            public string progress { get; set; }
            public string status { get; set; }
            public object[] export_pageArray { get; set; }
            public string type { get; set; }
            public bool export_combineInput { get; set; }
            public bool export_includeCallouts { get; set; }
            public bool export_includeMarkupLinks { get; set; }
            public string timestamp { get; set; }
            public Child2[] children { get; set; }
        }

        public class Child2
        {
            public string guid { get; set; }
            public string role { get; set; }
            public string mime { get; set; }
            public string urn { get; set; }
            public string type { get; set; }
            public string documentUrn { get; set; }
        }
    } // class
} // namespace
