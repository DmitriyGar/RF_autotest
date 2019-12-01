using NUnit.Framework;
using RF_autotest.Clients;
using RF_autotest.Models;
using RF_autotest.Models.SbProject;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RF_autotest.Tests
{
    [TestFixture]
    class SmokeTest
    {
        private ProjectHelperAnalyst _projectHelperAnalyst;
        private ProjectHelperAdmin _projectHelperAdmin;
        private ProjectHelperManager _projectHelperManager;
        private CreatedProject _createdSbProject;
        private CreatedProject _createdPaymentProject;
        private List<ReportsInfo> _generatedReports;
        private List<Payments> _sbProjectPayments;

        public SmokeTest()
        {
            _projectHelperAnalyst = new ProjectHelperAnalyst();
            _projectHelperAdmin = new ProjectHelperAdmin();
            _projectHelperManager = new ProjectHelperManager();
            _createdSbProject = new CreatedProject();
            _generatedReports = new List<ReportsInfo>();
            _createdPaymentProject = new CreatedProject();
            _sbProjectPayments = new List<Payments>();


        }
        [OneTimeSetUp]
        public void PreConditions()
        {
             _projectHelperAnalyst.LoginToApp();
             _projectHelperAdmin.LoginToApp();
             _projectHelperManager.LoginToApp();
        }

        [Test]
        public void SmokeTestSBWorkflow()
        {
            #region Create SB-project
            Debug.WriteLine("\n CREATING OF SB-PROJECT");
            _createdSbProject = _projectHelperAnalyst.CreateSBProject();
            Debug.WriteLine("Created SB project id: " + _createdSbProject.id + "\n Arrangement name: " + _createdSbProject.arrangement_name.FirstOrDefault() +'\n');
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("pending_calculation"));
            Assert.That(_createdSbProject.assignee, Is.Null);
            #endregion

            #region Assign Sb-Project to Analyst
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignProject(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_createdSbProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Unassign Sb-Project by Analyst
            Debug.WriteLine("UNASSIGNING OF SB-PROJECT BY ANALYST ");
            _projectHelperAnalyst.UnassignProject(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.assignee, Is.Null);
            Assert.That(_createdSbProject.owners.Count, Is.Zero);
            #endregion

            #region Assign Sb-Project to Analyst again
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY ANALYST AGAIN");
            _projectHelperAnalyst.AssignProject(_createdSbProject);
            _createdSbProject= _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_createdSbProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Calculate Sb-Project
            Debug.WriteLine("CALCULATING OF SB-PROJECT");
            _projectHelperAnalyst.CalculateSBproject(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("calculating"));
            _projectHelperAnalyst.WaitCalculatingSBproject(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("adjustments"));
            Assert.That(_createdSbProject.additional_attributes.total_amount, Is.Not.Null);
            #endregion

            #region Generate Report for Sb-Project
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReport(_createdSbProject);
            _generatedReports= _projectHelperAnalyst.GetReportsInfo(_createdSbProject.id);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("summary_by_evaluation_bu"));
            #endregion

            #region Send Sb-Project to manager
            Debug.WriteLine("SEND SB-PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Sb-Project by Manager
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_createdSbProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            #endregion

            #region Unassign Sb-Project by Admin
            Debug.WriteLine("UNASSIGNING OF SB-PROJECT BY ADMIN");
            _projectHelperAdmin.UnassignProjectFromManager(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.assignee, Is.Null);
            Assert.That(_createdSbProject.owners.Count==1);
            #endregion

            #region Reassign Sb-Project to Manager
            Debug.WriteLine("REASSIGNING OF SB-PROJECT TO MANAGER");
            _projectHelperAdmin.ReassignSbProjectToManager(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_createdSbProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            _projectHelperAnalyst.PaymentsPackageOff();
            #endregion

            #region Approve Sb-Project by Manager
            Debug.WriteLine("APPROVE SB-PROJECT BY MANAGER");
            _projectHelperManager.ApproveProjectByManager(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_createdSbProject.assignee, Is.Null);
            #endregion

            #region Approve SbProject by Client
            Debug.WriteLine("APPROVE SB-PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE SB PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_step, Is.Null);
            Assert.That(_createdSbProject.is_closed);
            Assert.That(_createdSbProject.assignee, Is.Null);
            #endregion
        }


        [Test]
        public void SmokeTestPaymentWorkflow()
        {
            #region Create SB-project
            Debug.WriteLine("\n CREATING OF SB-PROJECT");
            _createdSbProject = _projectHelperAnalyst.CreateSBProject();
            Debug.WriteLine("Created SB project id: " + _createdSbProject.id + " \n Arrangement name: " + _createdSbProject.arrangement_name + '\n');
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("pending_calculation"));
            Assert.That(_createdSbProject.assignee, Is.Null);
            #endregion

            #region Assign Sb-Project to Analyst
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignProject(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_createdSbProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Calculate Sb-Project
            Debug.WriteLine("CALCULATING OF SB-PROJECT");
            _projectHelperAnalyst.CalculateSBproject(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("calculating"));
            _projectHelperAnalyst.WaitCalculatingSBproject(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("adjustments"));
            Assert.That(_createdSbProject.additional_attributes.total_amount, Is.Not.Null);
            #endregion

            #region Generate Report for Sb-Project
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReport(_createdSbProject);
            _generatedReports = _projectHelperAnalyst.GetReportsInfo(_createdSbProject.id);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("summary_by_evaluation_bu"));
            #endregion

            #region Send Sb-Project to manager
            Debug.WriteLine("SEND SB-PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Sb-Project by Manager
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_createdSbProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            _projectHelperAnalyst.PaymentsPackageOn();
            #endregion

            #region Approve Sb-Project by Manager
            Debug.WriteLine("APPROVE SB-PROJECT BY MANAGER");
            _sbProjectPayments = _projectHelperAnalyst.GetSbPayments(_createdSbProject);
            _projectHelperManager.ApproveProjectByManager(_createdSbProject);
            _createdSbProject = _projectHelperAnalyst.GetProjectInfo(_createdSbProject.id);
            Assert.That(_createdSbProject.workflow_substep[0], Is.EqualTo("waiting_for_payments"));
            
            #endregion
            
            #region Create Payment-project
            Debug.WriteLine("CREATING OF PAYMENT-PROJECT");
            _createdPaymentProject = _projectHelperAnalyst.CreatePaymentProject();
            Debug.WriteLine("Created payment project id: " + _createdPaymentProject.id +'\n');
            Assert.That(_createdPaymentProject.workflow_substep[0], Is.EqualTo("add_payments"));
            Assert.That(_createdPaymentProject.assignee, Is.Null);
            #endregion

            #region Assign Payment-Project to Analyst
            Debug.WriteLine("ASSIGNING OF Payment-PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignProject(_createdPaymentProject);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            Assert.That(_createdPaymentProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_createdPaymentProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Add payments to Payment-Project
            Debug.WriteLine("ADD PAYMENTS TO PAYMENT-PROJECT FROM PAYMENT PACKAGE");
            _sbProjectPayments = _projectHelperAnalyst.GetSbPayments(_createdSbProject);
            _projectHelperAnalyst.AddPaymentsPaymentPackage(_createdPaymentProject, _sbProjectPayments);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            Assert.That(_createdPaymentProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_createdPaymentProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Generate Report for Payment-Project
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReport(_createdPaymentProject);
            _generatedReports = _projectHelperAnalyst.GetReportsInfo(_createdPaymentProject.id);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("payment_package_summary_pay_to_bu"));
            #endregion

            #region Send Payment-Project to manager
            Debug.WriteLine("SEND PAYMENT-PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_createdPaymentProject);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            Assert.That(_createdPaymentProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Payment-Project by Manager
            Debug.WriteLine("ASSIGNING OF PAYMENT-PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_createdPaymentProject);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            Assert.That(_createdPaymentProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_createdPaymentProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));

            #endregion

            #region Unassign Payment-Project by Admin
            Debug.WriteLine("UNASSIGNING OF PAYMENT-PROJECT BY ADMIN");
            _projectHelperAdmin.UnassignProjectFromManager(_createdPaymentProject);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            Assert.That(_createdPaymentProject.assignee, Is.Null);
            Assert.That(_createdPaymentProject.owners.Count == 1);
            #endregion

            #region Reassign Payment-Project to Manager
            Debug.WriteLine("REASSIGNING OF PAYMENT-PROJECT TO MANAGER");
            _projectHelperAdmin.ReassignSbProjectToManager(_createdPaymentProject);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            Assert.That(_createdPaymentProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_createdPaymentProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            
            #endregion
  
            #region Approve Payment-Project by Manager
            Debug.WriteLine("APPROVE PAYMENT-PROJECT BY MANAGER");
           
            _projectHelperManager.ApproveProjectByManager(_createdPaymentProject);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            Assert.That(_createdPaymentProject.workflow_substep[0], Is.EqualTo("final_review"));       
            Assert.That(_createdPaymentProject.assignee, Is.Null);
            #endregion

            #region Approve Payment Project by Client
            Debug.WriteLine("APPROVE PAYMENT-PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_createdPaymentProject);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            Assert.That(_createdPaymentProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_createdPaymentProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_createdPaymentProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE PAYMENT PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_createdPaymentProject);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_createdPaymentProject);
            _createdPaymentProject = _projectHelperAnalyst.GetProjectInfo(_createdPaymentProject.id);
            _projectHelperAnalyst.PaymentsPackageOff();
            Assert.That(_createdPaymentProject.workflow_step, Is.Null);
            Assert.That(_createdPaymentProject.is_closed);
            Assert.That(_createdPaymentProject.assignee, Is.Null);
            #endregion
        }


        [TearDown]
        public void PostConditions()
        { 
            Debug.WriteLine("DELETING OF SB-PROJECT");
            _projectHelperAdmin.DeleteProject(_createdSbProject);
            _projectHelperAdmin.DeleteProject(_createdPaymentProject);
        }
    }
}
