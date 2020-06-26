using System.Data;
using System.Collections.Generic;

using static CustomBIMFromCSV.Tools;

using BimProjectSetupCommon;
using BimProjectSetupCommon.Workflow;

using Autodesk.Forge.BIM360.Serialization;

namespace CustomBIMFromCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get options from command line arguments
            AppOptions options = AppOptions.Parse(args);

            ProjectWorkflow projectProcess = new ProjectWorkflow(options);
            FolderWorkflow folderProcess = new FolderWorkflow(options);
            ProjectUserWorkflow projectUserProcess = new ProjectUserWorkflow(options);
            AccountWorkflow accountProcess = new AccountWorkflow(options);

            DataTable csvData = projectProcess.CustomGetDataFromCsv();
            List<BimProject> projects = projectProcess.GetAllProjects();

            List<BimCompany> companies = null;
            BimProject currentProject = null;
            List<HqUser> projectUsers = null;
            List<NestedFolder> folders = null;
            List<NestedFolder> currentFolders = new List<NestedFolder>(); // One folder for each root folder

            // Itterate each row of the CSV-File
            for (int row = 0; row < csvData.Rows.Count; row++)
            {
                bool isUserAtRow = !string.IsNullOrEmpty(csvData.Rows[row]["user_email"].ToString());

                string projectName = csvData.Rows[row]["project_name"].ToString();
                // Find project or create one if not exists
                if (!string.IsNullOrEmpty(projectName))
                {
                    currentProject = projects.Find(x => x.name == projectName);

                    if (currentProject == null)
                    {
                        projectProcess.CustomCreateProject(projectName);
                        projects = projectProcess.GetAllProjects(); // Update after creation

                        currentProject = projects.Find(x => x.name == projectName);
                        CheckProjectCreated(currentProject, projectName);

                    } 

                    companies = accountProcess.GetCompanies();
                    accountProcess.CustomAddAllCompanies(csvData, companies, row);
                    companies = accountProcess.GetCompanies(); // Update after creation

                    projectUsers = projectUserProcess.CustomGetAllProjectUsers(currentProject.id);
                    projectUserProcess.CustomAddAllProjectUsers(csvData, projectUsers, companies, currentProject.name, row);
                    projectUsers = projectUserProcess.CustomGetAllProjectUsers(currentProject.id); // Update after creation

                    folders = folderProcess.CustomGetFolderStructure(currentProject);
                    CheckRootsExist(folders);

                    row = AssignPermissionToRootFolders(csvData, row, isUserAtRow, projectUsers, folderProcess, folders, currentProject.id);

                    isUserAtRow = !string.IsNullOrEmpty(csvData.Rows[row]["user_email"].ToString());  // Update for this row
                    
                }

                currentFolders = CreateFoldersAndAssignPermissions(csvData, row, isUserAtRow, projectUsers, folderProcess, folders, currentFolders, currentProject.id);

            }
        }
    }
}
