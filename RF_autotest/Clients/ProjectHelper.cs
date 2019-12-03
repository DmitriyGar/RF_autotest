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
using System.Dynamic;
using System.Threading;

namespace RF_autotest.Clients
{
    class ProjectHelper: BaseClient
        
    {
        private readonly string  _loginToAppResource = "/apigateway/v1/sessions"; //post
        private readonly string _getProjectInfoResource = @"/rfprojects/v1/projects/{0}";
        private readonly string _getReportsProjectResource = "/rfreports/v1/{0}/reports"; //get
        private readonly string _generateReportResource = "/rfreports/v1/{0}/reports"; //post
        private readonly string _getRFServisesResource = "/clients/v1/services"; //get
        private Login _credentials;
        protected Dictionary<string, string> _headers;
        protected RequestBuilder _requests;
        protected string _client;
        protected List<ReportsInfo>  _report;

        protected ProjectHelper()
        {
            _credentials = new Login();
            _requests = new RequestBuilder();
            _report = new  List<ReportsInfo>();
            _client = Configuration.Client;
        }

        protected string Login(string login)
        {
            _credentials.username = login;
            _credentials.password = Configuration.Password;
            string json = JsonConvert.SerializeObject(_credentials);          
            var response =_requests.PostRequest(_loginToAppResource,json, _headers).Content;      
            var _session_id = JsonConvert.DeserializeObject<SessionIdentification>(response).SessionId;
            Debug.WriteLine("Logged to the app as:  "+ json);
            Debug.WriteLine("sessionID:  "+ _session_id + '\n');
            return _session_id;
        }

        protected void assignProject(CreatedProject project, string resourse,[Optional] string body)
        {
            setProjectTypeHeaders(project);
            var response = _requests.PutRequest(String.Format(resourse, project.id), body, _headers);
            _waitForAssignProject(project, true);
            Debug.WriteLine("Assign project: " + response.Content + '\n');
        }

        protected void unassignProject(CreatedProject project, string resourse, [Optional] string body)
        {
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(resourse, project.id), body, _headers);
            _waitForAssignProject(project, false);
            Debug.WriteLine("Unassign project: " + response.Content + '\n');
        }

        private void _waitForAssignProject(CreatedProject project,  bool assign)
        {
            int timer = 30000;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            if (assign == true) { 
                while (project.assignee == null )
                {
                    project = getProjectInfo(project.id);
                    if (Convert.ToInt32(stopTimer.ElapsedMilliseconds) > timer) break;
                }
            } else
            {
                while (project.assignee != null)
                {
                    project = getProjectInfo(project.id);
                    if (Convert.ToInt32(stopTimer.ElapsedMilliseconds) > timer) break;
                }
            }
            stopTimer.Stop();
        }

        protected CreatedProject getProjectInfo(string projectId)
        {
            var project = JsonConvert.DeserializeObject<CreatedProject>(_requests.GetRequest(String.Format(_getProjectInfoResource, projectId), _headers).Content);
            return project;
        }

        private RFServices _getRFServices()
        {
            var services = JsonConvert.DeserializeObject<RFServices>(_requests.GetRequest(_getRFServisesResource, _headers).Content);
            return services;
        }

        protected bool IsPaymentPackageOn()
        {
            bool isPaymentPkgOn = false;
            foreach (var service in _getRFServices().data)
                if (service.service_code == "randf" && service.is_payment_package_enabled == true)
                    isPaymentPkgOn = true;
            return isPaymentPkgOn;
        }


        protected void generateReport(CreatedProject project)
        {
            string json="";
            if (project.project_type == "sb_rebate")
            {
                json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/GenerateReportSBProject.json");
            }
            else if (project.project_type == "sb_rebate_payments")
            {
                json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/GenerateReportPaymentProject.json");
            }
            
            var response= _requests.PostRequest(String.Format(_generateReportResource, project.id), json, _headers);
            Thread.Sleep(2000);
            response = _requests.PostRequest(String.Format(_generateReportResource, project.id), json, _headers);
            Debug.WriteLine("Generate report: " + response.IsSuccessful + '\n');
            _waitGenerationReportSBproject(project);
        }

        private IRestResponse _waitGenerationReportSBproject(CreatedProject project)
        {
            _report = JsonConvert.DeserializeObject<List<ReportsInfo>>(getReportsInfo(project.id).Content);
            int timer = 360000;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            Debug.WriteLine("Report generating...: "+'\n');
            while (_report.Count!=1)
            {
                _report = JsonConvert.DeserializeObject<List<ReportsInfo>>(getReportsInfo(project.id).Content);
                if (Convert.ToUInt32(stopTimer.ElapsedMilliseconds) > timer) break;
            }
            Debug.WriteLine("Report generated: " + _report.FirstOrDefault().report_type + '\n');
            var response = getReportsInfo(project.id);
            return response;
        }

        protected IRestResponse getReportsInfo(string projectId) 
        {
            if (_headers.ContainsKey("X-Project-Type"))
                _headers.Remove("X-Project-Type");
            var response = _requests.GetRequest(String.Format(_getReportsProjectResource, projectId), _headers);
            return response;
        }

        protected void waitChangingWorkflowSubStep(CreatedProject project, string current, string expected)
        {
            
            int timer = 30000;
            if (current == "calculating")
                timer = 1800000;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            if (expected != null)
            {
                
                while (project.workflow_substep[0] != expected)
                {
                    project = getProjectInfo(project.id);
                    if (project.workflow_substep[0] == expected) { break; }
                    else if (Convert.ToUInt32(stopTimer.ElapsedMilliseconds) > timer)
                    {
                        Debug.WriteLine("ERROR: Substep wasn't changed. " + '\n');
                        break;
                    }
                }
            }
            else
            {
                while (project.workflow_substep!=null)
                {
                    project = getProjectInfo(project.id);
                    if (project.workflow_substep==null) { break; }
                    else if (Convert.ToUInt32(stopTimer.ElapsedMilliseconds) > timer)
                    {
                        Debug.WriteLine("ERROR: Substep wasn't changed. " + '\n');
                        break;
                    }
                }
            }
        }

        protected void setProjectTypeHeaders(CreatedProject project)
        {
            if (!_headers.ContainsValue(project.project_type))
            {
                if (_headers.ContainsKey("X-Project-Type"))
                    _headers.Remove("X-Project-Type");
                _headers.Add("X-Project-Type", project.project_type);
            }
        }
    }
}
