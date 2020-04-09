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

namespace Autodesk.BimProjectSetup.ForgeApi.Serialization
{
    public class AboutMe
    {
        public enum Language {
            cs, de, en, es, fr, hu, it, pl, pt_BR, ru, jp, zh_CN, zh_CW, ko
        }
        public string userId;
        public string userName;
        public string emailId;
        public string firstName;
        public string lastName;
        public string emailVerified;
        //public bool 2FaEnabled;
        public string countryCode;
        public Language language;
        public bool optin;
        public DateTime lastModified;
        public string websiteUrl;
    }
}
