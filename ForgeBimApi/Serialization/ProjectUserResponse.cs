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
using Newtonsoft.Json;

namespace Autodesk.Forge.BIM360.Serialization
{
    public class ProjectUserResponse : ResponseStats
    {
        public ProjectUserSuccessItem[] success_items;
        public ProjectUserFailureItem[] failure_items;
    }

    public class ProjectUserSuccessItem
    {
        public string email;
        public string company_id;
        public string[] industry_roles;
        public Services services;
        public string user_id;
        public string project_id;
        public string account_id;
        public Status status;
    }

    public class ProjectUserFailureItem : ProjectUserSuccessItem
    {
        public ResponseContent[] errors;
    }
}
