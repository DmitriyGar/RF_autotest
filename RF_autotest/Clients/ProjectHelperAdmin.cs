using Newtonsoft.Json;
using RF_autotest.Models;
using RF_autotest.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RF_autotest.Clients
{
    class ProjectHelperAdmin 
    {
        private string _loginToAppResource = "/apigateway/v1/sessions"; //post
        private string _getUnassignedProjectsResource = "/rfprojects/v1/unassigned";
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
        public void DeleteProject(string projectId)
        {
            var response = _requests.DeleteRequest(String.Format(_deleteProjectResource, projectId), _headers);
            Debug.WriteLine("Deleted project:  " + response.Content);
        }
       

    }
}
