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
using System;

namespace BimProjectSetupCommon.Workflow
{
    public class AccountWorkflow : BaseWorkflow
    {
        public AccountWorkflow(AppOptions options) : base(options)
        {
            DataController.InitializeAccountUsers();
            DataController.InitializeCompanies();
        }
        public List<BimCompany> CustomUpdateCompanies(DataTable table, int startRow, AccountWorkflow accountProcess)
        {
            Util.LogInfo($"\nRetrieving companies...");
            List<BimCompany>  companies = accountProcess.GetCompanies();

            Util.LogInfo($"Adding companies...");
            List<BimCompany> _companies = CustomGetCompanies(table, companies, startRow);
            DataController.AddCompanies(_companies);

            List<BimCompany> updatedCompanies = accountProcess.GetCompanies();
            return updatedCompanies;
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
        private List<BimCompany> CustomGetCompanies(DataTable table, List<BimCompany> companies, int startRow)
        {
            if (table == null)
            {
                return null;
            }

            List<BimCompany> resultCompanies = new List<BimCompany>();

            // Create list with all existing company names
            List<string> existingCompanies = new List<string>();
            foreach (BimCompany existingCompany in companies)
            {
                existingCompanies.Add(existingCompany.name);
            }
            existingCompanies = existingCompanies.ConvertAll(d => d.ToLower());

            for (int row = startRow; row < table.Rows.Count; row++)
            {
                // Itterate until next project
                if (!string.IsNullOrEmpty(table.Rows[row]["project_name"].ToString()) && row != startRow)
                {
                    break;
                }

                if (string.IsNullOrEmpty(table.Rows[row]["company"].ToString()))
                {
                    continue;
                }

                BimCompany newCompany = new BimCompany();

                newCompany.name = Util.GetStringOrNull(table.Rows[row]["company"]);

                // Check if company with same name has been already added
                bool isCompanyAdded = false;
                foreach (BimCompany company in resultCompanies)
                {
                    if (company.name.ToLower() == newCompany.name.ToLower())
                    {
                        isCompanyAdded = true;
                        break;
                    }
                }

                // Check if there was a company with the same name already in the CSV-File
                if (row > 0)
                {
                    for (int j = row - 1; j>=0; j--)
                    {
                        if(table.Rows[row]["company"].ToString() == table.Rows[j]["company"].ToString())
                        {
                            isCompanyAdded = true;
                            if (!string.IsNullOrEmpty(table.Rows[row]["company_trade"].ToString()))
                            {
                                Util.LogImportant($"Company '{table.Rows[row]["company"]}' has already been added. Company trade is taken only from the first occurrence of the company.");
                            }
                        }
                    }
                }

                if (!isCompanyAdded)
                {
                    string trade = Util.GetStringOrNull(table.Rows[row]["company_trade"]);
                    List<string> allowedTrades = GetTrades();

                    allowedTrades = allowedTrades.ConvertAll(d => d.ToLower());

                    if (!allowedTrades.Contains(trade.ToLower()))
                    {
                        trade = "Architecture";
                        Util.LogImportant($"The given company trade '{table.Rows[row]["company_trade"]}' for company '{newCompany.name}' is not recognized. The default trade '{trade}' will be used. For reference see row number {row + 2} in the CSV-File.");
                    }

                    newCompany.trade = trade;

                    // Add only if company had not been added already and company does not already exist
                    if (newCompany != null && !isCompanyAdded && !existingCompanies.Contains(newCompany.name.ToLower())) resultCompanies.Add(newCompany);
                }
            }

            return resultCompanies;
        }
        private List<string> GetTrades()
        {
            List<string> trades = new List<string>();
            trades.Add("Architecture");
            trades.Add("Communications");
            trades.Add("Communications | Data");
            trades.Add("Concrete");
            trades.Add("Concrete | Cast-in-Place");
            trades.Add("Concrete | Precast");
            trades.Add("Construction Management");
            trades.Add("Conveying Equipment");
            trades.Add("Conveying Equipment | Elevators");
            trades.Add("Demolition");
            trades.Add("Earthwork");
            trades.Add("Earthwork | Site Excavation & Grading");
            trades.Add("Electrical");
            trades.Add("Electrical Power Generation");
            trades.Add("Electronic Safety & Security");
            trades.Add("Equipment");
            trades.Add("Equipment | Kitchen Appliances");
            trades.Add("Exterior Improvements");
            trades.Add("Exterior | Fences & Gates");
            trades.Add("Exterior | Landscaping");
            trades.Add("Exterior | Irrigation");
            trades.Add("Finishes");
            trades.Add("Finishes | Carpeting");
            trades.Add("Finishes | Ceiling");
            trades.Add("Finishes | Drywall");
            trades.Add("Finishes | Flooring");
            trades.Add("Finishes | Painting & Coating");
            trades.Add("Finishes | Tile");
            trades.Add("Fire Suppression");
            trades.Add("Furnishings");
            trades.Add("Furnishings | Casework & Cabinets");
            trades.Add("Furnishings | Countertops");
            trades.Add("Furnishings | Window Treatments");
            trades.Add("General Contractor");
            trades.Add("HVAC Heating, Ventilating, & Air Conditioning");
            trades.Add("Industry-Specific Manufacturing Processing");
            trades.Add("Integrated Automation");
            trades.Add("Masonry");
            trades.Add("Material Processing & Handling Equipment");
            trades.Add("Metals");
            trades.Add("Metals | Structural Steel / Framing");
            trades.Add("Moisture Protection");
            trades.Add("Moisture Protection | Roofing");
            trades.Add("Moisture Protection | Waterproofing");
            trades.Add("Openings");
            trades.Add("Openings | Doors & Frames");
            trades.Add("Openings | Entrances & Storefronts");
            trades.Add("Openings | Glazing");
            trades.Add("Openings | Roof Windows & Skylights");
            trades.Add("Openings | Windows");
            trades.Add("Owner");
            trades.Add("Plumbing");
            trades.Add("Pollution & Waste Control Equipment");
            trades.Add("Process Gas & Liquid Handling, Purification, & Storage Equipment");
            trades.Add("Process Heating, Cooling, & Drying Equipment");
            trades.Add("Process Integration");
            trades.Add("Process Integration | Piping");
            trades.Add("Special Construction");
            trades.Add("Specialties");
            trades.Add("Specialties | Signage");
            trades.Add("Utilities");
            trades.Add("Water & Wastewater Equipment");
            trades.Add("Waterway & Marine Construction");
            trades.Add("Wood & Plastics");
            trades.Add("Wood & Plastics | Millwork");
            trades.Add("Wood & Plastics | Rough Carpentry");

            return trades;
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
