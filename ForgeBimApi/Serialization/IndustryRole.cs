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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Autodesk.Forge.BIM360.Serialization
{
    public class IndustryRole
    {
        public IndustryRole() { }

        public string id;
        public string project_id;
        public string name;        
        // Changed
        public RolesServices services;
    }

    public class RolesServices
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RolesDocumentManagement document_management;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RolesProjectAdministration project_administration;

    }

    public class RolesProjectAdministration
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RolesAccessLevel access_level; // only allowed values are "admin" or "user"
    }

    public class RolesDocumentManagement : RolesProjectAdministration
    {
    }

    // Changed
    public enum RolesAccessLevel { admin, user, no_access };
}
