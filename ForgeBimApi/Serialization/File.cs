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
using Newtonsoft.Json;

namespace Autodesk.Forge.BIM360.Serialization
{
    public class File
    {
        public File(
            string id,
            string fileGuid,
            string fileName,
            string fileUrn,
            string filePath,
            string fileType,
            string fileVersion,
            int publishedViewsCount,
            int publishedSheetsCount,
            long fileSize,
            string fileModifiedDate,
            string userCreate,
            string userLastModified,
            string processName
            )
        {
            Id = id;
            FileGuid = fileGuid;
            FileName = fileName;
            FileUrn = fileUrn;
            FilePath = filePath;
            FileType = fileType;
            FileVersion = fileVersion;
            PublishedViewsCount = publishedViewsCount;
            PublishedSheetsCount = publishedSheetsCount;
            FileSize = fileSize;
            FileModifiedDate = fileModifiedDate;
            UserCreate = userCreate;
            UserLastModified = userLastModified;
            ProcessName = processName;
            ExtractDate = DateTime.UtcNow.ToString();
        }

        private string Id = null;
        public string FileGuid { get; private set; }
        public string FileName { get; private set; }
        public string FileUrn { get; private set; }
        public string FilePath { get; private set; }
        public string FileType { get; private set; }
        public string FileVersion { get; private set; }
        public int PublishedViewsCount { get; private set; }
        public int PublishedSheetsCount { get; private set; }
        public long FileSize { get; private set; }
        public string FileModifiedDate { get; private set; }
        public string UserCreate { get; private set; }
        public string UserLastModified { get; private set; }
        public string ProcessName { get; private set; }
        public string ExtractDate { get; private set; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                if (Id != null)
                    hash = hash * 486187739 + Id.GetHashCode();
                if (FileGuid != null)
                    hash = hash * 486187739 + FileGuid.GetHashCode();
                if (FileName != null)
                    hash = hash * 486187739 + FileName.GetHashCode();
                if (FileUrn != null)
                    hash = hash * 486187739 + FileUrn.GetHashCode();
                if (FilePath != null)
                    hash = hash * 486187739 + FilePath.GetHashCode();
                if (FileType != null)
                    hash = hash * 486187739 + FileType.GetHashCode();
                if (FileVersion != null)
                    hash = hash * 486187739 + FileVersion.GetHashCode();
                hash = hash * 486187739 + PublishedViewsCount.GetHashCode();
                hash = hash * 486187739 + PublishedSheetsCount.GetHashCode();
                hash = hash * 486187739 + FileSize.GetHashCode();
                if (FileModifiedDate != null)
                    hash = hash * 486187739 + FileModifiedDate.GetHashCode();
                if (UserCreate != null)
                    hash = hash * 486187739 + UserCreate.GetHashCode();
                if (UserLastModified != null)
                    hash = hash * 486187739 + UserLastModified.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is File && Equals((File)obj);
        }

        public bool Equals(File f)
        {
            return Id == f.Id;
        }
    }
}
