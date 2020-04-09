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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Autodesk.Forge.BIM360.Serialization
{
  public class Version
    {
        public JsonApi jsonapi { get; set; }
        public Data data { get; set; }

        public Version(string fileName, string storageId, string itemId)
        {
            jsonapi = new JsonApi
            {
                version = "1.0"
            };

            data = new Data
            {
                type = "versions",
                attributes = new Attributes
                {
                    name = fileName,
                    extension = new Extension
                    {
                        //type = "versions:autodesk.core:File",
                        type = "versions:autodesk.bim360:File",
                        version = "1.0"
                    }
                },
                relationships = new Relationships
                {
                    item = new Item
                    {
                        data = new Data
                        {
                            type = "items",
                            id = itemId
                        }
                    },
                    storage = new Storage
                    {
                        data = new Data
                        {
                            type = "objects",
                            id = storageId
                        }
                    }
                }
            };
        }
    } // class
} // namespace
