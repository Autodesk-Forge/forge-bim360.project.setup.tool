# forge-bim360.project.setup.tool

![Platforms](https://img.shields.io/badge/platform-Windows-lightgray.svg)
![.NET](https://img.shields.io/badge/.NET-4.6-blue.svg)
[![CefSharp](https://img.shields.io/badge/CefSharp-57.0.0-blue.svg)](http://opensource.org/licenses/MIT)

[![Data-Management](https://img.shields.io/badge/Data%20Management-v2-green.svg)](http://developer.autodesk.com/)
[![BIM-360](https://img.shields.io/badge/BIM%20360-v1-green.svg)](http://developer.autodesk.com/)

![Advanced](https://img.shields.io/badge/Level-Advanced-red.svg)
[![MIT](https://img.shields.io/badge/License-MIT-blue.svg)](http://opensource.org/licenses/MIT)



# Description
Command line tool to setup BIM 360 projects, services, users, it includes the following functionalities:
- Create Projects based on CSV input file
- Copy Projects with folders structure from a template project(supporting copy role permission)
- Activate project services by adding project admin
- Import users to project as admin or normal user

### Custom functionalities based on CSV input file:
 - Create projects with custom folder structure
 - Add users to project with industry roles
 - Add companies
 - Add folder permissions to users
 - Add folder permissions to roles
 - Upload multiple files from local folders to BIM360 folders

# Thumbnail
![thumbnail](/thumbnail.gif)

# Prerequisites
- Visual Studio: Either Community (Windows) or Code (Windows, MacOS).
- CefSharp: Chromium based browser control for .NET apps
- .NET Framework basic knowledge with C#

# Running locally
- For using this sample, you need an Autodesk developer credentials. Visit the [Forge Developer Portal](https://developer.autodesk.com), sign up for an account, then [create an app](https://developer.autodesk.com/myapps/create). 
- Connect your Forge App to a Specific BIM 360 Account, follow the [tutorial](https://forge.autodesk.com/en/docs/bim360/v1/tutorials/getting-started/get-access-to-account/)
- Download the repository, open `BimProjectSetupTool.sln` Solution on Visual Studio. The build process should download the required packages (**Autodesk.Forge** and dependencies). Compile and build the project, run it, please check the [User Guide](BIM360-ProjectSetupTool-UsageGuide.pdf) for all the details about usage and preparation of csv files.

# Hot to use Custom.bat
- See the steps from 'Running locally' to build the project
- Go to **.bin/Debug/sample** folder
- Edit the **Custom.bat** file and add your details
- There are two files **BIM360_Custom_Template**, one is a CSV-File and the other one an Excel-File
- You can edit the Excel-File for your purpose and save it as CSV-File. You can also directy change the CSV-File. Remeber: **the program uses only the CSV-File, so be sure to have the information saved in this file before starting the program**.
- After changing the BIM360_Custom_Template run the **Custom.bat** file
- For information on what the program has done you can go to **.bin/Debug/Log** folder read the log files. **logImportant.txt** should be always checked as it provides information of default values or skipped parameters.
- You will see an error if the CSV-File was not correctly populated. Specific information about the error will be printed on the console and will be saved in the log files.
- For every run of the program the log files will be deleted so you can't see old log files, just the last ones.

# Important notes on Custom.bat
- If you just want to add users to a project delete all the information in the columns from "root_folder" to "role_permission", but don't delete the columns.
- After running the programm it is still possible to add new information in the CSV-File and run the program again. Everything is checked and only the new information will be updated in BIM360.
- Soon there will be a tutorial video with more information

# Features
![features](./UseCases.png)

This sample app includes a few features as detailed in [User Guide](BIM360-ProjectSetupTool-UsageGuide.pdf), in short, please check the following video for the introduction, or follow the scripts directly to create projects, activate services, import project users.

[![https://youtu.be/dAIn9ANPYrE](http://img.youtube.com/vi/dAIn9ANPYrE/0.jpg)](https://youtu.be/dAIn9ANPYrE "BIM 360 project setup tool - introductioon")

Always replace the capitalized content in braces with your values before executing the script:
 ```powershell
 "<YOUR-CLIENT-ID>" - e.g. "LI8CLkyS4zO4OIvRPW2nPzR2UHk0zU7"
 etc.
 ```

## Feature 1: Create BIM360 projects:
 ```powershell
Autodesk.BimProjectSetup.exe -p ".\sample\BIM360_Projects_Template.csv" -c "<YOUR-CLIENT-ID>" -s "<YOUR-CLIENT-SECRET>" -a "<ACCOUNT-ID>" -b "https://developer.api.autodesk.com" -t ";" -z "," -e "UTF-8" -d "yyyy-MM-dd" -r false
```
Watch the following video:

[![https://youtu.be/pU40CrG5Gys](http://img.youtube.com/vi/pU40CrG5Gys/0.jpg)](https://youtu.be/pU40CrG5Gys "BIM 360 project setup tool - create projects")

## Feature 2: Create BIM360 projects from template project:
 ```powershell
Autodesk.BimProjectSetup.exe -p ".\sample\BIM360_Projects_CF_Template.csv" -c "<YOUR-CLIENT-ID>" -s "<YOUR-CLIENT-SECRET>" -a "<ACCOUNT-ID>" -b "https://developer.api.autodesk.com" -t ";" -z "," -e "UTF-8" -d "yyyy-MM-dd" -r false -h "admin@company.com" --CF
```
Watch the following video:

[![https://youtu.be/QTSUuhMqX7o](http://img.youtube.com/vi/QTSUuhMqX7o/0.jpg)](https://youtu.be/QTSUuhMqX7o "BIM 360 project setup tool - create project from template")

## Feature 3: Activate Services by Adding User as Admin
 ```powershell
Autodesk.BimProjectSetup.exe -s ".\sample\BIM360_Service_Template.csv" -c "<YOUR-CLIENT-ID>" -s "<YOUR-CLIENT-SECRET>" -a "<ACCOUNT-ID>" -b "https://developer.api.autodesk.com" -t ";" -z "," -e "UTF-8" -d "yyyy-MM-dd" -r false
```
Watch the following video:

[![https://youtu.be/iEz8cGIayeM](http://img.youtube.com/vi/iEz8cGIayeM/0.jpg)](https://youtu.be/iEz8cGIayeM "BIM 360 project setup tool - activate services")

## Feature 4: Add Users to Project as Admin or User
 ```powershell
Autodesk.BimProjectSetup.exe -u ".\sample\BIM360_ProjectUser_Template.csv" -c "<YOUR-CLIENT-ID>" -s "<YOUR-CLIENT-SECRET>" -a "<ACCOUNT-ID>" -b "https://developer.api.autodesk.com" -t ";" -z "," -e "UTF-8" -d "yyyy-MM-dd" -r false -h "project.admin@company.com"
```
Watch the following video:

[![https://youtu.be/cXnvoOS_-Wk](http://img.youtube.com/vi/cXnvoOS_-Wk/0.jpg)](https://youtu.be/cXnvoOS_-Wk "BIM 360 project setup tool - import users")

Note: These template files used in the scripts are just samples which are defined in the spreadsheet structure for you to reference, all required fields must be filled in order for the tool to work properly, please refer the [User Guide](BIM360-ProjectSetupTool-UsageGuide.pdf) for the details.


## Command Line Options
- Usage: Autodesk.BimProjectSetup.exe [-p] [-x] [-u] [-c] [-s] [-a] [-b] [-t] [-z] [-e] [-d] [-r] [-h] [--CF][--AR][–EU]
    - -p Path to CSV input file for project creation
    - -x Path to CSV input file for service activation
    - -u Path to CSV input file with project user information
    - -c Forge client ID
    - -s Forge client secret
    - -a BIM 360 Account ID
    - -b BaseUrl (default= "https://developer.api.autodesk.com")
    - -t Separator character (default = ';')
    - -z Second Separator character (default = ',')
    - -e Encoding (default = UTF-8)
    - -d Date time format pattern(default = yyyy-MM-dd)
    - -r Trial run [true/false] (default = false)
    - -h BIM 360 Account admin Email address
    - -f Directory path where local folders with data are stored
    - --CF Copy folders
    - --AR Admin Industry Role
    - --EU Use the EU region account

# Packages 3rd party libraries used
- The app use [NuGet](https://api.nuget.org/v3/index.json) to manage all the packages
- The [Autodesk.Forge](https://www.nuget.org/packages/Autodesk.Forge/) packages is included by default
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [NLog](https://nlog-project.org/)
- [RestSharp](http://restsharp.org/)


# Further Reading
**Documentation:**
- [BIM 360 API](https://developer.autodesk.com/en/docs/bim360/v1/overview/) and [App Provisioning](https://forge.autodesk.com/blog/bim-360-docs-provisioning-forge-apps)


# Tips & Tricks
- At least one path to an input file must be provided with the -p, -x or -u options;
- Special care must be taken when editing the file in Microsoft Excel:
    1. Make sure that Excel does not alter the date formatting to the System format on the machine which the file is edited. By default the dd/MM/YYYY is used in the CSV to format the date. When the Windows system uses a different time/date format, this sometimes causes Excel to save the date to the default Windows system format. This format can be changed in Configuration Panel | Regional Settings
    2. Make sure the .CSV file is formatted in UTF-8 code.
    3. Open the file in a text editor such as Notepad++ after it was saved to ensure the above two points.
- Logging can be configured in the Nlog.config file. Further information can be found here: https://github.com/nlog/NLog/wiki/Configuration-file.
- If running the tool from the batch file under sample folder, please make sure to set FORGE_CLIENT_ID, FORGE_CLIENT_SECRET, FORGE_BIM_ACCOUNT_ID correctly(**DO NOT** include quotation marks).
- Currently, the following project services could be activated:

    | Service Type | Description | 
    | :-----:| :----: | 
    | admin | BIM 360 Project Administration Service | 
    | doc_manager | BIM 360 Document Management Service | 
    | pm | BIM 360 Project Management Service | 
    | fng | BIM 360 Field Management Service |
    | collab | BIM 360 Design Collaboration Service | 
    | cost | BIM 360 Cost Management Service | 
    | gng | BIM 360 Model Coordination Service | 
    | plan | BIM 360 Plan Classic Service | 
    | field | BIM 360 Field Classic Service | 
    | glue | BIM 360 Glue Classic Service | 
- Project admin has to accept the invitation from email to make the services accessable.

# Limitation
- CSV files need to be prepared with the correct format and required parameters, check [User Guide](BIM360-ProjectSetupTool-UsageGuide.pdf) for details.
- **Copy Folder** only support **Plan** and **Project File** folder and their subfolders.
- Copy folder support copy role permissions with this tool.


# Change History
All the changes will be tracked, please see the [Change History](CHANGELOG.md) file for full details.

# License
This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE.md) file for full details.


# Written by
- Oliver Scharpf, Global Consulting Delivery team, Autodesk.
- Reviewed and maintained by Zhong Wu [@johnonsoftware](https://twitter.com/johnonsoftware), [Forge Partner Development](http://forge.autodesk.com)
