using Newtonsoft.Json;
using RestSharp;
using RF_autotest.Models;
using RF_autotest.Settings;
using System;
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
        private string _loginToAppResource = "/apigateway/v1/sessions"; //post
        private string _getUnassignedProjectsResource = "/rfprojects/v1/unassigned";
        private string _createSbProjectResource ="/rfprojects/v1/projects"; //post
        private string _getProjectInfo = @"/rfprojects/v1/projects/{0}";
        private string _assignSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/assign";//put
        private string _assignPaymentProjectResource = "/rfworkflow/v1/workflows/{0}/add_payments/assign";//put
        private string _generateReportSbProjectResource = "/rfreports/v1/{0}/reports"; //post
        private string _calculateSbProjectResource = @"/rfworkflow/v1/workflows/{0}/pending_calculation/complete"; //put
        private string _SendToManagerResource = "/rfworkflow/v1/workflows/{0}/add_payments/complete";//put
        private string _approveByManagerResource = "";
        
        private string _client="umbrella";
        private Login _credentials;
        private Dictionary<string, string> _headers;
        private RequestBuilder _requests;
        private string _session_id;

        public ProjectHelperAnalyst([Optional]Dictionary<string, string> headers)
        {
            _headers = new Dictionary<string, string>(){
                
                { "Accept", "application/json" },
                { "X-client", _client}
            };
            _credentials = new Login();
            _requests = new RequestBuilder();
            
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
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/CreateProject.json");
            var response = _requests.PostRequest(_createSbProjectResource, json, _headers);
            return response;

        }
        public IRestResponse AssignSbProject(CreatedProject project)
        {
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_assignSbProjectResource, project.id), "",  _headers);
            WaitForAssignProject(project, 5000);
            Debug.WriteLine("Assign project: " + response.Content + '\n');
            return response;

        }
        private void WaitForAssignProject(CreatedProject project, [Optional]int time_milliseconds )
        {
            int timer = 30000;
            if (time_milliseconds == 0)
                time_milliseconds = timer;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            while (project.assignee==null || Convert.ToInt16(stopTimer.ElapsedMilliseconds) < time_milliseconds)
            {
                project = JsonConvert.DeserializeObject<CreatedProject>(GetProjectInfo(project.id).Content);
            }
            stopTimer.Stop();
        }
        void UnassignProject()
        {

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
        public IRestResponse WaitforCalculatingSBproject(CreatedProject project)
        {
           
            while (project.workflow_substep[0] == "calculating"||project.workflow_substep[0] == "pending_calculation")
            {
                project = JsonConvert.DeserializeObject<CreatedProject>(GetProjectInfo(project.id).Content);
               
            }
            Debug.WriteLine("Calculation finished. Sub-step: " + project.workflow_substep[0] + '\n');
            var response = GetProjectInfo(project.id);
            return response;
        }
     
        void SendProjectToManager()
        {

        }
        void PaymentsPackageOff()
        {

        }
        void RejectProjectByManager()
        {

        }
        void AproveProjectByManager()
        {

        }
        void ReturnProjectByClient()
        {

        }
        void AproveProjectByClient()
        {

        }
        void CloseProjectWithProjectDetails()
        {

        }

    }
}
