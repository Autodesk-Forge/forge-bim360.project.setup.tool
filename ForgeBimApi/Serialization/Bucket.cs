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

using System.Collections.Generic;

using Newtonsoft.Json;

namespace Autodesk.Forge.BIM360.Serialization
{

  public class Bucket : Base
  {
    #region Properties

    [JsonProperty("bucketKey")]
    public string bucketKey { get; set; }

    [JsonProperty("bucketOwner")]
    public string bucketOwner { get; set; }

    [JsonProperty("createdDate")]
    public long createdDate { get; set; }

    [JsonProperty("permissions")]
    public List<Permission> permissions { get; set; }

    [JsonProperty("authId")]
    public string authId { get; set; }

    [JsonProperty("access")]
    public string access { get; set; }

    [JsonProperty("policyKey")]
    public string policyKey { get; set; }

    public string objectId { get; set; }

    public string objectKey { get; set; }

    public string sha1 { get; set; }

    public int size { get; set; }

    public string contentType { get; set; }

    public string location { get; set; }

    #endregion Properties

    #region Constructors

    public Bucket()
    {

    } // constructor

    public Bucket(string sBucketKey, string sPolicyKey)
    {
      bucketKey = sBucketKey;
      policyKey = sPolicyKey;
    } // constructor

    #endregion Constructors

    #region Methods

    #endregion Methods

  } // class
} // namespace
