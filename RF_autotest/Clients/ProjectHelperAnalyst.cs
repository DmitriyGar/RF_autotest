using Newtonsoft.Json;
using RF_autotest.Models;
using RF_autotest.Models.SbProject;
using RF_autotest.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Dynamic;
using RF_autotest.Models.PaymentProject;
using RestSharp;

namespace RF_autotest.Clients
{
    class ProjectHelperAnalyst: ProjectHelper
        
    {
        private readonly string _getUnassignedProjectsResource = "/rfprojects/v1/unassigned";
        private readonly string _createProjectResource ="/rfprojects/v1/projects"; //post
        private readonly string _assignSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/assign";//put
        private readonly string _unassignSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/unassign";//put
        private readonly string _calculateSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/complete"; //put
        private readonly string _sendSBProjectToManagerResource = @"/rfworkflow/v1/workflows/{0}/adjustments/complete";//put
        private readonly string _approveSbProjectByClientResource = @"/rfworkflow/v1/workflows/{0}/final_review/complete"; //put
        private readonly string _closeSbProjectResource = @"/rfworkflow/v1/workflows/{0}/manual_payment/complete"; //put
        private readonly string _sendPaymentProjectToManagerResource = @"/rfworkflow/v1/workflows/{0}/add_payments/complete";//put
        private readonly string _assignPaymentProjectResource = "/rfworkflow/v1/workflows/{0}/add_payments/assign";//put
        private readonly string _unassignPaymentProjectResource = "/rfworkflow/v1/workflows/{0}/add_payments/unassign";//put
        private readonly string _paymentPackageOnOffResource = @"/clients/v1/services/Rebates%2520and%2520Fees"; //put
        private readonly string _enterSBManualPaymentsResource = @"/rfpayments/v1/payments/{0}/{1}"; //put
        private readonly string _enterPaymentManualPaymentsResource = @"/rfpaymentspkg/v1/payment_packages/{0}/payments/{1}"; //patch
        private readonly string _getPaymentsPaymentProjectResource = @"/rfpaymentspkg/v1/payment_packages/{0}/payments"; //patch
        private readonly string _getPaymentsResource= @"/rfpayments/v1/payments/{0}"; //get
        private readonly string _getPaymentsPaymentPackageResource = @"/rfpaymentspkg/v1/source_payments/available"; //get
        private readonly string _addPaymentsPaymentPackageResource = @"/rfpaymentspkg/v1/payment_packages/{0}"; //patch
        private List<Payments> _sbPayments;
        private string _session_id;

        public ProjectHelperAnalyst() {
            _headers = new Dictionary<string, string>(){

                { "Accept", "application/json" },
                { "X-client", _client},
                { "x-projecttypes-group", "rf" }
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
            var response = _requests.PostRequest(_createProjectResource, json, _headers);
            Debug.WriteLine("Creating response:  " + response.StatusCode + '\n');
            var project=JsonConvert.DeserializeObject<CreatedProject>(response.Content);
            waitChangingWorkflowSubStep(getProjectInfo(project), null, "pending_calculation");
            return project;
        }

        public CreatedProject CreateSbFullRecalculationProject(CreatedProject parentProject)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/CreateFullProject.json");
            var full = JsonConvert.DeserializeObject<CreateFullProject>(json);
            full.parent_project_uuid = parentProject.id;
            full.project_name="Full Recalculation of "+parentProject.project_name;
            full.project_type = parentProject.project_type + "_recalculation";
            json = JsonConvert.SerializeObject(full);
            Debug.WriteLine("json: " + json);
            var response = _requests.PostRequest(_createProjectResource, json, _headers);
            Debug.WriteLine("Creating :  " + response.IsSuccessful + '\n');
            var project = JsonConvert.DeserializeObject<CreatedProject>(response.Content);
            waitChangingWorkflowSubStep(getProjectInfo(project), null, "pending_calculation");
            return project;
        }

