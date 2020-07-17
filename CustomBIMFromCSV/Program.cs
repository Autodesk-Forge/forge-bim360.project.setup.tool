using System.Data;
using System.Collections.Generic;

using static CustomBIMFromCSV.Tools;

using BimProjectSetupCommon;
using BimProjectSetupCommon.Workflow;
using BimProjectSetupCommon.Helpers;

using Autodesk.Forge.BIM360.Serialization;

namespace CustomBIMFromCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            // Delete previous versions of log.txt
            System.IO.File.Delete("Log/logInfo.txt");
            System.IO.File.Delete("Log/logImportant.txt");
            
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
            NestedFolder currentFolder = null;

            for (int row = 0; row < csvData.Rows.Count; row++)
            {
                string projectName = csvData.Rows[row]["project_name"].ToString();
                
                if (!string.IsNullOrEmpty(projectName))
                {
                    Util.LogImportant($"\nCurrent project: {projectName}");

                    currentProject = projects.Find(x => x.name == projectName);

                    if (currentProject == null)
                    {
                        projects = projectProcess.CustomCreateProject(csvData, row, projectName, projectProcess);

                        currentProject = projects.Find(x => x.name == projectName);
                        CheckProjectCreated(currentProject, projectName);
                    }

                    folders = folderProcess.CustomGetFolderStructure(currentProject);

                    companies = accountProcess.CustomUpdateCompanies(csvData, row, accountProcess);

                    projectUsers = projectUserProcess.CustomUpdateProjectUsers(csvData, row, companies, currentProject, projectUserProcess);
                }

                currentFolder = CreateFoldersAndAssignPermissions(csvData, row, projectUsers, folderProcess, folders, currentFolder, currentProject, projectUserProcess);

                UploadFilesFromFolder(csvData, row, folderProcess, currentFolder, currentProject.id, options.LocalFoldersPath);
            }
        }
    }
}
