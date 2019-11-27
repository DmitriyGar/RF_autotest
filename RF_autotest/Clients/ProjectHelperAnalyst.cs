using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RF_autotest.Models;
using RF_autotest.Models.SbProject;
using RF_autotest.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Clients
{
    class ProjectHelperAnalyst: BaseClient
        
    {
        private readonly string  _loginToAppResource = "/apigateway/v1/sessions"; //post
        private readonly string _getUnassignedProjectsResource = "/rfprojects/v1/unassigned";
        private readonly string _createSbProjectResource ="/rfprojects/v1/projects"; //post
        private readonly string _getProjectInfo = @"/rfprojects/v1/projects/{0}";
        private readonly string _assignSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/assign";//put
        private readonly string _unassignSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/unassign";//put
        //private readonly string _assignPaymentProjectResource = "/rfworkflow/v1/workflows/{0}/add_payments/assign";//put
        private readonly string _getReportSbProjectResource = "/rfreports/v1/{0}/reports"; //get
        private readonly string _generateReportSbProjectResource = "/rfreports/v1/{0}/reports"; //post
        private readonly string _calculateSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/complete"; //put
        private readonly string _SendSBProjectToManagerResource = @"/rfworkflow/v1/workflows/{0}/adjustments/complete";//put
        //private readonly string _SendPaymentProjectToManagerResource = "/rfworkflow/v1/workflows/{0}/add_payments/complete";//put
        private readonly string _paymentPackageOnResource = @"/clients/v1/services/Rebates%2520and%2520Fees"; //put
        private readonly string _approveSbProjectByClientResource = @"/rfworkflow/v1/workflows/{0}/final_review/complete"; //put
        private readonly string _closeSbProjectPaymentDetailsResource = @"/rfworkflow/v1/workflows/{0}/manual_payment/complete"; //put

        private string _client="umbrella";
        private Login _credentials;
        private Dictionary<string, string> _headers;
        private RequestBuilder _requests;
        private List<ReportsInfo>  _report;
        private string _session_id;

        public ProjectHelperAnalyst([Optional]Dictionary<string, string> headers)
        {
            _headers = new Dictionary<string, string>(){
                
                { "Accept", "application/json" },
                { "X-client", _client}
            };
            _credentials = new Login();
            _requests = new RequestBuilder();
            _report = new  List<ReportsInfo>() ;
                
            
        }

        public void LoginToApp()
        {
            _credentials.username = Configuration.LoginRfAnalyst;
            _credentials.password = Configuration.Password;
            string json = JsonConvert.SerializeObject(_credentials);          
            var response =_requests.PostRequest(_loginToAppResource,json, _headers).Content;      
            _session_id = JsonConvert.DeserializeObject<SessionIdentification>(response).SessionId;
            _headers.Add("Authorization", "SessionID " + _session_id);
            Debug.WriteLine("Logged to the app as:  "+ json);
            Debug.WriteLine("sessionID:  "+ _session_id + '\n');

        }

        public void GetUnassignedProjects()
        {
            var response = _requests.GetRequest(_getUnassignedProjectsResource, _headers);
            Debug.WriteLine("List of unassigned projects:  " + response.Content + '\n');

        }

        public IRestResponse CreateSBProject()
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/CreateProjectQA.json");
            var response = _requests.PostRequest(_createSbProjectResource, json, _headers);
            Debug.WriteLine("Creating response:  " + response.StatusCode + '\n');
            return response;

        }

        public IRestResponse AssignSbProject(CreatedProject project)
        {
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_assignSbProjectResource, project.id), "",  _headers);
            WaitForAssignProject(project, 5000, true);
            Debug.WriteLine("Assign project: " + response.Content + '\n');
            return response;

        }

        private void WaitForAssignProject(CreatedProject project, [Optional]int time_milliseconds, bool assign)
        {
            int timer = 30000;
            if (time_milliseconds == 0)
                time_milliseconds = timer;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            if (assign == true) { 
                while (project.assignee == null || Convert.ToInt32(stopTimer.ElapsedMilliseconds) < time_milliseconds)
                {
                    project = JsonConvert.DeserializeObject<CreatedProject>(GetProjectInfo(project.id).Content);
                }
            } else
            {
                while (project.assignee != null || Convert.ToInt32(stopTimer.ElapsedMilliseconds) < time_milliseconds)
                {
                    project = JsonConvert.DeserializeObject<CreatedProject>(GetProjectInfo(project.id).Content);
                }
            }

            stopTimer.Stop();
        }

        public IRestResponse UnassignProject(CreatedProject project)
        {
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_unassignSbProjectResource, project.id), "", _headers);
            WaitForAssignProject(project, 5000, false);
            Debug.WriteLine("Unassign project: " + response.Content + '\n');
            return response;
        }

        public IRestResponse GetProjectInfo(string projectId)
        {
            var response = _requests.GetRequest(String.Format(_getProjectInfo, projectId), _headers);
            return response;
        }

        public IRestResponse CalculateSBproject(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/calculate.json");
            if (!_headers.ContainsKey("X-Project-Type"))
            _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_calculateSbProjectResource, project.id), json, _headers);
            Debug.WriteLine("Start calculating: " + response.IsSuccessful + '\n');
            return response;
        }
        public IRestResponse WaitCalculatingSBproject(CreatedProject project)
        {
           
            while (project.workflow_substep[0] == "calculating"||project.workflow_substep[0] == "pending_calculation")
            {
                project = JsonConvert.DeserializeObject<CreatedProject>(GetProjectInfo(project.id).Content);
               
            }
            Debug.WriteLine("Calculation finished. Sub-step: " + project.workflow_substep[0] + '\n');
            Debug.WriteLine("Total amount: " + project.additional_attributes.total_amount + '\n');
            var response = GetProjectInfo(project.id);
            return response;
        }

        public IRestResponse GenerateReportSBproject(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/GenerateReportSBProject.json");
            if (_headers.ContainsKey("X-Project-Type"))
                _headers.Remove("X-Project-Type");
            var response= _requests.PostRequest(String.Format(_generateReportSbProjectResource, project.id), json, _headers);
            Debug.WriteLine("Generate report: " + response.IsSuccessful + '\n');
            _waitGenerationReportSBproject(project);
            response = GetProjectInfo(project.id);
            return response;
        }

        private IRestResponse _waitGenerationReportSBproject(CreatedProject project, [Optional]int time_milliseconds)
        {
            int timer = 120000;
            if (time_milliseconds == 0)
                time_milliseconds = timer;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            Debug.WriteLine("Report generating...: "+'\n');
            while (_report.Count==0 || Convert.ToUInt32(stopTimer.ElapsedMilliseconds) < time_milliseconds)
            {
                _report = JsonConvert.DeserializeObject<List<ReportsInfo>>(GetReportsInfo(project.id).Content);
                if (_report.Count != 0)
                {
                    break;
                }
            }
            Debug.WriteLine("Report generated: " + _report.FirstOrDefault().report_type + '\n');
            var response = GetReportsInfo(project.id);
            return response;
        }

        public IRestResponse GetReportsInfo(string projectId)
           
        {
            var response = _requests.GetRequest(String.Format(_getReportSbProjectResource, projectId), _headers);
            return response;
        }

        public IRestResponse SendProjectToManager(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/SendToManager.json");
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_SendSBProjectToManagerResource, project.id), json, _headers);
            Debug.WriteLine("Send SB project to Manager: " + response.IsSuccessful + '\n');
            return response;

        }
        public void PaymentsPackageOn()
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/PaymentsPackageOn.json");
            var response =_requests.PutRequest(String.Format(_paymentPackageOnResource), json, _headers).Content;
            Debug.WriteLine("Payment package on: " + response + '\n');
           
        }
        

        void ReturnProjectByClient()
        {

        }
        public IRestResponse AproveProjectByClient(CreatedProject project)
         
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/ApproveByClient.json");
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_approveSbProjectByClientResource, project.id), json, _headers);
            Debug.WriteLine("Approve SB project by Client: " + response.IsSuccessful + '\n');
            return response;

        }
        void CloseProjectWithPaymentDetails()
        {

        }

    }
}
