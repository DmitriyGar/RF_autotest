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
        ProjectHelperAnalyst ProjectHelperAnalyst;
        ProjectHelperAdmin ProjectHelperAdmin;
        private CreatedProject _createdSbProject;
        private List<ReportsInfo> _generatedReports;

        public SmokeTest()
        {
            
           
            ProjectHelperAnalyst = new ProjectHelperAnalyst();
            ProjectHelperAdmin = new ProjectHelperAdmin();
            _createdSbProject = new CreatedProject();
            _generatedReports = new List<ReportsInfo>();

        }
        [OneTimeSetUp]
        public void PreConditions()
        {
           
            ProjectHelperAnalyst.LoginToApp();
            ProjectHelperAdmin.LoginToApp();
        }
      
        [Test]
        public void SmokeTestWorkflow()
        {
            #region Create SB-project
            Debug.WriteLine("CREATING OF SB-PROJECT");
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(ProjectHelperAnalyst.CreateSBProject().Content);
            Debug.WriteLine("Created project: id: " + _createdSbProject.id + " \n Arrangement name: " + _createdSbProject.arrangement_name +'\n');
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("pending_calculation"));
            Assert.IsNotNull(_createdSbProject.id);
            #endregion

            #region Assign Sb-Project to Analyst
            Debug.WriteLine("ASSIGNING BY ANALYST OF SB-PROJECT");
            ProjectHelperAnalyst.AssignSbProject(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(ProjectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_createdSbProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Unassign Sb-Project by Analyst
            Debug.WriteLine("UNASSIGNING BY ANALYST OF SB-PROJECT");
            ProjectHelperAnalyst.UnassignProject(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(ProjectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.assignee, Is.Null);
            Assert.That(_createdSbProject.owners.Count, Is.Zero);
            #endregion

            #region Assign Sb-Project to Analyst again
            Debug.WriteLine("ASSIGNING BY ANALYST OF SB-PROJECT AGAIN");
            ProjectHelperAnalyst.AssignSbProject(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(ProjectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_createdSbProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Calculate Sb-Project
            Debug.WriteLine("CALCULATING OF SB-PROJECT");
            ProjectHelperAnalyst.CalculateSBproject(_createdSbProject);
            Thread.Sleep(3000);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(ProjectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("calculating"));
            ProjectHelperAnalyst.WaitCalculatingSBproject(_createdSbProject);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(ProjectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("adjustments"));
            Assert.That(_createdSbProject.additional_attributes.total_amount, Is.Not.Null);
            #endregion

            #region Generate Report for SbProject
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            ProjectHelperAnalyst.GenerateReportSBproject(_createdSbProject);
            _generatedReports= JsonConvert.DeserializeObject<List<ReportsInfo>>(ProjectHelperAnalyst.GetReportsInfo(_createdSbProject.id).Content);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("summary_by_evaluation_bu"));
            #endregion

            #region Send SbProject to manager
            Debug.WriteLine("SEND SB PROJECT TO MANAGER");
            ProjectHelperAnalyst.SendProjectToManager(_createdSbProject);
            Thread.Sleep(3000);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(ProjectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion
        }


        [OneTimeTearDown]
        public void PostConditions()
        {
            Debug.WriteLine("DELETING OF SB-PROJECT");
            ProjectHelperAdmin.DeleteProject(_createdSbProject.id);
        }
    }
}
