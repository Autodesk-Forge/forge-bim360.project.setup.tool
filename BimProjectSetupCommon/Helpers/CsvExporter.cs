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
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Autodesk.Forge.BIM360.Serialization;
using System.Globalization;

namespace BimProjectSetupCommon.Helpers
{
    internal static class CsvExporter
    {

        public static bool ExploreFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }           
            filePath = System.IO.Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
            return true;
        }

        private static void WriteFileAndExplore(string csv, string name)
        {
            Random rnd = new Random();
            string rand = string.Empty;
            for (var i = 0; i < 10; i++)
            {
                rand += ((char)(rnd.Next(1, 26) + 64)).ToString();
            }
            string path = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}{name}_{rand}.csv";
            System.IO.File.WriteAllText(path, csv);
            ExploreFile(path);
        }

        private static void AppendProperty(StringBuilder csv, PropertyInfo prop, Object o)
        {
            if (prop.PropertyType == typeof(DateTime))
            {
                DateTime date = (DateTime)prop.GetValue(o);
                string formatString = DefaultConfig.dateFormat;
                string d = date != null ? date.ToString(format: formatString, provider: CultureInfo.InvariantCulture) : string.Empty;
                csv.Append(d);
            }
            else
            {
                csv.Append(prop.GetValue(o));
            }
        }

        internal static void ExportObjectsToCsv(IEnumerable<Object> objects, Type type, string baseFileName)
        {
            StringBuilder csv = new StringBuilder();
            PropertyInfo[] props = type.GetProperties();

            foreach (PropertyInfo prop in props)
            {
                // when templates are exported some properties need to be excluded
                // this is done by annotating the Property of the type with [InclInTemplAttribute(false)]
                InclInTemplAttribute att = (InclInTemplAttribute)prop.GetCustomAttribute(typeof(InclInTemplAttribute), false);
                
                if(objects == null && att!= null && false == att.include)
                {
                    continue;
                }
                csv.Append(prop.Name);
                csv.Append(DefaultConfig.delimiter);
            }
            if(csv.Length > 1) csv.Remove(csv.Length - 1, 1);
            csv.AppendLine();

            // If Export CSV and CSV with Services buttons are clicked
            if (objects != null)
            {
                //create rows in CSV
                foreach (Object project in objects)
                {
                    foreach (PropertyInfo prop in props)
                    {
                        AppendProperty(csv, prop, project);
                        csv.Append(DefaultConfig.delimiter);
                    }
                    if (csv.Length > 1) csv.Remove(csv.Length - 1, 1);
                    csv.AppendLine();
                }
            }
            WriteFileAndExplore(csv.ToString(), baseFileName);
        }

        internal static void ExportProjectsCsv(List<BimProject> projects)
        {
            ExportObjectsToCsv(projects, typeof(BimProject), "BimProject");
        }


        internal static void ExportCompaniesCsv(List<int> arrayOfIndices)
        {
            // Collect selected company objects from the list
            List<BimCompany> companies = new List<BimCompany>();
            arrayOfIndices.ForEach((index) => companies.Add(DataController.Companies[index]));

            ExportObjectsToCsv(companies, typeof(BimCompany), "BIM360_AccountCompanies");

        }

        internal static void ExportUsersCsv(List<int> arrayOfIndices)
        {
            List<HqUser> users = new List<HqUser>();
            arrayOfIndices.ForEach((index) => users.Add(DataController.AccountUsers[index]));

            // Create Column Headers
            string csv = "";

            PropertyInfo[] props = typeof(HqUser).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                csv += prop.Name.ToString() + DefaultConfig.delimiter;
            }

            csv += Environment.NewLine;

            // If Export CSV and CSV with Services buttons are clicked
            if (users != null)
            {
                //create rows in CSV
                foreach (HqUser user in users)
                {
                    foreach (PropertyInfo prop in props)
                    {
                        if (prop.Name.ToString() == "last_sign_in" || prop.Name.ToString() == "created_at" || prop.Name.ToString() == "updated_at")
                        {
                            try
                            {
                                DateTime date = (DateTime)prop.GetValue(user);
                                string d = date.ToString(DefaultConfig.dateFormat);
                                csv += d + DefaultConfig.delimiter.ToString();
                                continue;
                            }
                            catch
                            {
                                csv += "error parsing date" + DefaultConfig.delimiter.ToString();
                                continue;
                            }

                        }

                        if (prop.Name.ToString() == "phone")
                        {
                            // Collect phone number from the nested phone object
                            string phone_string = (string)prop.GetValue(user);
                            try
                            {
                                Phone phone = JsonConvert.DeserializeObject<Phone>(phone_string);
                                if (phone != null)
                                    csv += phone.Number.ToString() + DefaultConfig.delimiter.ToString();
                                else
                                    csv += DefaultConfig.delimiter.ToString();
                                continue;
                            }
                            catch
                            {
                                csv += phone_string + DefaultConfig.delimiter.ToString();
                                continue;
                            }
                        }

                        if (prop.Name.ToString() == "about_me")
                        {
                            string about_me = (string)prop.GetValue(user);
                            if(about_me != null)
                            {
                                if (about_me.Length == 0)
                                {
                                    csv += DefaultConfig.delimiter.ToString();
                                    continue;
                                }

                                if (DefaultConfig.delimiter != ',')
                                {
                                    csv += about_me.ToString() + DefaultConfig.delimiter.ToString();
                                    continue;
                                }

                                csv += about_me.Replace(',', ' ') + DefaultConfig.delimiter.ToString();
                                continue;
                            }
                            else
                            {
                                csv += DefaultConfig.delimiter.ToString();
                                continue;
                            }

                        }

                        csv += prop.GetValue(user) + DefaultConfig.delimiter.ToString();
                    }
                    //foreach (FieldInfo field in fields)
                    //{
                    //    if (field.Name.ToString() == "last_sign_in" || field.Name.ToString() == "created_at" || field.Name.ToString() == "updated_at")
                    //    {
                    //        DateTime date = (DateTime)field.GetValue(user);
                    //        string d = date.ToString("yyyy-MM-dd");
                    //        csv += d + Config.delimiter.ToString();
                    //    }
                    //    else
                    //    {
                    //        csv += field.GetValue(user) + Config.delimiter.ToString();
                    //    }
                    //}

                    csv += Environment.NewLine;
                }
            }

            Random rnd = new Random();
            int length = 20;
            var fileName = "";
            for (var i = 0; i < length; i++)
            {
                fileName += ((char)(rnd.Next(1, 26) + 64)).ToString();
            }
            string path = @"c:\temp\BIM360_AccountUser_" + fileName + ".csv";

            System.IO.File.WriteAllText(path, csv);
            System.Diagnostics.Process.Start(path);
        }

        internal static void ExportServicesCsvTemplate()
        {
            // Create Column Headers
            StringBuilder csv = new StringBuilder();

            csv.Append("project_name").Append(DefaultConfig.delimiter);
            csv.Append("service_type").Append(DefaultConfig.delimiter);
            csv.Append("company").Append(DefaultConfig.delimiter);
            csv.Append("email").Append(DefaultConfig.delimiter);

            WriteFileAndExplore(csv.ToString(), "BIM360_Service_Template");
        }

        internal static void ExportUsersCsvTemplate()
        {
            // Create Column Headers
            StringBuilder csv = new StringBuilder();
            csv.Append("project_name").Append(DefaultConfig.delimiter);
            csv.Append("email").Append(DefaultConfig.delimiter);
            csv.Append("pm_access").Append(DefaultConfig.delimiter);
            csv.Append("docs_access").Append(DefaultConfig.delimiter);
            csv.Append("company_name").Append(DefaultConfig.delimiter);
            csv.Append("industry_roles");

            WriteFileAndExplore(csv.ToString(), "BIM360_ProjectUser_Template");
        }

        internal static void WriteResults(DataTable dt, AppOptions _options, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
            sb.AppendLine(string.Join(_options.Separator.ToString(), columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(_options.Separator.ToString(), fields));
            }

            string name = Path.GetFileNameWithoutExtension(filePath);
            string path = Path.GetDirectoryName(filePath);
            string newName = $"{name}_processed{Path.GetExtension(filePath)}";
            string fileName = Path.Combine(path, newName);

            System.IO.File.WriteAllText(fileName, sb.ToString(), _options.Encoding);
        }
    }
}
