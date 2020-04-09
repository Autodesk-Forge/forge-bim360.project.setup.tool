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
  public class Container
    {
        public string id { get; set; }
        public string ea_project_id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string root_urn { get; set; }
        public string urn { get; set; }
        public string normal_folder_urn { get; set; }
        public string recycle_folder_urn { get; set; }
        public string plans_folder_urn { get; set; }
        public string drawing_folder_urn { get; set; }
        public string photos_folder_urn { get; set; }
        public string project_tb_folder_urn { get; set; }
        public string status { get; set; }
        public string issue_container_id { get; set; }
        public string everyone_col_urn { get; set; }
        public Account_Status account_status { get; set; }
        public string account_id { get; set; }
        public Account_Features account_features { get; set; }
        public string account_display_name { get; set; }
        public bool locked { get; set; }
        public string checklist_container_id { get; set; }
        public object lbs_container_id { get; set; }
    } // class

    public class Account_Status
    {
        public string category { get; set; }
        public string price_model { get; set; }
    }

    public class Account_Features
    {
        public bool subfolder_access_control { get; set; }
        public bool upload_non_design_files { get; set; }
        public bool compare_2d_documents { get; set; }
        public bool multi_doc_viewing { get; set; }
    }
} // namespace
