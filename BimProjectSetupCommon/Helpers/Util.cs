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
using System.Globalization;
using System.Text.RegularExpressions;
using RestSharp;
using NLog;


namespace BimProjectSetupCommon.Helpers
{
    /// <summary>
    /// Static class that contains functions
    /// </summary>
    public static class Util
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        internal static IEnumerable<List<T>> SplitList<T>(List<T> myList, int nSize = 30)
        {
            for (int i = 0; i < myList.Count; i += nSize)
            {
                yield return myList.GetRange(i, Math.Min(nSize, myList.Count - i));
            }
        }

        internal static string GetStringOrNull(object value)
        {
            string s = null;
            s = Convert.ToString(value);
            {
                if (string.IsNullOrEmpty(s)) s = null;
            }
            return s;
        }

        internal static DateTime? GetDate(object date)
        {
            DateTime? result = null;
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                string[] formats = { "M/d/yyyy", "d/M/yyyy", "M-d-yyyy", "d-M-yyyy", "d-MMM-yy", "d-MMMM-yyyy", "yyyy-MM-dd" };

                DateTime d;
                DateTime.TryParseExact(date.ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out d);

                result = d;

                // Original code below
                //result = DateTime.ParseExact(Convert.ToString(date), _options.FormatPattern, provider);
            }
            catch { }
            return result;
        }

        internal static bool IsNameValid(string name)
        {
            bool result = string.IsNullOrWhiteSpace(name);
            if (!result)
            {
                result = Regex.IsMatch(name, DefaultConfig.invalidChars1);
            }
            if (!result)
            {
                result = Regex.IsMatch(name, DefaultConfig.invalidChars2);
            }
            return !result;
        }
    }
}
