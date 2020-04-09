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

using System.Data;
using System.Collections.Generic;
using BimProjectSetupCommon.Helpers;
using Autodesk.Forge.BIM360.Serialization;

namespace BimProjectSetupCommon.Workflow
{
    public class AccountWorkflow : BaseWorkflow
    {
        public AccountWorkflow(AppOptions options) : base(options)
        {
            DataController.InitializeAccountUsers();
            DataController.InitializeCompanies();
        }

        public void AddCompaniesFromCsv()
        {
            Log.Info($"");
            Log.Info($"Updating companies...");

            DataController._companyTable = CsvReader.ReadDataFromCSV(DataController._companyTable, DataController._options.CompanyFilePath); 
            List<BimCompany> companies = CompanyTableToList(DataController._companyTable);
            DataController.AddCompanies(companies);
        }
        public void AddUsersFromCsv()
        {
            Log.Info($"");
            Log.Info($"Updating users...");

            DataController._accountUserTable = CsvReader.ReadDataFromCSV(DataController._accountUserTable, DataController._options.AccountUserFilePath);
            List<HqUser> users = UserTableToList(DataController._accountUserTable);
            DataController.AddAccountUsers(users);
        }

        public List<BimCompany> GetCompanies()
        {
            return DataController.Companies;
        }
        public List<HqUser> GetUsers()
        {
            return DataController.AccountUsers;
        }

        private List<BimCompany> CompanyTableToList(DataTable input)
        {
            List<BimCompany> companies = new List<BimCompany>();
            foreach (DataRow row in input.Rows)
            {
                BimCompany company = new BimCompany();

                company.name = Util.GetStringOrNull(row["name"]);
                company.trade = Util.GetStringOrNull(row["trade"]);
                company.address_line_1 = Util.GetStringOrNull(row["address_line_1"]);
                company.address_line_2 = Util.GetStringOrNull(row["address_line_2"]);
                company.city = Util.GetStringOrNull(row["city"]);
                company.state_or_province = Util.GetStringOrNull(row["state_or_province"]);
                company.postal_code = Util.GetStringOrNull(row["postal_code"]);
                company.country = Util.GetStringOrNull(row["country"]);
                company.phone = Util.GetStringOrNull(row["phone"]);
                company.website_url = Util.GetStringOrNull(row["website_url"]);
                company.description = Util.GetStringOrNull(row["description"]);
                company.erp_id = Util.GetStringOrNull(row["erp_id"]);
                company.tax_id = Util.GetStringOrNull(row["tax_id"]);

                companies.Add(company);
            }
            return companies;
        }
        private List<HqUser> UserTableToList(DataTable input)
        {
            List<HqUser> users = new List<HqUser>();
            foreach (DataRow row in input.Rows)
            {
                HqUser user = new HqUser();

                user.company_id = Util.GetStringOrNull(row["company_id"]);
                user.email = Util.GetStringOrNull(row["email"]);
                user.nickname = Util.GetStringOrNull(row["nickname"]);
                user.first_name = Util.GetStringOrNull(row["first_name"]);
                user.last_name = Util.GetStringOrNull(row["last_name"]);
                user.image_url = Util.GetStringOrNull(row["image_url"]);
                user.address_line_1 = Util.GetStringOrNull(row["address_line_1"]);
                user.address_line_2 = Util.GetStringOrNull(row["address_line_2"]);
                user.city = Util.GetStringOrNull(row["city"]);
                user.state_or_province = Util.GetStringOrNull(row["state_or_province"]);
                user.postal_code = Util.GetStringOrNull(row["postal_code"]);
                user.country = Util.GetStringOrNull(row["country"]);
                user.phone = Util.GetStringOrNull(row["phone"]);
                user.company = Util.GetStringOrNull(row["company"]);
                user.job_title = Util.GetStringOrNull(row["job_title"]);
                user.industry = Util.GetStringOrNull(row["industry"]);
                user.about_me = Util.GetStringOrNull(row["about_me"]);

                users.Add(user);
            }
            return users;
        }

        #region CSV export
        public void ExportCompaniesCsv(List<int> arrayOfIndices)
        {
            CsvExporter.ExportCompaniesCsv(arrayOfIndices);
        }
        public void ExportUsersCsv(List<int> arrayOfIndices)
        {
            CsvExporter.ExportUsersCsv(arrayOfIndices);
        }
        #endregion region
    }
}