        public CreatedProject CreateSbFullReversalProject(CreatedProject parentProject)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/CreateFullProject.json");
            var full = JsonConvert.DeserializeObject<CreateFullProject>(json);
            full.parent_project_uuid = parentProject.id;
            full.project_name = "Full Reversal of " + parentProject.project_name;
            full.project_type = parentProject.project_type + "_reversal";
            Debug.WriteLine("json: " + json);
            json = JsonConvert.SerializeObject(full);
            var response = _requests.PostRequest(_createProjectResource, json, _headers);
            Debug.WriteLine("Creating :  " + response.IsSuccessful + '\n');
            var project = JsonConvert.DeserializeObject<CreatedProject>(response.Content);
            waitChangingWorkflowSubStep(getProjectInfo(project), null, "pending_calculation");
            return project;
        }

        public CreatedProject CreatePaymentProject()
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/CreatePaymentProject.json");
            var response = _requests.PostRequest(_createProjectResource, json, _headers);
            Debug.WriteLine("Creating response:  " + response.StatusCode + '\n');
            var project = JsonConvert.DeserializeObject<CreatedProject>(response.Content);
            waitChangingWorkflowSubStep(getProjectInfo(project), null, "add_payments");
            return project;
        }

        public void AssignProject(CreatedProject project)
        {
            if (GetProjectInfo(project).project_type != "sb_rebate_payments")
            { assignProject(project, _assignSbProjectResource); }
            else if (GetProjectInfo(project).project_type == "sb_rebate_payments")
            { assignProject(project, _assignPaymentProjectResource); }
        }

        public void UnassignProject(CreatedProject project)
        {
            if (GetProjectInfo(project).project_type != "sb_rebate_payments")
            { unassignProject(project, _unassignSbProjectResource); }
            else if (GetProjectInfo(project).project_type == "sb_rebate_payments")
            { unassignProject(project, _unassignPaymentProjectResource); }
        }

        public CreatedProject GetProjectInfo(CreatedProject project)
        {
            return getProjectInfo(project);
        }

        public void CalculateSBproject(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/calculate.json");
            setProjectTypeHeaders(project);
            var response = _requests.PutRequest(String.Format(_calculateSbProjectResource, project.id), json, _headers);
            Debug.WriteLine("Start calculating: " + response.IsSuccessful + '\n');
            waitChangingWorkflowSubStep(GetProjectInfo(project),"pending_calculation", "calculating");
           
        }
        public void WaitCalculatingSBproject(CreatedProject project)
        {
            waitChangingWorkflowSubStep(GetProjectInfo(project), "calculating", "adjustments");
            project=GetProjectInfo(project);
            Debug.WriteLine("Calculation finished. Sub-step: " + project.workflow_substep[0]);
            Debug.WriteLine("Total amount: " + project.additional_attributes.total_amount + '\n');          
        }

        public void GenerateReport(CreatedProject project)
        {
            generateReport(project);
        }

        public List<ReportsInfo> GetReportsInfo(string projectId) 
        {
            var generatedReports = JsonConvert.DeserializeObject<List<ReportsInfo>>(getReportsInfo(projectId).Content);
            return generatedReports;
        }

        public void SendProjectToManager(CreatedProject project)
        {
            IRestResponse response =new RestResponse();
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/SendToManager.json");
            setProjectTypeHeaders(project);
            if (GetProjectInfo(project).project_type != "sb_rebate_payments")
            {
                response = _requests.PutRequest(String.Format(_sendSBProjectToManagerResource, project.id), json, _headers);
                waitChangingWorkflowSubStep(GetProjectInfo(project), "adjustments", "manager_review");
            }
            else if (GetProjectInfo(project).project_type == "sb_rebate_payments")
            {
                response = _requests.PutRequest(String.Format(_sendPaymentProjectToManagerResource, project.id), json, _headers);
                waitChangingWorkflowSubStep(GetProjectInfo(project), "add_payments", "manager_review");
            }
            Debug.WriteLine("Send project to Manager: " + response.IsSuccessful + '\n');
            
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
            setProjectTypeHeaders(project);
            var response = _requests.PutRequest(String.Format(_approveSbProjectByClientResource, project.id), json, _headers);
            Debug.WriteLine("Approve SB project by Client: " + response.IsSuccessful + '\n');

                waitChangingWorkflowSubStep(GetProjectInfo(project), "final_review", "manual_payment");
            
        }
        
        private List<PaymentPackage> _getPaymentsPaymentPackage(CreatedProject project)
        {
            
            var response = _requests.GetRequest(String.Format(_getPaymentsPaymentPackageResource, project.id), _headers);
            var paymentsPackage = JsonConvert.DeserializeObject <List<PaymentPackage>> (response.Content);
            return paymentsPackage;
        }

        public void AddPaymentsPaymentPackage(CreatedProject payment_project, List<Payments> sbProjectpayments )
        {
            
            var paymentsPackage = _getPaymentsPaymentPackage(payment_project);
            IRestResponse response;
            var addPayment=new AddPaymentsPaymentPackage();
            addPayment.source_payments_uuid_list = new List<string>();
            addPayment.erp_enabled = "false";
            var paymentsInPayPkg = new List<Payment>();
            string json;
            
            for (int i=0; i < paymentsPackage.Count; i++)
            {
                foreach (Payments pay in sbProjectpayments)
                {
                     foreach (var payments in paymentsPackage[i].payments) {
                         if (payments.id == pay.id)
                         {
                             addPayment.source_payments_uuid_list.Add(payments.id); 
                         }
                     }
                     json = JsonConvert.SerializeObject(addPayment);
                     setProjectTypeHeaders(payment_project);
                     response = _requests.PatchRequest(String.Format(_addPaymentsPaymentPackageResource, payment_project.id), json, _headers);
                     Debug.WriteLine("Add payments: " + response.IsSuccessful + '\n');
                }
            }
        }           

        public List<Payments> GetSbPayments(CreatedProject project)
        {
            var response = _requests.GetRequest(String.Format(_getPaymentsResource, project.id), _headers);
            _sbPayments = JsonConvert.DeserializeObject<List<Payments>>(response.Content);
            return _sbPayments;
        }

        private List<PaymentsPayment> _getPaymentPayments(CreatedProject project)
        {
            setProjectTypeHeaders(project);
            var response = _requests.GetRequest(String.Format(_getPaymentsPaymentProjectResource, project.id), _headers);
            var payments = JsonConvert.DeserializeObject<List<PaymentsPayment>>(response.Content);
            return payments;
        }

        public void EnterPaymentDetails(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/ManualPayments.json");
            setProjectTypeHeaders(project);
            if (GetProjectInfo(project).project_type != "sb_rebate_payments")
            {
                foreach (Payments pay in GetSbPayments(project))
                {
                    var response = _requests.PutRequest(String.Format(_enterSBManualPaymentsResource, project.id, pay.id), json, _headers);
                    Debug.WriteLine("Enter payment details SB: " + response.IsSuccessful + '\n');
                }
            }
            else if (GetProjectInfo(project).project_type == "sb_rebate_payments")
            {
                foreach (PaymentsPayment pay in _getPaymentPayments(project))
                {
                     var response = _requests.PatchRequest(String.Format(_enterPaymentManualPaymentsResource, project.id, pay.id), json, _headers);
                     Debug.WriteLine("Enter payment details Payment: " + response.IsSuccessful + '\n');
                  
                }
            }
                _waitEnterPaymentDetails(project);
        }

        private void _waitEnterPaymentDetails(CreatedProject project)
        {
            int timer = 30000;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            foreach (PaymentsPayment pay in _getPaymentPayments(project))
            {
                while (pay.check_number==null&&pay.check_date==null&&pay.paid_amount==null)
                {
                    project = getProjectInfo(project);
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
            setProjectTypeHeaders(project);
            var response = _requests.PutRequest(String.Format(_closeSbProjectResource, project.id),json, _headers);
            waitChangingWorkflowSubStep(GetProjectInfo(project), "manual_payment", null);
            Debug.WriteLine("Close SB project: " + response.IsSuccessful + '\n');
        }

    }
}
