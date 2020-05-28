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
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using BimProjectSetupCommon.Helpers;
using Autodesk.Forge.BIM360;
using Autodesk.Forge.BIM360.Serialization;

namespace BimProjectSetupCommon.Workflow
{
    public class ServiceWorkflow : BaseWorkflow
    {
        private BimProjectsApi _projectApi = null;

        public ServiceWorkflow(AppOptions options) : base(options)
        {
            _projectApi = new BimProjectsApi(GetToken, options);
            DataController.InitializeAllProjects();
        }

        public void ActivateServicesProcess(List<BimProject> projects, List<ServiceActivation> service)
        {
            try
            {
                ActivateServices(projects, service);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        public void ActivateServicesProcess(List<string> projectNames, List<ServiceActivation> service)
        {
            List<BimProject> projects = new List<BimProject>();
            foreach (string projName in projectNames)
            {
                BimProject proj = DataController.AllProjects.Find(x => x.name == projName);
                projects.Add(proj);
            }
            ActivateServicesProcess(projects, service);
        }
        public void ActivateServicesProcess()
        {
            try
            {
                DataController._serviceTable = CsvReader.ReadDataFromCSV(DataController._serviceTable, DataController._options.ServiceFilePath);
                if (false == _options.TrialRun)
                {
                    ActivateServices();
                }
                else
                {
                    Log.Info("Trial run finished. No further processing");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        internal bool CheckRequiredParams(ServiceActivation act)
        {
            bool isNull = string.IsNullOrEmpty(act.role) || string.IsNullOrEmpty(act.service_type) || string.IsNullOrEmpty(act.company_id)
                || string.IsNullOrEmpty(act.email);

            return !isNull;
        }
        internal void ActivateServices()
        {
            Log.Info("");
            Log.Info("Adding admins and activating services..");

            Dictionary<int, ServiceActivation> _serviceToActivate = DataController.GetServiceActivations();

            if (_serviceToActivate == null || _serviceToActivate.Count < 1)
            {
                Log.Warn("- service activation was unable to start due to insufficient input data");
                return;
            }

            foreach(int rowIndex in _serviceToActivate.Keys)
            {
                string projName = _serviceToActivate[rowIndex].project_name;
                string id = DataController.GetProjectIdByName(projName);
                IRestResponse response = ActivateService(_serviceToActivate[rowIndex], id, rowIndex);
            }
            CsvExporter.WriteResults(DataController._serviceTable, _options, _options.ServiceFilePath);
        }
        private void ActivateServices(List<BimProject> projects, List<ServiceActivation> services)
        {
            try
            {
                Log.Info("");
                Log.Info("Start add services and admins");
                foreach (BimProject project in projects)
                {
                    foreach (ServiceActivation service in services)
                    {
                        BimCompany comp = DataController.Companies.FirstOrDefault(c => c.name != null && c.name.Equals(service.company));
                        if (comp != null) service.company_id = comp.id;

                        Log.Info($"- processing {service.service_type} for {service.email} on { project.name}");
                        string id = DataController.GetProjectIdByName(project.name);
                        if (id == null)
                        {
                            string msg = $"- system was unable to find project name '{project.name}'";
                            Log.Error(msg);
                            Log.Info($"");
                            continue;
                        }
                        ActivateService(service, id);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }
        private IRestResponse ActivateService(ServiceActivation service, string projectId, int rowIndex = -1)
        {
            IRestResponse response = null;
            if (false == CheckRequiredParams(service))
            {
                string msg = $"- one of the required parameters is null or empty. Required are: role, service_type,company_id,email";
                if(rowIndex > -1)
                {
                    DataController._serviceTable.Rows[rowIndex]["result"] = ResultCodes.IncompleteInputData;
                    DataController._serviceTable.Rows[rowIndex]["result_message"] = msg;
                }

                Log.Error(msg);
            }

            try
            {
                response = _projectApi.PostUserAndService(projectId, service);
                ActivateProjectResponseHandler(response, projectId, rowIndex);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return response;
        }

        #region Response Handler
        internal static void ActivateProjectResponseHandler(IRestResponse response, string id, int rowIndex = -1)
        {
            LogResponse(response);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                ServiceActivationResponse res = JsonConvert.DeserializeObject<ServiceActivationResponse>(response.Content);
                if (rowIndex > -1)
                {
                    DataController._serviceTable.Rows[rowIndex]["result"] = ResultCodes.Activated;
                    DataController._serviceTable.Rows[rowIndex]["result_message"] = $"{res.service_type} has been activated";
                }
                DataController.AllProjects.FirstOrDefault(p => p.id == id).status = Status.active;
                Log.Info($"- {res.service_type} has been activated");
            }
            else
            {
                ResponseContent content = JsonConvert.DeserializeObject<ResponseContent>(response.Content);
                string msg = ((content != null && content.message != null) ? content.message : null);
                if (rowIndex > -1)
                {
                    DataController._serviceTable.Rows[rowIndex]["result"] = ResultCodes.Error;
                    DataController._serviceTable.Rows[rowIndex]["result_message"] = msg;
                }
                Log.Warn($"- service not activated. {msg}");
            }
        }
        internal static void LogResponse(IRestResponse response)
        {
            Log.Info($"- status code: {response.StatusCode}");
            if (response.ErrorException != null)
            {
                Log.Error($"- error message: {response.ErrorMessage}");
                Log.Error($"- error exception: {response.ErrorException}");
            }
        }
        #endregion

        #region CSV export
        public void ExportServicesCsv()
        {
            CsvExporter.ExportServicesCsvTemplate();
        }
        #endregion
    }
}
