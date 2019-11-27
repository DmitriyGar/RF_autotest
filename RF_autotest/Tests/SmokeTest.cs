using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RF_autotest.Clients;
using RF_autotest.Models;
using RF_autotest.Models.SbProject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RF_autotest.Tests
{
    [TestFixture]
    class SmokeTest
    {
        private ProjectHelperAnalyst _projectHelperAnalyst;
        private ProjectHelperAdmin _projectHelperAdmin;
        ProjectHelperManager _projectHelperManager;
        private CreatedProject _createdSbProject;
        private List<ReportsInfo> _generatedReports;

        public SmokeTest()
        {
            _projectHelperAnalyst = new ProjectHelperAnalyst();
            _projectHelperAdmin = new ProjectHelperAdmin();
            _projectHelperManager = new ProjectHelperManager();
            _createdSbProject = new CreatedProject();
            _generatedReports = new List<ReportsInfo>();

        }
        [Test]
        public void PreConditions()
        {
            DataBaseClient db = new DataBaseClient();
          //  _projectHelperAnalyst.LoginToApp();
          //  _projectHelperAdmin.LoginToApp();
          //  _projectHelperManager.LoginToApp();
        }
      
        
        public void SmokeTestWorkflow()
        {
            #region Create SB-project
            Debug.WriteLine("CREATING OF SB-PROJECT");
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperAnalyst.CreateSBProject().Content);
            Debug.WriteLine("Created project: id: " + _createdSbProject.id + " \n Arrangement name: " + _createdSbProject.arrangement_name +'\n');
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("pending_calculation"));
            Assert.IsNotNull(_createdSbProject.id);
            #endregion

            #region Assign Sb-Project to Analyst
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignSbProject(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_createdSbProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Unassign Sb-Project by Analyst
            Debug.WriteLine("UNASSIGNING OF SB-PROJECT BY ANALYST ");
            _projectHelperAnalyst.UnassignProject(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.assignee, Is.Null);
            Assert.That(_createdSbProject.owners.Count, Is.Zero);
            #endregion

            #region Assign Sb-Project to Analyst again
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY ANALYST AGAIN");
            _projectHelperAnalyst.AssignSbProject(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_createdSbProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Calculate Sb-Project
            Debug.WriteLine("CALCULATING OF SB-PROJECT");
            _projectHelperAnalyst.CalculateSBproject(_createdSbProject);
            Thread.Sleep(3000);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("calculating"));
            _projectHelperAnalyst.WaitCalculatingSBproject(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("adjustments"));
            Assert.That(_createdSbProject.additional_attributes.total_amount, Is.Not.Null);
            #endregion

            #region Generate Report for SbProject
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReportSBproject(_createdSbProject);
            _generatedReports= JsonConvert.DeserializeObject<List<ReportsInfo>>(_projectHelperAnalyst.GetReportsInfo(_createdSbProject.id).Content);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("summary_by_evaluation_bu"));
            #endregion

            #region Send SbProject to manager
            Debug.WriteLine("SEND SB PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_createdSbProject);
            Thread.Sleep(3000);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Sb-Project by Manager
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperManager.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_createdSbProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            #endregion

            #region Unassign Sb-Project by Admin
            Debug.WriteLine("UNASSIGNING OF SB-PROJECT BY ADMIN");
            _projectHelperAdmin.UnassignProjectFromManager(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperManager.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.assignee, Is.Null);
            Assert.That(_createdSbProject.owners.Count==1);
            #endregion

            #region Reassign Sb-Project to Manager
            Debug.WriteLine("REASSIGNING OF SB-PROJECT TO MANAGER");
            _projectHelperAdmin.ReassignSbProjectToManager(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperManager.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_createdSbProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            #endregion

            #region Approve SbProject by Manager
            Debug.WriteLine("APPROVE SB PROJECT BY MANAGER");
            _projectHelperManager.ApproveProjectByManager(_createdSbProject);
            Thread.Sleep(3000);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_createdSbProject.assignee, Is.Null);
            #endregion

            #region Approve SbProject by Client
            Debug.WriteLine("APPROVE SB PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_createdSbProject);
            Thread.Sleep(3000);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(_projectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion
        }


        [OneTimeTearDown]
        public void PostConditions()
        {
            Debug.WriteLine("DELETING OF SB-PROJECT");
            _projectHelperAdmin.DeleteProject(_createdSbProject.id);
        }
    }
}
