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

using System.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Autodesk.Forge.BIM360.Serialization
{
    public class Base
    {
        private string _sContent = string.Empty;
        [JsonIgnore]
        public string Content
        {
            get
            {
                string _sContent = string.Empty;
                JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.NullValueHandling = NullValueHandling.Ignore;

                string sContent = (string)JsonConvert.SerializeObject(this, jss);
                if (!string.IsNullOrEmpty(sContent))
                {
                    dynamic json = JValue.Parse(sContent);
                    _sContent = json.ToString();
                } // if
                else _sContent = string.Empty;

                return _sContent;
            } // get

            set { _sContent = value; }
        } // property Content

        [JsonIgnore]
        public string ErrorMessage { get; set; }

        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }

        [JsonIgnore]
        public string Request
        {
            get
            {
                JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.NullValueHandling = NullValueHandling.Ignore;

                string sRequest = (string)JsonConvert.SerializeObject(this, jss);
                return sRequest;
            } // get
        } // Content
    } // class
}
