using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RF_autotest.Clients;
using RF_autotest.Models;
using System;
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

        public SmokeTest()
        {
            
           
            ProjectHelperAnalyst = new ProjectHelperAnalyst();
            ProjectHelperAdmin = new ProjectHelperAdmin();
            _createdSbProject = new CreatedProject();

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

            #region Calculate Sb-Project
            Debug.WriteLine("CALCULATING OF SB-PROJECT");
            ProjectHelperAnalyst.CalculateSBproject(_createdSbProject);
            Thread.Sleep(5000);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(ProjectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("calculating"));
            ProjectHelperAnalyst.WaitforCalculatingSBproject(_createdSbProject);
            Thread.Sleep(3000);
            _createdSbProject = JsonConvert.DeserializeObject<CreatedProject>(ProjectHelperAnalyst.GetProjectInfo(_createdSbProject.id).Content);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("adjustments"));
            #endregion

            #region Generate Report for SbProject
            //todo
            #endregion
        }


        [OneTimeTearDown]
        public void PostConditions()
        {
            //Thread.Sleep(5000);
            Debug.WriteLine("DELETING OF SB-PROJECT");
            ProjectHelperAdmin.DeleteProject(_createdSbProject.id);
        }
    }
}
