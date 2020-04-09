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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Autodesk.Forge.BIM360.Serialization
{
    [AttributeUsage (AttributeTargets.Property)]
    public sealed class InclInTemplAttribute : System.Attribute
    {
        public bool include = true;
        public InclInTemplAttribute(bool include = true)
        {
            this.include = include;
        }
    }

    public class BimProject
    {
        [JsonProperty(Required = Required.Always)]
        public string name { get; set; }
        public string service_types { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public string project_type { get; set; }
        public string value { get; set; }
        public string currency { get; set; }
        public string job_number { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string state_or_province { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public string business_unit_id { get; set; }
        public string timezone { get; set; }
        public string language { get; set; }
        public string construction_type { get; set; }
        public string contract_type { get; set; }
        [InclInTempl(false)]
        public string id { get; set; }
        [InclInTempl(false)]
        public string last_sign_in { get; set; }
        public string folders_created_from { get; set; }
        [JsonConverter(typeof(StringEnumConverter)), InclInTempl(false)]
        public Status status { get; set; }
        //TODO: below are new options in the API - needs further implementation and new testing
        [InclInTempl(false)]
        public string template_project_id { get; set; }
        [InclInTempl(false)]
        public Boolean include_locations { get; set; }
        [InclInTempl(false)]
        public Boolean include_companies { get; set; }

        // To exclude name properties when updating the existing project
        [JsonIgnore, InclInTempl(false)]
        public Boolean include_name_to_request_body { get; set; }

        // Added below method to dynamically exclude project name for serialization during the runtime
        public bool ShouldSerializename()
        {
            return (this.include_name_to_request_body);
        }

        public BimProject ShallowCopy()
        {
            return (BimProject)this.MemberwiseClone();
        }
    }

    public enum Status
    {
        active,
        pending,
        inactive,
        archived,
    }

}
