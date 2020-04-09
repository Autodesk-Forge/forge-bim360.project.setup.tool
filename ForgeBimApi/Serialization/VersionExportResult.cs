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
  public class VersionExportResult
  {
        public string result { get; set; }
        public string urn { get; set; }
        public Acceptedjobs acceptedJobs { get; set; }
        public string[] registerKeys { get; set; }

        public class Acceptedjobs
        {
            public Output output { get; set; }
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
            public string urlType { get; set; }
            public string baseUrl { get; set; }
        }
    } // class
} // namespace
