using System;
using System.Data;
using System.Collections.Generic;

using BimProjectSetupCommon.Workflow;

using Autodesk.Forge.BIM360.Serialization;

namespace CustomBIMFromCSV
{
    internal static class Tools
    {

        internal static List<NestedFolder> CreateFoldersAndAssignPermissions(DataTable table, int rowIndex, bool isUserAtRow, List<HqUser> projectUsers, FolderWorkflow folderProcess, List<NestedFolder> folders, List<NestedFolder> currentFolders, string projectId)
        {
            string folderColumnName = GetFolderColumnName(table, rowIndex);
            List<string> rootFolders = new List<string>() { "Plans", "Project Files" };
            // Check if folder at this row
            if (folderColumnName != null)
            {
                currentFolders.Clear();
                string currFolderName = table.Rows[rowIndex][folderColumnName].ToString();

                for (int i = 0; i < rootFolders.Count; i++)
                {
                    List<string> currParentFolders = GetParentFolders(rootFolders[i], table, rowIndex, folderColumnName);

                    NestedFolder currParentFolder = FindParentFolders(currParentFolders, folders);

                    currentFolders.Add(RecursiveFindFolder(currFolderName, currParentFolder.childrenFolders));

                    if (currentFolders[i] == null)
                    {
                        currentFolders[i] = CreateFolder(folderProcess, projectId, currParentFolder, currFolderName);
                    }
                    else
                    {
                        Console.WriteLine("Folder exists with name: " + currFolderName);
                    }

                    if (isUserAtRow)
                    {
                        AssignPermission(table, rowIndex, projectUsers, folderProcess, currentFolders[i], projectId);
                    }
                }

            }

            // Asign user to folder if user at row but no folder at row
            else if (isUserAtRow && folderColumnName == null)
            {
                for (int i = 0; i < rootFolders.Count; i++)
                {
                    if (currentFolders[i] != null)
                    {
                        AssignPermission(table, rowIndex, projectUsers, folderProcess, currentFolders[i], projectId);
                    }
                }
            }

            return currentFolders;
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

        internal static int AssignPermissionToRootFolders(DataTable table, int rowIndex, bool isUserAtRow, List<HqUser> projectUsers, FolderWorkflow folderProcess, List<NestedFolder> folders, string projectId)
        {
            string firstSubFolder = GetFolderColumnName(table, rowIndex);
            while (firstSubFolder == null)
            {
                if (isUserAtRow)
                {
                    // Add permission to both 'Plans' and 'Project Files' folders
                    AssignPermission(table, rowIndex, projectUsers, folderProcess, folders[0], projectId);
                    AssignPermission(table, rowIndex, projectUsers, folderProcess, folders[1], projectId);
                }

                if (rowIndex + 1 >= table.Rows.Count)
                {
                    break;
                }

                rowIndex++;
                isUserAtRow = !string.IsNullOrEmpty(table.Rows[rowIndex]["user_email"].ToString());
                firstSubFolder = GetFolderColumnName(table, rowIndex);
            }

            return rowIndex;
        }

        internal static void AssignPermission(DataTable table, int rowIndex, List<HqUser> projectUsers, FolderWorkflow folderProcess, NestedFolder folder, string projectId)
        {
            string userEmail = table.Rows[rowIndex]["user_email"].ToString();
            HqUser existingUser = projectUsers.Find(x => x.email == userEmail);

            // Check if user exists
            if (existingUser != null)
            {
                string letterPermission = table.Rows[rowIndex]["permission"].ToString();
                AssignPermissionToFolder(folderProcess, folder, existingUser, letterPermission, projectId);
            }
            else
            {
                throw new ApplicationException($"There was a problem creating and retrieving user with email: " + userEmail);
            }
        }

        internal static void AssignPermissionToFolder(FolderWorkflow folderProcess, NestedFolder folder, HqUser user, string letterPermission, string ProjectId)
        {
            Console.WriteLine("Assigning permission to folder: " + folder.name + " for user: " + user.email);

            List<string> permissionLevel = GetPermission(letterPermission);
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
                // Check between the "project_name" and "permission"
                if (!string.IsNullOrEmpty(table.Rows[rowIndex][column].ToString()) && table.Columns.IndexOf(column) < permissionColumnIndex &&
                    table.Columns.IndexOf(column) > 0)
                {
                    return column.ColumnName;
                }
            }

            return null;
        }

        internal static List<string> GetParentFolders(string rootFolder, DataTable table, int rowIndex, string columnName)
        {
            List<string> parentNames = new List<string>();

            int columnIndex = table.Columns.IndexOf(table.Columns[columnName]);

            while (columnIndex > 0 && rowIndex > 0)
            {
                // Find parent folder -> look in the previous columns up until name is found
                for (int i = rowIndex - 1; i >= 0; i--)
                {
                    // Parent folder must be at the previous column
                    if (!string.IsNullOrEmpty(table.Rows[i][columnIndex - 1].ToString()))
                    {
                        // Root folder
                        if (table.Columns[columnIndex - 1].ColumnName == "project_name")
                        {
                            parentNames.Insert(0, rootFolder);
                            rowIndex = i;
                            break;
                        }
                        else
                        {
                            parentNames.Insert(0, table.Rows[i][columnIndex - 1].ToString());
                            rowIndex = i;
                            break;
                        }
                    }
                }

                columnIndex -= 1;
            }

            return parentNames;
        }

        internal static NestedFolder CreateFolder(FolderWorkflow folderProcess, string projectId, NestedFolder parentFolder, string folderName)
        {
            Console.WriteLine("Creating folder with name: " + folderName);

            string newFolderId = folderProcess.CustomCreateFolder(projectId, parentFolder.id, folderName);

            NestedFolder subFolder = new NestedFolder(folderName, newFolderId, parentFolder);

            parentFolder.childrenFolders.Add(subFolder);

            return subFolder;
        }

        internal static NestedFolder FindParentFolders(List<string> parentFolders, List<NestedFolder> folderStructure)
        {

            NestedFolder resultFolder = null;

            for (int i = 0; i < parentFolders.Count; i++)
            {
                bool foundFolder = false;

                foreach (NestedFolder folder in folderStructure)
                {
                    if (folder.name == parentFolders[parentFolders.Count - 1])
                    {
                        resultFolder = folder;
                    }

                    if (folder.name == parentFolders[i])
                    {
                        foundFolder = true;
                        folderStructure = folder.childrenFolders;
                        break;
                    }
                }

                if (!foundFolder)
                {
                    throw new ApplicationException($"No parent folder found in the structure!");
                }

            }

            return resultFolder;
        }

        internal static NestedFolder RecursiveFindFolder(string folderName, List<NestedFolder> folderStructure)
        {
            NestedFolder resultFolder = null;

            foreach (NestedFolder folder in folderStructure)
            {
                if (folder.name == folderName)
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
                throw new ApplicationException($"There was a problem retrieving new project with name: " + projectName);
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

            throw new ApplicationException($"No such permission found! Please check the CSV-File.");
        }

    }
}
