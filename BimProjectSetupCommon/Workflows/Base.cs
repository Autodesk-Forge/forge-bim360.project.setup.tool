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
using Autodesk.Forge.BIM360;
using NLog;
using BimProjectSetupCommon.Helpers;

namespace BimProjectSetupCommon.Workflow
{
    public class BaseWorkflow
    {
        internal static Logger Log = LogManager.GetCurrentClassLogger();
        internal static string _token = null;
        internal static AppOptions _options = null;
        internal static DateTime StartAuth { get; set; }

        public BaseWorkflow(AppOptions options)
        {
            _options = options;
            DataController._options = options;
        }

        public string GetToken()
        {
            if (_token == null || ((DateTime.Now - StartAuth) > TimeSpan.FromMinutes(30)))
            {
                _token = Authentication.Authenticate(DataController._options);
                StartAuth = DateTime.Now;
                return _token;
            }
            else return _token;
        }
    }
}
