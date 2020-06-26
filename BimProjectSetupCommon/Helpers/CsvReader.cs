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
using System.IO;
using System.Data;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using NLog;
using Autodesk.Forge.BIM360.Serialization;

namespace BimProjectSetupCommon.Helpers
{
    internal static class CsvReader
    {
        private static Encoding Encoding { get; set; }
        private static char Separator { get; set; }

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private static Dictionary<string, List<IndustryRole>> _projectToRolesDict = new Dictionary<string, List<IndustryRole>>();
        private static Dictionary<string, BimProject> _nameToProjectMap = new Dictionary<string, BimProject>();

        internal static DataTable ReadFile(string filePath)
        {
            DataTable result = new DataTable();
            Log.Info("Reading file: " + filePath);
            using (StreamReader reader = new StreamReader(filePath, Encoding))
            {
                string line;
                int lineIndex = 0;

                while (null != (line = reader.ReadLine()))
                {
                    List<string> values = ParseLine(line);
                    if(values == null || values.Count == 0)
                    {
                        continue;
                    }

                    if (lineIndex == 0)
                    {
                        foreach (string value in values)
                        {
                            string columnName = value;

                            if (true == result.Columns.Contains(columnName))
                            {
                                columnName = columnName + "_1";
                            }
                            DataColumn col = new DataColumn(columnName, typeof(string));

                            result.Columns.Add(col);
                        }
                    }
                    else
                    {
                        DataRow row = result.NewRow();
                        int colIndex = 0;

                        foreach (string value in values)
                        {
                            if (colIndex < result.Columns.Count)
                            {
                                row[colIndex++] = value;
                            }
                        }
                        result.Rows.Add(row);
                    }
                    lineIndex++;
                }
                Log.Info("- total lines read from the csv file: " + (lineIndex - 1).ToString());
            }
            return result;
        }
        internal static DataTable ReadFile(string filePath, char separator, Encoding encoding)
        {
            if (separator != char.MinValue)
            {
                Separator = separator;
            }
            if (encoding != null)
            {
                Encoding = encoding;
            }
            DataTable result = ReadFile(filePath);
            return result;
        }
        internal static DataTable CustomReadDataFromCSV()
        {

            Console.WriteLine("Reading CSV-File...");

            if (false == string.IsNullOrEmpty(DataController._options.FilePath))
            {
                DataTable table = ReadFile(DataController._options.FilePath, DataController._options.Separator, DataController._options.Encoding);

                CustomCheckRequiredColumns(table);

                CustomCheckRequiredRows(table);

                return table;
            }
            else
            {
                throw new ApplicationException($"No FilePath for the CSV-File is given!\n");
            }
        }
        internal static void CustomCheckRequiredColumns(DataTable table)
        {

            Console.WriteLine("Checking required columns...");

            // Check if all required columns are existing
            // Only one level of subfolders is required - more than one is also possible but not required
            if (!table.Columns.Contains("project_name") || !table.Columns.Contains("level_1") ||
                !table.Columns.Contains("permission") || !table.Columns.Contains("user_email") || !table.Columns.Contains("company"))
            {
                throw new ApplicationException($"Not all required columns are presented in the CSV-File. Required columns are: " +
                    $"'project_name', 'level_1', 'permission', 'user_email' and 'company'\n");
            }

            // Check if the order of the columns is correct and if the company column is the last one
            else if (table.Columns.IndexOf("project_name") > table.Columns.IndexOf("level_1") ||
                table.Columns.IndexOf("level_1") > table.Columns.IndexOf("permission") || table.Columns.IndexOf("permission") > table.Columns.IndexOf("user_email") ||
                table.Columns.IndexOf("user_email") > table.Columns.IndexOf("company") || table.Columns.IndexOf("company") != table.Columns.Count -1)
            {
                throw new ApplicationException($"The columns in the CSV-File are not in the correct order. Please use the template!\n");
            }
        }
        internal static void CustomCheckRequiredRows(DataTable table)
        {

            Console.WriteLine("Checking required rows...");

            // First row must be a populated row
            if (string.IsNullOrEmpty(table.Rows[0]["project_name"].ToString()))
            {
                throw new ApplicationException($"The first row of the CSV-File must include a project.\n");
            }

            for (int i = 1; i < table.Rows.Count; i++)
            {
                // Check if emty row exists before each new project (without the first one)
                foreach (DataColumn column in table.Columns)
                {
                    if (!string.IsNullOrEmpty(table.Rows[i]["project_name"].ToString()) && !string.IsNullOrEmpty(table.Rows[i-1][column].ToString()))
                    {
                        throw new ApplicationException($"Before each new project an empty row is required in the CSV-File (not the first project).\n");
                    }

                }

                // Check if a user always corresponds to a company
                //if (!string.IsNullOrEmpty(table.Rows[i]["company"].ToString()) && string.IsNullOrEmpty(table.Rows[i]["user_email"].ToString()))
                //{
                //    return false;
                //}

                // Check if a permission alwas corresponds to a user and vice versa
                if ((!string.IsNullOrEmpty(table.Rows[i]["permission"].ToString()) && string.IsNullOrEmpty(table.Rows[i]["user_email"].ToString())) ||
                    (string.IsNullOrEmpty(table.Rows[i]["permission"].ToString()) && !string.IsNullOrEmpty(table.Rows[i]["user_email"].ToString())))
                {
                    throw new ApplicationException($"A permission must always corresponds to a user and vice versa. Please use the template!\n");
                }
            }
        }
        internal static DataTable ReadDataFromCSV(DataTable targetTable, string filePath)
        {
            if (targetTable != null) targetTable.Clear();
            if (false == string.IsNullOrEmpty(filePath))
            {
                targetTable = ReadFile(filePath, DataController._options.Separator, DataController._options.Encoding);
                targetTable.Columns.Add(new DataColumn("result"));
                targetTable.Columns.Add(new DataColumn("result_message"));
            }
            else
            {
                Log.Warn("CSV File is missing or the given path is wrong. Please check again.");
            }
            return targetTable;
        }
        internal static void ReadDataFromProjectCSV()
        {
            DataController._projectTable = ReadDataFromCSV(DataController._projectTable, DataController._options.FilePath);

            if (false == string.IsNullOrEmpty(DataController._options.FilePath))
            {
                // Change the delimeter of service_types from GlobalResource.delimeterForServiceType to comma
                if (DataController._projectTable.Columns.Contains("service_types"))
                {
                    foreach (DataRow row in DataController._projectTable.Rows)
                    {
                        var val = row["service_types"];
                        row["service_types"] = Convert.ToString(val).Replace(DefaultConfig.secondDelimiter, ',');
                    }
                }

                // Add columns if not existing
                PropertyInfo[] props = typeof(BimProject).GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    if (!DataController._projectTable.Columns.Contains(prop.Name))
                    {
                        DataController._projectTable.Columns.Add(new DataColumn(prop.Name));
                    }
                }
            }
        }
        private static List<string> ParseLine(string line)
        {
            List<string> values = new List<string>();
            string value = string.Empty;
            bool isQuote = false;
            char prevChar = ' ';

            foreach (char c in line)
            {
                if (c == '"')
                {
                    isQuote = !isQuote;
                }
                if ((c == Separator) && (false == isQuote))
                {
                    value = value.Trim();
                    values.Add(value);
                    value = string.Empty;
                }
                else
                {
                    bool add = true;

                    if ((c == '"') && prevChar != '"')
                    {
                        add = false;
                    }
                    if (true == add)
                    {
                        value += c;
                    }
                }
                prevChar = c;
            }
            value = value.Trim();
            if (0 < value.Length)
            {
                values.Add(value);
            }
            return values;
        }
    }
}
