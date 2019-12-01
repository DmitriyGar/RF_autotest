using RestSharp;
using RF_autotest.Models;
using RF_autotest.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace RF_autotest.Clients
{
    class ProjectHelperAdmin : ProjectHelper
    {
        private readonly string _unassignSbProjectFromManagerResource = @"/rfworkflow/v1/workflows/{0}/manager_review/unassign"; //put
        private readonly string _reassignSbProjectToManagerResource = @"/rfworkflow/v1/workflows/{0}/manager_review/reassign";
        private readonly string _deleteProjectResource = "/rfprojects/v1/projects/{0}";  //delete
        private DataBaseClient _dbClient;
        private string _session_id;

        public ProjectHelperAdmin(){
            _headers = new Dictionary<string, string>(){

                { "Accept", "application/json" },
                { "X-client", _client}
            };
            _dbClient = new DataBaseClient();
        }

        public void LoginToApp()
        {
            _session_id = Login(Configuration.LoginRfAdmin);
            _headers.Add("Authorization", "SessionID " + _session_id);
        }

        public void DeleteProject(CreatedProject project)
        {
            IRestResponse response = new RestResponse();
            if (project.workflow_step != null && project.workflow_step != "payment" && project.workflow_step != "calculating"&& project.workflow_step !="final_review")
            {
                response = _requests.DeleteRequest(String.Format(_deleteProjectResource, project.id), _headers);
                Debug.WriteLine("Deleted project:  " + response.Content);
            }
            else
                _dbClient.DeleteProjectInDB(project.id);
        }

        public void ReassignSbProjectToManager(CreatedProject project)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/ReassignToManager.json");
            assignProject(project, _reassignSbProjectToManagerResource,json);
        }

        public void UnassignProjectFromManager(CreatedProject project)
        {
            unassignProject(project, _unassignSbProjectFromManagerResource);
        }
    }
}
