using Newtonsoft.Json;
using RestSharp;
using RF_autotest.Models;
using RF_autotest.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace RF_autotest.Clients
{
    class ProjectHelperAdmin 
    {
        private string _loginToAppResource = "/apigateway/v1/sessions"; //post
        private string _getUnassignedProjectsResource = "/rfprojects/v1/unassigned";
        private readonly string _getProjectInfo = @"/rfprojects/v1/projects/{0}";
        private readonly string _unassignSbProjectFromManagerResource = @"/rfworkflow/v1/workflows/{0}/manager_review/unassign";//put
        private readonly string _reassignSbProjectToManagerResource = @"/rfworkflow/v1/workflows/{0}/manager_review/reassign";
        private string _deleteProjectResource = "/rfprojects/v1/projects/{0}";  //delete

        private string _client = "umbrella";
        private Dictionary<string, string> _headers;
        private Login _credentials;

        private RequestBuilder _requests;
        private string _session_id;
        public ProjectHelperAdmin([Optional]Dictionary<string, string> headers)
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
            _credentials.username = Configuration.LoginRfAdmin;
            _credentials.password = Configuration.Password;
            string json = JsonConvert.SerializeObject(_credentials);
            var response = _requests.PostRequest(_loginToAppResource, json, _headers).Content;
            _session_id = JsonConvert.DeserializeObject<SessionIdentification>(response).SessionId;
            _headers.Add("Authorization", "SessionID " + _session_id);
            Debug.WriteLine("Logged to the app as:  " + json );
            Debug.WriteLine("sessionID:  " + _session_id + '\n');

        }
        public bool DeleteProject(string projectId)
        {
            bool isDeleted=false;
            var response = _requests.DeleteRequest(String.Format(_deleteProjectResource, projectId), _headers);
            if (response.IsSuccessful)
                isDeleted = true;
            else
                isDeleted = false;
            Debug.WriteLine("Deleted project:  " + response.Content);
            return isDeleted;

        }

        public IRestResponse GetProjectInfo(string projectId)
        {
            var response = _requests.GetRequest(String.Format(_getProjectInfo, projectId), _headers);
            return response;
        }


        public IRestResponse ReassignSbProjectToManager(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/ReassignToManager.json");
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_reassignSbProjectToManagerResource, project.id), json, _headers);
            WaitForAssignProject(project,  true);
            Debug.WriteLine("Reassign project: " + response.Content + '\n');
            return response;
            //TODO unhardcode
        }

        private void WaitForAssignProject(CreatedProject project, bool assign)
        {
            int time_milliseconds = 30000;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Start();
            if (assign == true)
            {
                while (project.assignee == null || Convert.ToInt32(stopTimer.ElapsedMilliseconds) < time_milliseconds)
                {
                    project = JsonConvert.DeserializeObject<CreatedProject>(GetProjectInfo(project.id).Content);
                }
            }
            else
            {
                while (project.assignee != null || Convert.ToInt32(stopTimer.ElapsedMilliseconds) < time_milliseconds)
                {
                    project = JsonConvert.DeserializeObject<CreatedProject>(GetProjectInfo(project.id).Content);
                }
            }

            stopTimer.Stop();
        }

        public IRestResponse UnassignProjectFromManager(CreatedProject project)
        {
            if (!_headers.ContainsKey("X-Project-Type"))
                _headers.Add("X-Project-Type", project.project_type);
            var response = _requests.PutRequest(String.Format(_unassignSbProjectFromManagerResource, project.id), "", _headers);
            WaitForAssignProject(project,  false);
            Debug.WriteLine("Unassign project: " + response.Content + '\n');
            return response;
            //TODO unhardcode
        }

    }
}
