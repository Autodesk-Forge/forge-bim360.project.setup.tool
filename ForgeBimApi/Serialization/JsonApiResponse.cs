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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Forge.BIM360.Serialization;

namespace Autodesk.Forge.BIM360.Serialization
{
    public class JsonApiResponse<T>
    {
        public class JsonApi
        {
            public string version { get; set; }
        }

        public class Links
        {
            public Self self { get; set; }
        }

        public class Self
        {
            public string href { get; set; }
        }

        public JsonApi jsonapi { get; set; }
        public Links links { get; set; }
        public T data { get; set; }
        public T included { get; set; }
    }
}
