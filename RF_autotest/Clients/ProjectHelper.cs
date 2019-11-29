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

namespace RF_autotest.Clients
{
    class ProjectHelper: BaseClient
        
    {
        private readonly string  _loginToAppResource = "/apigateway/v1/sessions"; //post
        private readonly string _getProjectInfoResource = @"/rfprojects/v1/projects/{0}";
        private readonly string _getReportSbProjectResource = "/rfreports/v1/{0}/reports"; //get
        private readonly string _generateReportSbProjectResource = "/rfreports/v1/{0}/reports"; //post
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

        protected void _assignSbProject(CreatedProject project, string resourse,[Optional] string body)
        {
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(resourse, project.id), body, _headers);
            _waitForAssignProject(project, true);
            Debug.WriteLine("Assign project: " + response.Content + '\n');
        }

        protected void _unassignProject(CreatedProject project, string resourse, [Optional] string body)
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
                    project = _getProjectInfo(project.id);
                    if (Convert.ToInt32(stopTimer.ElapsedMilliseconds) > timer) break;
                }
            } else
            {
                while (project.assignee != null)
                {
                    project = _getProjectInfo(project.id);
                    if (Convert.ToInt32(stopTimer.ElapsedMilliseconds) > timer) break;
                }
            }
            stopTimer.Stop();
        }

        protected CreatedProject _getProjectInfo(string projectId)
        {
            var project = JsonConvert.DeserializeObject<CreatedProject>(_requests.GetRequest(String.Format(_getProjectInfoResource, projectId), _headers).Content);
            return project;
        }

        protected void _generateReportSBproject(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/GenerateReportSBProject.json");
            if (_headers.ContainsKey("X-Project-Type"))
                _headers.Remove("X-Project-Type");
            var response= _requests.PostRequest(String.Format(_generateReportSbProjectResource, project.id), json, _headers);
            Debug.WriteLine("Generate report: " + response.IsSuccessful + '\n');
            _waitGenerationReportSBproject(project);
        }

        private IRestResponse _waitGenerationReportSBproject(CreatedProject project)
        {
            int timer = 360000;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            Debug.WriteLine("Report generating...: "+'\n');
            while (_report.Count==0)
            {
                _report = JsonConvert.DeserializeObject<List<ReportsInfo>>(_getReportsInfo(project.id).Content);
                if (Convert.ToUInt32(stopTimer.ElapsedMilliseconds) > timer) break;
            }
            Debug.WriteLine("Report generated: " + _report.FirstOrDefault().report_type + '\n');
            var response = _getReportsInfo(project.id);
            return response;
        }

        protected IRestResponse _getReportsInfo(string projectId) 
        {
            var response = _requests.GetRequest(String.Format(_getReportSbProjectResource, projectId), _headers);
            return response;
        }

        protected void _waitChangingWorkflowSubStep(CreatedProject project, string current, string expected)
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
                    project = _getProjectInfo(project.id);
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
                    project = _getProjectInfo(project.id);
                    if (project.workflow_substep==null) { break; }
                    else if (Convert.ToUInt32(stopTimer.ElapsedMilliseconds) > timer)
                    {
                        Debug.WriteLine("ERROR: Substep wasn't changed. " + '\n');
                        break;
                    }
                }
            }
        }
    }
}
