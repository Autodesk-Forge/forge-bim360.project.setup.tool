using System;
using System.IO;
using System.Data;
using System.Collections.Generic;

using BimProjectSetupCommon.Workflow;
using BimProjectSetupCommon.Helpers;

using Autodesk.Forge.BIM360.Serialization;
using Autodesk.Forge.BIM360;
using System.Runtime.Remoting.Messaging;

namespace CustomBIMFromCSV
{
    internal static class Tools
    {
        internal static NestedFolder CreateFoldersAndAssignPermissions(DataTable table, int rowIndex, List<HqUser> projectUsers, FolderWorkflow folderProcess, List<NestedFolder> folders, NestedFolder currentFolder, BimProject project, ProjectUserWorkflow projectUserProcess)
        {
            bool isUserAtRow = !string.IsNullOrEmpty(table.Rows[rowIndex]["user_email"].ToString());
            bool isRoleAtRow = !string.IsNullOrEmpty(table.Rows[rowIndex]["role_permission"].ToString());

            string folderColumnName = GetFolderColumnName(table, rowIndex);

            // Check if folder at this row
            if (folderColumnName != null)
            {
                string currFolderName = table.Rows[rowIndex][folderColumnName].ToString();

                List<string> currParentFolders = GetParentFolders(table, rowIndex, folderColumnName);

                // If currently subfolder
                if (currParentFolders.Count > 0)
                {
                    NestedFolder currParentFolder = FindParentFolder(currParentFolders, folders);
                    currentFolder = RecursiveFindFolder(currFolderName, currParentFolder.childrenFolders);

                    if (currentFolder == null)
                    {
                        currentFolder = CreateFolder(folderProcess, project.id, currParentFolder, currFolderName, currParentFolders[0]);
                    }
                    else
                    {
                        Util.LogInfo($"Folder in '{currParentFolders[0]}' already exists with name: {currFolderName}.");
                    }

                    if (isUserAtRow || isRoleAtRow)
                    {
                        AssignPermission(table, rowIndex, projectUsers, folderProcess, currentFolder, project, projectUserProcess);
                    }
                }
                // If currently a root folder
                else
                {
                    if (isUserAtRow || isRoleAtRow) 
                    {
                        Util.LogInfo($"\nCurrently at root folder '{currFolderName}'...\n");
                        NestedFolder rootFolder = folders.Find(x => x.name.ToLower() == currFolderName.ToLower());
                        AssignPermission(table, rowIndex, projectUsers, folderProcess, rootFolder, project, projectUserProcess);
                        currentFolder = rootFolder;
                    }
                }
            }
            // Asign user to folder if user at row but no folder at row
            else if ( (isUserAtRow || isRoleAtRow) && folderColumnName == null)
            {
                if (currentFolder != null)
                {
                    AssignPermission(table, rowIndex, projectUsers, folderProcess, currentFolder, project, projectUserProcess);
                }
            }

            return currentFolder;
        }

        internal static void UploadFilesFromFolder(DataTable table, int rowIndex, FolderWorkflow folderProcess, NestedFolder currentFolder, string projectId, string localFoldersPath)
        {

            bool isFolderAtRow = !string.IsNullOrEmpty(table.Rows[rowIndex]["local_folder"].ToString());
            string folderColumnName = GetFolderColumnName(table, rowIndex);

            // Check if folder at this row
            if (folderColumnName != null && isFolderAtRow)
            {
                UploadFiles(table, rowIndex, folderProcess, currentFolder, projectId, localFoldersPath);
            }
            // Upload files from folder if local folder given at row but no BIM360 folder at row
            else if (isFolderAtRow && folderColumnName == null)
            {
                if (currentFolder != null)
                {
                    UploadFiles(table, rowIndex, folderProcess, currentFolder, projectId, localFoldersPath);
                }
            }
        }

        internal static void UploadFiles(DataTable table, int rowIndex, FolderWorkflow folderProcess, NestedFolder folder, string projectId, string localFoldersPath)
        {
            string localFolderName = table.Rows[rowIndex]["local_folder"].ToString();
            if(!string.IsNullOrEmpty(localFolderName))
            {
                if (string.IsNullOrEmpty(localFoldersPath) == true)
                {
                    Util.LogError($"Argument -f for LocalFoldersPath is not given! The program has been stopped.");
                    throw new ApplicationException($"Stopping the program... You can see the log file for more information.");
                }

                string localFolderPath = localFoldersPath + @"\" + localFolderName;

                bool folderExist = Directory.Exists(localFolderPath);

                if (folderExist)
                {
                    Util.LogInfo($"Uploading files from local folder '{localFolderName}' to BIM360 folder '{folder.name}'...");

                    string[] filePaths = Directory.GetFiles(localFolderPath);

                    List<string> existingFileNames = folderProcess.CustomGetExistingFileNames(projectId, folder.id);

                    List<string> allowedFileTypesPlans = new List<string> { ".rvt", ".pdf", ".dwg", ".dwf", ".dwfx", ".ifc" };

                    foreach (string filePath in filePaths)
                    {
                        if (!existingFileNames.Contains(Path.GetFileName(filePath)))
                        {
                            // Find root folder
                            NestedFolder rootFolder = folder;
                            while (rootFolder.parentFolder != null)
                            {
                                rootFolder = rootFolder.parentFolder;
                            }

                            string fileExtension = Path.GetExtension(filePath);
                            if (rootFolder.name == "Plans" && !allowedFileTypesPlans.Contains(fileExtension))
                            {
                                Util.LogImportant($"File with name '{Path.GetFileName(filePath)}' is not a supported file type for folder 'Plans'. Skipping... Supported file types are: '.rvt', '.pdf', '.dwg', '.dwf', '.dwfx', '.ifc'.");
                                continue;
                            }

                            folderProcess.CustomUploadFile(projectId, folder.id, filePath);
                        }
                        else
                        {
                            Util.LogInfo($"File with name '{Path.GetFileName(filePath)}' already exists in folder '{folder.name}'. Skipping...");
                        }
                    }
                }
                else
                {
                    Util.LogImportant($"Local folder with name {localFolderName} does not exist! Check if the folder is placed in the correct directory. Skipping files...");
                }
            }
        }

