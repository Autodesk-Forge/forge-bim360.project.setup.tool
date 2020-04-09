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
    public class DMError
    {
        #region Properties

        public Jsonapi jsonapi { get; set; }
        public Error[] errors { get; set; }

        public class Jsonapi
        {
            public string version { get; set; }
        }

        public class Error
        {
            public string id { get; set; }
            public string status { get; set; }
            public string code { get; set; }
            public string detail { get; set; }
        }

        #endregion Properties

        #region Constructors

        [DebuggerStepThrough]
        public DMError()
        {
        } // constructor

        #endregion Constructors
    } // class     
} // namespace
