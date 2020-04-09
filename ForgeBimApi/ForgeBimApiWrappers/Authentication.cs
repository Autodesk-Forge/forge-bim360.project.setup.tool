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
using Autodesk.Forge;
using Autodesk.Forge.Client;
using NLog;

namespace Autodesk.Forge.BIM360
{
    public class Authentication
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        // Initialize the oAuth 2.0 client configuration fron enviroment variables
        // you can also hardcode them in the code if you want in the placeholders below
        private static Scope[] _scope = new Scope[] { Scope.DataSearch, Scope.DataCreate, Scope.DataRead, Scope.DataWrite, Scope.AccountRead, Scope.AccountWrite };

        public static string Authenticate(ApplicationOptions options)
        {
            try
            {
                TwoLeggedApi _twoLeggedApi = null;
                if (string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    _twoLeggedApi = new TwoLeggedApi();
                }else
                {
                    _twoLeggedApi = new TwoLeggedApi(options.BaseUrl);
                }
                
                ApiResponse<dynamic> bearer = _twoLeggedApi.AuthenticateWithHttpInfo(options.ForgeClientId, options.ForgeClientSecret, oAuthConstants.CLIENT_CREDENTIALS, _scope);
                if (bearer.StatusCode != 200)
                {
                    throw new Exception("Request failed! (with HTTP response " + bearer.StatusCode + ")");
                }                 
                if (bearer.Data == null)
                {
                    Log.Info("You were not granted a new access_token!");
                    return null;
                }
                // The call returned successfully and you got a valid access_token.
                string token = bearer.Data.token_type + " " + bearer.Data.access_token;
                return bearer.Data.access_token;
            }
            catch (Exception ex )
            {
                throw ex;
            }
        }
    }
}
