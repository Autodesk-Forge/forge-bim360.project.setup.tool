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
using System.Linq;
using System.Text;
using BimProjectSetupCommon.Helpers;
using Autodesk.Forge.BIM360;

namespace BimProjectSetupCommon
{
    public class AppOptions : ApplicationOptions
    {
        public string FilePath { get; set; }
        public string ServiceFilePath { get; set; }
        public string ProjectUserFilePath { get; set; }
        public string CompanyFilePath { get; set; }
        public string AccountUserFilePath { get; set; }
        public string ForgeClientId { get; private set; }
        public string ForgeClientSecret { get; private set; }
        public string ForgeBimAccountId { get; private set; }
        public string BaseUrl { get; private set; }
        public char Separator
        {
            get
            {
                return DefaultConfig.delimiter;
            }
            set
            {
                DefaultConfig.delimiter = value;
            }
        }
        public char ServiceSeparator
        {
            get
            {
                return DefaultConfig.secondDelimiter;
            }
            set
            {
                DefaultConfig.secondDelimiter = value;
            }
        }
        public Encoding Encoding { get; private set; }
        public string FormatPattern
        {
            get
            {
                return DefaultConfig.dateFormat;
            }
            set
            {
                DefaultConfig.dateFormat = value;
            }
        }
        public bool TrialRun { get; private set; }
        public string HqAdmin { get; private set; }
        public bool CopyFolders { get; private set; }
        public bool UpdateProjectUsers { get; private set; }
        public string AccountRegion
        {
            get
            {
                return DefaultConfig.accountRegion;
            }
            set
            {
                DefaultConfig.accountRegion = value;
            }
        }
        public string AdminRole
        {
            get
            {
                return DefaultConfig.adminRole;
            }
            set
            {
                DefaultConfig.adminRole = value;
            }
        }
        internal AppOptions()
        {
            ForgeClientId = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID") ?? "your_client_id";
            ForgeClientSecret = Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET") ?? "your_client_secret";
            ForgeBimAccountId = Environment.GetEnvironmentVariable("FORGE_BIM_ACCOUNT_ID") ?? "your_account_id";
            Encoding = GetEncoding("UTF-8");
            TrialRun = false;
            CopyFolders = false;
            UpdateProjectUsers = false;
            AccountRegion = "US";
        }

        public static AppOptions Parse(string[] args)
        {
            AppOptions options = new AppOptions();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.Equals("-p", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.FilePath = args[++i];
                }
                else if (arg.Equals("-x", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.ServiceFilePath = args[++i];
                }
                else if (arg.Equals("-u", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.ProjectUserFilePath = args[++i];
                }
                else if (arg.Equals("-c", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.ForgeClientId = args[++i];
                }
                else if (arg.Equals("-s", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.ForgeClientSecret = args[++i];
                }
                else if (arg.Equals("-a", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.ForgeBimAccountId = args[++i];
                }
                else if (arg.Equals("-b", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.BaseUrl = args[++i];
                }
                else if (arg.Equals("-t", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.Separator = args[++i].ToCharArray()[0];
                }
                else if (arg.Equals("-z", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.ServiceSeparator = args[++i].ToCharArray()[0];
                }
                else if (arg.Equals("-e", StringComparison.CurrentCultureIgnoreCase))
                {
                    string encoding = args[++i];
                    options.Encoding = GetEncoding(encoding);
                }
                else if (arg.Equals("-d", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.FormatPattern = args[++i];
                }
                else if (arg.Equals("-r", StringComparison.InvariantCultureIgnoreCase))
                {
                    bool trial = false;
                    bool couldParse = Boolean.TryParse(args[++i], out trial);
                    if (couldParse) options.TrialRun = trial;
                }
                else if (arg.Equals("-h", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.HqAdmin = args[++i];
                }
                else if (arg.Equals("--CF", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.CopyFolders = true;
                }
                else if (arg.Equals("--UP", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.UpdateProjectUsers = true;
                }
                else if (arg.Equals("--AR", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.AdminRole = args[++i];
                }
                else if (arg.Equals("--EU", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.AccountRegion = args[i].Remove(0, 2);
                    ++i;
                }
            }

            return options;
        }
        private static Encoding GetEncoding(string name)
        {
            EncodingInfo[] encodingInfos = Encoding.GetEncodings();
            EncodingInfo encodingInfo = null;

            if (encodingInfos != null)
            {
                encodingInfo = encodingInfos.FirstOrDefault(e => e.Name != null && e.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                if (encodingInfo == null)
                {
                    int codePage;

                    if (Int32.TryParse(name, out codePage))
                    {
                        encodingInfo = encodingInfos.FirstOrDefault(e => e.CodePage == codePage);
                    }
                }
            }
            if (encodingInfo == null)
            {
                throw new ApplicationException("Invalid value for encoding. Either valid code page or encoding name must be provided.");
            }
            return encodingInfo.GetEncoding();
        }
    }
}
