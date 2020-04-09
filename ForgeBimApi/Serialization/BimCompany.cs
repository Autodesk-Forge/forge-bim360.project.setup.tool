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

namespace Autodesk.Forge.BIM360.Serialization
{
    public class BimCompany
    {
        public string id { get; set; }
        [InclInTempl(false)]
        public string account_id { get; set; }
        public string name { get; set; }
        public string trade { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string postal_code { get; set; }
        public string state_or_province { get; set; }
        public string country { get; set; }
        public string phone { get; set; }
        public string website_url { get; set; }
        public string description { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string erp_id { get; set; }
        public string tax_id { get; set; }
    }

    public class BimCompaniesResponse
    {
        public int success;
        public int failure;
        public List<BimCompany> success_items;
        public List<BimCompaniesFailureItem> failure_items;
    }

    public class BimCompaniesFailureItem
    {
        public BimCompany item;
        public string error;
    }

}
