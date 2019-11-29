using RestSharp;
using RF_autotest.Models;
using RF_autotest.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace RF_autotest.Clients
{
   class ProjectHelperManager: ProjectHelper
        
    { 
        private readonly string _assignSbProjectResource = @"/rfworkflow/v1/workflows/{0}/manager_review/assign";//put
        private readonly string _unassignSbProjectResource = @"/rfworkflow/v1/workflows/{0}/manager_review/unassign";//put
        private readonly string _assignPaymentProjectResource = "/rfworkflow/v1/workflows/{0}/add_payments/assign";//put
        private readonly string _approveSBProjectByManagerResource = @"/rfworkflow/v1/workflows/{0}/manager_review/complete";//put
        private readonly string _approvePaymentProjectByManagerResource = "/rfworkflow/v1/workflows/{0}/add_payments/complete";//put
        private string _session_id;

        public ProjectHelperManager() {
            _headers = new Dictionary<string, string>(){

                { "Accept", "application/json" },
                { "X-client", _client}
            };
        }

        public void LoginToApp()
        {
            _session_id = Login(Configuration.LoginRfManager);
            _headers.Add("Authorization", "SessionID " + _session_id);
        }

        public CreatedProject GetProjectInfo(string projectId)
        {
            return _getProjectInfo(projectId);
        }

        public void AssignSbProject(CreatedProject project)
        {
            _assignSbProject(project, _assignSbProjectResource);
        }

        public void UnassignProject(CreatedProject project)
        {
            _unassignProject(project, _unassignSbProjectResource);
        }


        public void GenerateReportSBproject(CreatedProject project)
        {
            _generateReportSBproject(project);
        }

        public IRestResponse GetReportsInfo(string projectId)
        {
            return _getReportsInfo(projectId);
        }

        public IRestResponse ApproveProjectByManager(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/ApproveByClient.json");
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_approveSBProjectByManagerResource, project.id), json, _headers);
            Debug.WriteLine("Approve SB project by Manager: " + response.IsSuccessful + '\n');
            _waitChangingWorkflowSubStep(GetProjectInfo(project.id), "manager_review", "final_review");
            return response;
        }

        void RejectProjectByManager()
        {
        }

    }
}
