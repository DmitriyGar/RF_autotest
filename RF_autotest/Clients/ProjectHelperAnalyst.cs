using Newtonsoft.Json;
using RF_autotest.Models;
using RF_autotest.Models.SbProject;
using RF_autotest.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Dynamic;

namespace RF_autotest.Clients
{
    class ProjectHelperAnalyst: ProjectHelper
        
    {
        private readonly string _getUnassignedProjectsResource = "/rfprojects/v1/unassigned";
        private readonly string _createSbProjectResource ="/rfprojects/v1/projects"; //post
        private readonly string _assignSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/assign";//put
        private readonly string _unassignSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/unassign";//put
        //private readonly string _assignPaymentProjectResource = "/rfworkflow/v1/workflows/{0}/add_payments/assign";//put
        private readonly string _calculateSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/complete"; //put
        private readonly string _SendSBProjectToManagerResource = @"/rfworkflow/v1/workflows/{0}/adjustments/complete";//put
        //private readonly string _SendPaymentProjectToManagerResource = "/rfworkflow/v1/workflows/{0}/add_payments/complete";//put
        private readonly string _paymentPackageOnOffResource = @"/clients/v1/services/Rebates%2520and%2520Fees"; //put
        private readonly string _approveSbProjectByClientResource = @"/rfworkflow/v1/workflows/{0}/final_review/complete"; //put
        private readonly string _closeSbProjectResource = @"/rfworkflow/v1/workflows/{0}/manual_payment/complete"; //put
        private readonly string _enterManualPaymentsResource = @"/rfpayments/v1/payments/{0}/{1}"; //put
        private readonly string _getPaymentsResource= @"/rfpayments/v1/payments/{0}"; //get
        private List<Payments> _payments;
        private string _session_id;

        public ProjectHelperAnalyst() {
            _headers = new Dictionary<string, string>(){

                { "Accept", "application/json" },
                { "X-client", _client}
            };
        }

        public void LoginToApp()
        {
            _session_id=Login(Configuration.LoginRfAnalyst);
            _headers.Add("Authorization", "SessionID " + _session_id);
        }

        public void GetUnassignedProjects()
        {
            var response = _requests.GetRequest(_getUnassignedProjectsResource, _headers);
            Debug.WriteLine("List of unassigned projects:  " + response.Content + '\n');

        }

        public CreatedProject CreateSBProject()
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/CreateProjectQA.json");
            var response = _requests.PostRequest(_createSbProjectResource, json, _headers);
            Debug.WriteLine("Creating response:  " + response.StatusCode + '\n');
            var project=JsonConvert.DeserializeObject<CreatedProject>(response.Content);
            _waitChangingWorkflowSubStep(_getProjectInfo(project.id), null, "pending_calculation");
            return project;
        }

        public void AssignSbProject(CreatedProject project)
        {
            _assignSbProject(project, _assignSbProjectResource);
        }

        public void UnassignProject(CreatedProject project)
        {
            _unassignProject(project, _unassignSbProjectResource);
        }

        public CreatedProject GetProjectInfo(string projectId)
        {
            return _getProjectInfo(projectId);
        }

        public void CalculateSBproject(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/calculate.json");
            if (!_headers.ContainsKey("X-Project-Type"))
            _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_calculateSbProjectResource, project.id), json, _headers);
            Debug.WriteLine("Start calculating: " + response.IsSuccessful + '\n');
            _waitChangingWorkflowSubStep(GetProjectInfo(project.id),"pending_calculation", "calculating");
           
        }
        public void WaitCalculatingSBproject(CreatedProject project)
        {
            _waitChangingWorkflowSubStep(GetProjectInfo(project.id), "calculating", "adjustments");
            project=GetProjectInfo(project.id);
            Debug.WriteLine("Calculation finished. Sub-step: " + project.workflow_substep[0]);
            Debug.WriteLine("Total amount: " + project.additional_attributes.total_amount + '\n');          
        }

        public void GenerateReportSBproject(CreatedProject project)
        {
            _generateReportSBproject(project);
        }

        public List<ReportsInfo> GetReportsInfo(string projectId) 
        {
            var generatedReports = JsonConvert.DeserializeObject<List<ReportsInfo>>(_getReportsInfo(projectId).Content);
            return generatedReports;
        }

        public void SendProjectToManager(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/SendToManager.json");
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_SendSBProjectToManagerResource, project.id), json, _headers);
            Debug.WriteLine("Send SB project to Manager: " + response.IsSuccessful + '\n');
            _waitChangingWorkflowSubStep(GetProjectInfo(project.id), "adjustments", "manager_review");
        }

        public void PaymentsPackageOn()
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/PaymentsPackageOn.json");
            var response =_requests.PutRequest(String.Format(_paymentPackageOnOffResource), json, _headers);
            Debug.WriteLine("Payment package on: " + response.IsSuccessful + '\n');    
        }

        public void PaymentsPackageOff()
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/PaymentsPackageOff.json");
            var response = _requests.PutRequest(String.Format(_paymentPackageOnOffResource), json, _headers);
            Debug.WriteLine("Payment package off: " + response.IsSuccessful + '\n');
        }

        void ReturnProjectByClient()
        {
            //TODO
        }

        public void AproveProjectByClient(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/ApproveByClient.json");
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_approveSbProjectByClientResource, project.id), json, _headers);
            Debug.WriteLine("Approve SB project by Client: " + response.IsSuccessful + '\n');
            _waitChangingWorkflowSubStep(GetProjectInfo(project.id), "final_review", "manual_payment");
        }

        private List<Payments> _getPayments(CreatedProject project)
        {
            var response = _requests.GetRequest(String.Format(_getPaymentsResource, project.id), _headers);
            _payments = JsonConvert.DeserializeObject<List<Payments>>(response.Content);
            return _payments;
        }

        public void EnterPaymentDetails(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/ManualPayments.json");
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            foreach (Payments pay in _getPayments(project))
            {
                var response = _requests.PutRequest(String.Format(_enterManualPaymentsResource, project.id, pay.id), json, _headers);
                Debug.WriteLine("Enter payment details: " + response.IsSuccessful + '\n');
            }
            _waitEnterPaymentDetails(project);
        }

        private void _waitEnterPaymentDetails(CreatedProject project)
        {
            int timer = 30000;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            foreach (Payments pay in _getPayments(project)) {
                while (pay.check_number==null&&pay.check_date==null&&pay.paid_amount==null)
                {
                    project = _getProjectInfo(project.id);
                    if (pay.check_number != null && pay.check_date != null && pay.paid_amount != null)
                        { break; }
                    else if (Convert.ToUInt32(stopTimer.ElapsedMilliseconds) > timer)
                    {
                        Debug.WriteLine("ERROR: Substep wasn't changed. " + '\n');
                        break;
                    }
                }
            }
            stopTimer.Stop();
        }

        public void CloseProjectWithPaymentDetails(CreatedProject project)
        {
            dynamic close = new ExpandoObject();
              close.action="close";
            string json = JsonConvert.SerializeObject(close);
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_closeSbProjectResource, project.id),json, _headers);
            _waitChangingWorkflowSubStep(GetProjectInfo(project.id), "manual_payment", null);
            Debug.WriteLine("Close SB project: " + response.IsSuccessful + '\n');
        }

    }
}
