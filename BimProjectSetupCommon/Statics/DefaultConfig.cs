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

namespace BimProjectSetupCommon.Helpers
{
    public static class DefaultConfig
    {
        public static char delimiter = ';';
        public static char secondDelimiter = ',';
        public static int limit = 100;
        public static int offset = 0;
        public static string invalidChars1 = @"[\\/?[\]>*:<|,]";
        public static string invalidChars2 = "[\"]";
        public static string dateFormat = "MM/dd/yyyy";
        public static string accountRegion = "US";
        public static string adminRole = "VDC Manager";
    }
}