        internal static List<string> CopyParentFolders(NestedFolder folder)
        {
            List<string> parentFolders = new List<string>();

            while (folder.parentFolder != null)
            {
                folder = folder.parentFolder;
                parentFolders.Add(folder.name);
            }

            return parentFolders;
        }

        internal static void AssignPermission(DataTable table, int rowIndex, List<HqUser> projectUsers, FolderWorkflow folderProcess, NestedFolder folder, BimProject project, ProjectUserWorkflow projectUserProcess)
        {
            // Add role permission
            string roleName = table.Rows[rowIndex]["role_permission"].ToString();
            if (!string.IsNullOrEmpty(roleName))
            {
                List<IndustryRole> roles = projectUserProcess.GetRolesForProject(project.name);
                IndustryRole role = roles.Find(x => x.name.ToLower() == roleName.ToLower());
                if(role != null)
                {
                    string letterPermission = table.Rows[rowIndex]["permission"].ToString();
                    AssignRolePermissionToFolder(folderProcess, folder, role, letterPermission, project.id);
                }
                else
                {
                    Util.LogImportant($"No role found with name: {roleName}. No permission for this role for folder '{folder.name}' will be assigned. See row number {rowIndex + 2} in the CSV-File.");
                }
            }

            // Add user permission
            string userEmail = table.Rows[rowIndex]["user_email"].ToString();
            if (!string.IsNullOrEmpty(userEmail))
            {
                HqUser existingUser = projectUsers.Find(x => x.email == userEmail);

                // Check if user exists
                if (existingUser != null)
                {
                    string letterPermission = table.Rows[rowIndex]["permission"].ToString();
                    AssignUserPermissionToFolder(folderProcess, folder, existingUser, letterPermission, project.id);
                }
                else
                {
                    Util.LogImportant($"No user found with email: {userEmail}. No permission for this user for folder '{folder.name}' will be assigned. See row number {rowIndex + 2} in the CSV-File.");
                }
            }
        }

        internal static void AssignRolePermissionToFolder(FolderWorkflow folderProcess, NestedFolder folder, IndustryRole role, string letterPermission, string ProjectId)
        {
            Util.LogInfo($"Assigning permission '{letterPermission}' to folder '{folder.name}' for role '{role.name}'.");

            List<string> permissionLevel = GetPermission(letterPermission);
            if(permissionLevel == null)
            {
                permissionLevel = new List<string> { "VIEW", "COLLABORATE" };
                Util.LogImportant($"Permission '{letterPermission}' for role '{role.name}' for folder '{folder.name}' was not recognized. Default permission 'V' is taken for this folder.");
            }

            List<FolderPermission> curr_permissions = new List<FolderPermission>();
            curr_permissions.Add(new FolderPermission()
            {
                subjectId = role.id,
                subjectType = "ROLE",
                actions = permissionLevel,
                inheritActions = permissionLevel
            });

            folderProcess.CustomAssignPermissionToFolder(ProjectId, folder.id, curr_permissions);
        }
        internal static void AssignUserPermissionToFolder(FolderWorkflow folderProcess, NestedFolder folder, HqUser user, string letterPermission, string ProjectId)
        {
            Util.LogInfo($"Assigning permission '{letterPermission}' to folder '{folder.name}' for user '{user.email}'.");

            List<string> permissionLevel = GetPermission(letterPermission);
            if (permissionLevel == null)
            {
                permissionLevel = new List<string> { "VIEW", "COLLABORATE" };
                Util.LogImportant($"Permission '{letterPermission}' for user '{user.email}' for folder '{folder.name}' was not recognized. Default permission 'V' is taken for this folder.");
            }
            List<FolderPermission> curr_permissions = new List<FolderPermission>();
            curr_permissions.Add(new FolderPermission()
            {
                subjectId = user.id,
                subjectType = "USER",
                actions = permissionLevel,
                inheritActions = permissionLevel
            });

            folderProcess.CustomAssignPermissionToFolder(ProjectId, folder.id, curr_permissions);
        }

