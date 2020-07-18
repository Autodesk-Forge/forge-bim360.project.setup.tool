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

            Util.LogInfo("Reading CSV-File...");

            if (false == string.IsNullOrEmpty(DataController._options.FilePath))
            {
                DataTable table = ReadFile(DataController._options.FilePath, DataController._options.Separator, DataController._options.Encoding);

                CustomCheckRequiredColumns(table);

                CustomCheckRequiredRows(table);

                return table;
            }
            else
            {
                Util.LogError($"No FilePath for the CSV-File is given!\n");
                throw new ApplicationException($"Stopping the program... You can see the log file for more information.");
            }
        }
        internal static void CustomCheckRequiredColumns(DataTable table)
        {
            Util.LogInfo("\nChecking required columns...");

            bool isError = false;

            // Check if all required columns are existing
            if (!table.Columns.Contains("project_name") || !table.Columns.Contains("project_type") || !table.Columns.Contains("root_folder") ||
                !table.Columns.Contains("permission") || !table.Columns.Contains("role_permission") || !table.Columns.Contains("user_email") || !table.Columns.Contains("industry_role") || 
                !table.Columns.Contains("company") || !table.Columns.Contains("company_trade") || !table.Columns.Contains("local_folder"))
            {
                Util.LogError($"Not all required columns are presented in the CSV-File. Required columns are: " +
                    $"'project_name', 'project_type', 'root_folder', 'permission', 'role_permission', 'user_email', 'industry_role', 'company', 'company_trade' and 'local_folder'\n");
                isError = true;
            }

            // Check if the order of the columns is correct and if the local_folder column is the last one
            else if (table.Columns.IndexOf("project_name") > table.Columns.IndexOf("project_type") || table.Columns.IndexOf("project_type") > table.Columns.IndexOf("root_folder") ||
                table.Columns.IndexOf("root_folder") > table.Columns.IndexOf("permission") || table.Columns.IndexOf("permission") > table.Columns.IndexOf("role_permission") ||
                table.Columns.IndexOf("role_permission") > table.Columns.IndexOf("user_email") ||
                table.Columns.IndexOf("user_email") > table.Columns.IndexOf("industry_role") || table.Columns.IndexOf("industry_role") > table.Columns.IndexOf("company") ||
                table.Columns.IndexOf("company") > table.Columns.IndexOf("company_trade") || table.Columns.IndexOf("company_trade") > table.Columns.IndexOf("local_folder")  ||
                table.Columns.IndexOf("local_folder")  != table.Columns.Count -1)
            {
                Util.LogError($"The columns in the CSV-File are not in the correct order or unrecognized columns exist. Please use the template!\n");
                isError = true;
            }

            if (isError)
            {
                throw new ApplicationException($"Stopping the program... You can see the log file for more information.");
            }

        }
        internal static void CustomCheckRequiredRows(DataTable table)
        {

            Util.LogInfo("Checking required rows...");

            bool isError = false;

            // First row must be a populated row
            if (string.IsNullOrEmpty(table.Rows[0]["project_name"].ToString()))
            {
                Util.LogError($"The first row of the CSV-File must include a project.\n");
                isError = true;
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                // Check if emty row exists before each new project (without the first one)
                if (i > 0)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        if (!string.IsNullOrEmpty(table.Rows[i]["project_name"].ToString()) && !string.IsNullOrEmpty(table.Rows[i - 1][column].ToString()))
                        {
                            Util.LogError($"Before each new project an empty row is required in the CSV-File (not the first project). See row number {i + 2} in the CSV-File.\n");
                            isError = true;
                        }

                    }
                }

                // Check if a project_type is always available to a project
                if (!string.IsNullOrEmpty(table.Rows[i]["project_name"].ToString()) && string.IsNullOrEmpty(table.Rows[i]["project_type"].ToString()))
                {
                    Util.LogError($"Each project must have a project type assinged to it. See row number {i + 2} in the CSV-File.\n");
                    isError = true;
                }

                // Check if a company_trade is always available to a company for the first row of a specific company
                if (!string.IsNullOrEmpty(table.Rows[i]["company"].ToString()) && string.IsNullOrEmpty(table.Rows[i]["company_trade"].ToString()))
                {
                    bool isFirstTimeCompany = true;
                    if (i > 0)
                    {
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (table.Rows[i]["company"].ToString() == table.Rows[j]["company"].ToString())
                            {
                                isFirstTimeCompany = false;
                            }
                        }
                    }

                    if (isFirstTimeCompany)
                    {
                        Util.LogError($"Each company must have a company trade assinged to it. See row number {i + 2} in the CSV-File.\n");
                        isError = true;
                    }
                }

                // Check if a permission to each user if at least root_folder (if not at least root_folder -> user assigned only to project)
                if (!string.IsNullOrEmpty(table.Rows[i]["user_email"].ToString()))
                {
                    bool isRootFolder = false;
                    for (int j = i; j >= 0; j--)
                    {
                        if (!string.IsNullOrEmpty(table.Rows[j]["root_folder"].ToString()))
                        {
                            isRootFolder = true;
                        }
                    }
                    if (isRootFolder && string.IsNullOrEmpty(table.Rows[i]["permission"].ToString()))
                    {
                        Util.LogError($"A permission must always correspond to a user if at least there is root_folder. " +
                            $"Delete all folders to assign users only to a project or add permission for each user. See row number {i + 2} in the CSV-File.\n");

                        isError = true;
                    }
                }

                // Check if a permission always corresponds to a user or role
                if ((string.IsNullOrEmpty(table.Rows[i]["permission"].ToString()) && !string.IsNullOrEmpty(table.Rows[i]["user_email"].ToString())) ||
                    (string.IsNullOrEmpty(table.Rows[i]["permission"].ToString()) && !string.IsNullOrEmpty(table.Rows[i]["role_permission"].ToString())))
                {
                    // Check if folder at this row
                    bool isFolderAtRow = false;
                    for (int numCol = table.Columns.IndexOf("root_folder"); numCol < table.Columns.IndexOf("permission"); numCol++)
                    {
                        if (!string.IsNullOrEmpty((table.Rows[i].ItemArray[numCol].ToString())))
                        {
                            isFolderAtRow = true;
                        }
                    }
                    if (isFolderAtRow)
                    {
                        Util.LogError($"A permission must always correspond to a user or role for a certain folder. Please use the template! See row number {i + 2} in the CSV-File.\n");
                        isError = true;
                    }

                    if(!isFolderAtRow && !string.IsNullOrEmpty(table.Rows[i]["role_permission"].ToString()))
                    {
                        Util.LogError($"A 'role_permission' must always correspond to a folder. See row number {i + 2} in the CSV-File.\n");
                        isError = true;
                    }
                }

                // Check if user or role corresponds always to a permission
                if (!string.IsNullOrEmpty(table.Rows[i]["permission"].ToString()) && string.IsNullOrEmpty(table.Rows[i]["user_email"].ToString()) &&
                    string.IsNullOrEmpty(table.Rows[i]["role_permission"].ToString()))
                {
                    Util.LogError($"A user or role must always correspond to a permission. Please use the template! See row number {i + 2} in the CSV-File.\n");
                    isError = true;
                }

                // Only allowed values for 'root_folder': 'Plans' and 'Project Files'
                if (!string.IsNullOrEmpty(table.Rows[i]["root_folder"].ToString()) && 
                    table.Rows[i]["root_folder"].ToString().ToLower() != "plans" && table.Rows[i]["root_folder"].ToString().ToLower() != "project files")
                {
                    Util.LogError($"Only allowed values for 'root_folder' are: 'Plans' and 'Project Files'! See row number {i + 2} in the CSV-File.\n");
                    isError = true;
                }
            }

            if (isError)
            {
                throw new ApplicationException($"Stopping the program... You can see the log file for more information.");
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