        internal static void CheckRootsExist(List<NestedFolder> folderStructure)
        {
            string plansFolderId = RecursiveFindFolder("Plans", folderStructure).id;
            string projectFilesFolderId = RecursiveFindFolder("Project Files", folderStructure).id;

            if (plansFolderId == null || projectFilesFolderId == null)
            {
                throw new ApplicationException($"The project does't cointain the necessary root folders 'Plans' and 'Project Files'");
            }
        }

        internal static string GetFolderColumnName(DataTable table, int rowIndex)
        {
            int permissionColumnIndex = table.Columns.IndexOf("permission");

            foreach (DataColumn column in table.Columns)
            {
                // Check between the "project_type" and "permission"
                if (!string.IsNullOrEmpty(table.Rows[rowIndex][column].ToString()) && table.Columns.IndexOf(column) < permissionColumnIndex &&
                    table.Columns.IndexOf(column) > 1)
                {
                    return column.ColumnName;
                }
            }

            return null;
        }

        internal static List<string> GetParentFolders(DataTable table, int rowIndex, string columnName)
        {
            List<string> parentNames = new List<string>();

            if(columnName == "root_folder")
            {
                return parentNames;
            }

            int columnIndex = table.Columns.IndexOf(table.Columns[columnName]);
            while (columnIndex > 2 && rowIndex > 0)
            {
                // Find parent folder -> look in the previous columns up until name is found
                for (int i = rowIndex - 1; i >= 0; i--)
                {
                    // Parent folder must be at the previous column
                    if (!string.IsNullOrEmpty(table.Rows[i][columnIndex - 1].ToString()))
                    {
                        parentNames.Insert(0, table.Rows[i][columnIndex - 1].ToString());
                        rowIndex = i;
                        break;
                    }
                }

                columnIndex -= 1;
            }

            return parentNames;
        }

        internal static NestedFolder CreateFolder(FolderWorkflow folderProcess, string projectId, NestedFolder parentFolder, string folderName, string rootFolderName)
        {
            Util.LogInfo($"Creating folder in '{rootFolderName}' with name: {folderName}...");

            string newFolderId = folderProcess.CustomCreateFolder(projectId, parentFolder.id, folderName);

            NestedFolder subFolder = new NestedFolder(folderName, newFolderId, parentFolder);

            parentFolder.childrenFolders.Add(subFolder);

            return subFolder;
        }

        internal static NestedFolder FindParentFolder(List<string> parentFolders, List<NestedFolder> folderStructure)
        {

            NestedFolder resultFolder = null;

            for (int i = 0; i < parentFolders.Count; i++)
            {
                bool foundFolder = false;

                foreach (NestedFolder folder in folderStructure)
                {
                    if (folder.name.ToLower() == parentFolders[parentFolders.Count - 1].ToLower())
                    {
                        resultFolder = folder;
                    }

                    if (folder.name.ToLower() == parentFolders[i].ToLower())
                    {
                        foundFolder = true;
                        folderStructure = folder.childrenFolders;
                        break;
                    }
                }

                if (!foundFolder)
                {
                    Util.LogError($"No parent folder found in the structure!. Something went wrong. The program has been stopped.");
                    throw new ApplicationException($"Stopping the program... You can see the log file for more information.");
                }

            }

            return resultFolder;
        }

        internal static NestedFolder RecursiveFindFolder(string folderName, List<NestedFolder> folderStructure)
        {
            NestedFolder resultFolder = null;

            foreach (NestedFolder folder in folderStructure)
            {
                if (folder.name.ToLower() == folderName.ToLower())
                {
                    resultFolder = folder;
                    break;
                }
                if (folder.childrenFolders.Count > 0)
                {
                    resultFolder = RecursiveFindFolder(folderName, folder.childrenFolders);
                }
            }

            return resultFolder;
        }

        internal static void CheckProjectCreated(BimProject project, string projectName)
        {
            if (project == null)
            {
                Util.LogError($"There was a problem retrieving project with name: {projectName}. The program has been stopped.");
                throw new ApplicationException($"Stopping the program... You can see the log file for more information.");
            }
        }

        internal static List<string> GetPermission(string letterPermission)
        {
            if (letterPermission.ToUpper() == "V")
            {
                return new List<string> { "VIEW", "COLLABORATE" };
            }

            if (letterPermission.ToUpper() == "V+D")
            {
                return new List<string> { "VIEW", "DOWNLOAD", "COLLABORATE" };
            }

            if (letterPermission.ToUpper() == "U")
            {
                return new List<string> { "PUBLISH" };
            }

            if (letterPermission.ToUpper() == "V+D+U")
            {
                return new List<string> { "PUBLISH", "VIEW", "DOWNLOAD", "COLLABORATE" };
            }

            if (letterPermission.ToUpper() == "V+D+U+E")
            {
                return new List<string> { "PUBLISH", "VIEW", "DOWNLOAD", "COLLABORATE", "EDIT" };
            }

            if (letterPermission.ToUpper() == "FULL")
            {
                return new List<string> { "PUBLISH", "VIEW", "DOWNLOAD", "COLLABORATE", "EDIT", "CONTROL" };
            }

            return null;
        }

    }
}
