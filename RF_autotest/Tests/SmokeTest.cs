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
        private CreatedProject _sbProject;
        private CreatedProject _sbFullRecalculationProject;
        private CreatedProject _sbPartialRecalculationProject;
        private CreatedProject _sbPartialReversalProject;
        private CreatedProject _sbFullReversalProject;
        private CreatedProject _paymentProject;
        private List<ReportsInfo> _generatedReports;
        private List<Payments> _sbProjectPayments;

        public SmokeTest()
        {
            _projectHelperAnalyst = new ProjectHelperAnalyst();
            _projectHelperAdmin = new ProjectHelperAdmin();
            _projectHelperManager = new ProjectHelperManager();
            _sbProject = new CreatedProject();
            _sbFullRecalculationProject = new CreatedProject();
            _sbPartialRecalculationProject = new CreatedProject();
            _sbPartialReversalProject = new CreatedProject();
            _sbFullReversalProject = new CreatedProject();
            _generatedReports = new List<ReportsInfo>();
            _paymentProject = new CreatedProject();
            _sbProjectPayments = new List<Payments>();


        }
        [OneTimeSetUp]
        public void LoginToApp()
        {
             _projectHelperAnalyst.LoginToApp();
             _projectHelperAdmin.LoginToApp();
             _projectHelperManager.LoginToApp();
        }

        [SetUp]
        public void PreConditions()
        {
            #region Create SB-project
            Debug.WriteLine("*** CREATING OF SB-PROJECT ***");
            _sbProject = _projectHelperAnalyst.CreateSBProject();
            Debug.WriteLine("Created SB project id: " + _sbProject + "\n Arrangement name: " + _sbProject.arrangement_name.FirstOrDefault() + '\n');
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("pending_calculation"));
            Assert.That(_sbProject.project_type, Is.EqualTo("sb_rebate"));
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion

            #region Assign Sb-Project to Analyst
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignProject(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_sbProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Unassign Sb-Project by Analyst
            Debug.WriteLine("UNASSIGNING OF SB-PROJECT BY ANALYST ");
            _projectHelperAnalyst.UnassignProject(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.assignee, Is.Null);
            Assert.That(_sbProject.owners.Count, Is.Zero);
            #endregion

            #region Assign Sb-Project to Analyst again
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY ANALYST AGAIN");
            _projectHelperAnalyst.AssignProject(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_sbProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Calculate Sb-Project
            Debug.WriteLine("CALCULATING OF SB-PROJECT");
            _projectHelperAnalyst.CalculateSBproject(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("calculating"));
            _projectHelperAnalyst.WaitCalculatingSBproject(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("adjustments"));
            Assert.That(_sbProject.additional_attributes.total_amount, Is.Not.Null);
            #endregion

            #region Generate Report for Sb-Project
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReport(_sbProject);
            _generatedReports = _projectHelperAnalyst.GetReportsInfo(_sbProject.id);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("summary_by_evaluation_bu"));
            #endregion

            #region Send Sb-Project to manager
            Debug.WriteLine("SEND SB-PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Sb-Project by Manager
            Debug.WriteLine("ASSIGNING OF SB-PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            #endregion
        }

        [Test]
        public void SmokeTestSBWorkflow()
        {
            

            #region Unassign Sb-Project by Admin
            Debug.WriteLine("UNASSIGNING OF SB-PROJECT BY ADMIN");
            _projectHelperAdmin.UnassignProjectFromManager(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.assignee, Is.Null);
            Assert.That(_sbProject.owners.Count==1);
            #endregion

            #region Reassign Sb-Project to Manager
            Debug.WriteLine("REASSIGNING OF SB-PROJECT TO MANAGER");
            _projectHelperAdmin.ReassignSbProjectToManager(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            _projectHelperAnalyst.PaymentsPackageOff();
            #endregion

            #region Approve Sb-Project by Manager
            Debug.WriteLine("APPROVE SB-PROJECT BY MANAGER");
            _projectHelperManager.ApproveProjectByManager(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion

            #region Approve SbProject by Client
            Debug.WriteLine("APPROVE SB-PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_sbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE SB PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.Null);
            Assert.That(_sbProject.is_closed);
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion
        }


        [Test]
        public void SmokeTestPaymentWorkflow()
        {
           
            #region Approve Sb-Project by Manager
            Debug.WriteLine("APPROVE SB-PROJECT BY MANAGER");
            _projectHelperAnalyst.PaymentsPackageOn();
            _sbProjectPayments = _projectHelperAnalyst.GetSbPayments(_sbProject);
            _projectHelperManager.ApproveProjectByManager(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("waiting_for_payments"));
            
            #endregion
            
            #region Create Payment-project
            Debug.WriteLine("*** CREATING OF PAYMENT-PROJECT ***");
            _paymentProject = _projectHelperAnalyst.CreatePaymentProject();
            Debug.WriteLine("Created payment project id: " + _paymentProject +'\n');
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            Assert.That(_paymentProject.workflow_substep[0], Is.EqualTo("add_payments"));
            Assert.That(_paymentProject.project_type, Is.EqualTo("sb_rebate_payments"));
            Assert.That(_paymentProject.assignee, Is.Null);
            #endregion

            #region Assign Payment-Project to Analyst
            Debug.WriteLine("ASSIGNING OF PAYMENT-PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignProject(_paymentProject);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            Assert.That(_paymentProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_paymentProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Add payments to Payment-Project
            Debug.WriteLine("ADD PAYMENTS TO PAYMENT-PROJECT FROM PAYMENT PACKAGE");
            _sbProjectPayments = _projectHelperAnalyst.GetSbPayments(_sbProject);
            _projectHelperAnalyst.AddPaymentsPaymentPackage(_paymentProject, _sbProjectPayments);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            Assert.That(_paymentProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_paymentProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Generate Report for Payment-Project
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReport(_paymentProject);
            _projectHelperAnalyst.GenerateReport(_paymentProject);
            _generatedReports = _projectHelperAnalyst.GetReportsInfo(_paymentProject.id);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("payment_package_summary_pay_to_bu"));
            #endregion

            #region Send Payment-Project to manager
            Debug.WriteLine("SEND PAYMENT-PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_paymentProject);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            Assert.That(_paymentProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Payment-Project by Manager
            Debug.WriteLine("ASSIGNING OF PAYMENT-PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_paymentProject);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            Assert.That(_paymentProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_paymentProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));

            #endregion

            #region Unassign Payment-Project by Admin
            Debug.WriteLine("UNASSIGNING OF PAYMENT-PROJECT BY ADMIN");
            _projectHelperAdmin.UnassignProjectFromManager(_paymentProject);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            Assert.That(_paymentProject.assignee, Is.Null);
            Assert.That(_paymentProject.owners.Count == 1);
            #endregion

            #region Reassign Payment-Project to Manager
            Debug.WriteLine("REASSIGNING OF PAYMENT-PROJECT TO MANAGER");
            _projectHelperAdmin.ReassignSbProjectToManager(_paymentProject);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            Assert.That(_paymentProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_paymentProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            
            #endregion
  
            #region Approve Payment-Project by Manager
            Debug.WriteLine("APPROVE PAYMENT-PROJECT BY MANAGER");
           
            _projectHelperManager.ApproveProjectByManager(_paymentProject);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            Assert.That(_paymentProject.workflow_substep[0], Is.EqualTo("final_review"));       
            Assert.That(_paymentProject.assignee, Is.Null);
            #endregion

            #region Approve Payment Project by Client
            Debug.WriteLine("APPROVE PAYMENT-PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_paymentProject);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            Assert.That(_paymentProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_paymentProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_paymentProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE PAYMENT PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_paymentProject);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_paymentProject);
            _paymentProject = _projectHelperAnalyst.GetProjectInfo(_paymentProject);
            _projectHelperAnalyst.PaymentsPackageOff();
            Assert.That(_paymentProject.workflow_step, Is.Null);
            Assert.That(_paymentProject.is_closed);
            Assert.That(_paymentProject.assignee, Is.Null);
            #endregion
        }

        [Test]
        public void SmokeTestSbFullRecalculationWorkflow()
        {
            
            #region Approve Sb-Project by Manager
            Debug.WriteLine("APPROVE SB-PROJECT BY MANAGER");
            _projectHelperAnalyst.PaymentsPackageOff();
            _projectHelperManager.ApproveProjectByManager(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion

            #region Approve SbProject by Client
            Debug.WriteLine("APPROVE SB-PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_sbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE SB PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.Null);
            Assert.That(_sbProject.is_closed);
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion
        
            #region Create Full Recalculating SB-project 
            Debug.WriteLine("\n CREATING OF FULL RECALCULATION SB-PROJECT ");
            _sbFullRecalculationProject = _projectHelperAnalyst.CreateSbFullRecalculationProject(_sbProject);
            Debug.WriteLine("Created project id: " + _sbFullRecalculationProject + "\n Arrangement name: " + _sbFullRecalculationProject.arrangement_name.FirstOrDefault() + '\n');
            _projectHelperAnalyst.AssignProject(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.workflow_substep[0], Is.EqualTo("pending_calculation"));
            Assert.That(_sbFullRecalculationProject.project_type, Is.EqualTo("sb_rebate_recalculation"));
            Assert.That(_sbFullRecalculationProject.assignee, Is.Null);
            #endregion

            #region Assign Full Recalculation Sb-Project to Analyst
            Debug.WriteLine("ASSIGNING OF FULL RECALCULATION PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignProject(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_sbFullRecalculationProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Calculate Full Recalculation Sb-Project
            Debug.WriteLine("CALCULATING OF FULL RECALCULATION PROJECT");
            _projectHelperAnalyst.CalculateSBproject(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.workflow_substep[0], Is.EqualTo("calculating"));
            _projectHelperAnalyst.WaitCalculatingSBproject(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.workflow_substep[0], Is.EqualTo("adjustments"));
            Assert.That(_sbFullRecalculationProject.additional_attributes.total_amount, Is.Not.Null);
            #endregion

            #region Generate Report for Full Recalculation Sb-Project
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReport(_sbFullRecalculationProject);
            _generatedReports = _projectHelperAnalyst.GetReportsInfo(_sbFullRecalculationProject.id);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("summary_by_evaluation_bu"));
            #endregion

            #region Send Full Recalculation Sb-Project to manager
            Debug.WriteLine("SEND FULL RECALCULATION PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Full Recalculation Sb-Project by Manager
            Debug.WriteLine("ASSIGNING OF FULL RECALCULATION PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbFullRecalculationProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            #endregion

            #region Unassign  Full Recalculation Project by Admin
            Debug.WriteLine("UNASSIGNING OF FULL RECALCULATION PROJECT BY ADMIN");
            _projectHelperAdmin.UnassignProjectFromManager(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.assignee, Is.Null);
            Assert.That(_sbFullRecalculationProject.owners.Count == 1);
            #endregion

            #region Reassign  Full Recalculation Project to Manager
            Debug.WriteLine("REASSIGNING OF FULL RECALCULATION PROJECT TO MANAGER");
            _projectHelperAdmin.ReassignSbProjectToManager(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbFullRecalculationProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));

            #endregion

            #region Approve Full Recalculation Sb-Project by Manager
            Debug.WriteLine("APPROVE FULL RECALCULATION PROJECT BY MANAGER");
            _projectHelperAnalyst.PaymentsPackageOff();
            _projectHelperManager.ApproveProjectByManager(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_sbFullRecalculationProject.assignee, Is.Null);
            #endregion

            #region Approve Full Recalculation SbProject by Client
            Debug.WriteLine("APPROVE FULL RECALCULATION PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_sbFullRecalculationProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_sbFullRecalculationProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE FULL RECALCULATION PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_sbFullRecalculationProject);
            _sbFullRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbFullRecalculationProject);
            Assert.That(_sbFullRecalculationProject.workflow_step, Is.Null);
            Assert.That(_sbFullRecalculationProject.is_closed);
            Assert.That(_sbFullRecalculationProject.assignee, Is.Null);
            #endregion
        }

        [Test]
        public void SmokeTestSbFullReversalWorkflow()
        {
           
            #region Approve Sb-Project by Manager
            Debug.WriteLine("APPROVE SB-PROJECT BY MANAGER");
            _projectHelperAnalyst.PaymentsPackageOff();
            _projectHelperManager.ApproveProjectByManager(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion

            #region Approve SbProject by Client
            Debug.WriteLine("APPROVE SB-PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_sbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE SB PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.Null);
            Assert.That(_sbProject.is_closed);
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion
            
            #region Create Full Reversal SB-project 
            Debug.WriteLine("*** CREATING OF FULL REVERSAL SB-PROJECT ***");
            _sbFullReversalProject = _projectHelperAnalyst.CreateSbFullReversalProject(_sbProject);
            Debug.WriteLine("Created project id: " + _sbFullReversalProject + "\n Arrangement name: " + _sbFullReversalProject.arrangement_name.FirstOrDefault() + '\n');
            _projectHelperAnalyst.AssignProject(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.workflow_substep[0], Is.EqualTo("pending_calculation"));
            Assert.That(_sbFullReversalProject.project_type, Is.EqualTo("sb_rebate_reversal"));
            Assert.That(_sbFullReversalProject.assignee, Is.Null);
            #endregion

            #region Assign Full Reversal Sb-Project to Analyst
            Debug.WriteLine("ASSIGNING OF FULL REVERSAL PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignProject(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_sbFullReversalProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Calculate Full Reversal Sb-Project
            Debug.WriteLine("CALCULATING OF FULL REVERSAL PROJECT");
            _projectHelperAnalyst.CalculateSBproject(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.workflow_substep[0], Is.EqualTo("calculating"));
            _projectHelperAnalyst.WaitCalculatingSBproject(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.workflow_substep[0], Is.EqualTo("adjustments"));
            Assert.That(_sbFullReversalProject.additional_attributes.total_amount, Is.Not.Null);
            #endregion

            #region Generate Report for Full Reversal Sb-Project
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReport(_sbFullReversalProject);
            _generatedReports = _projectHelperAnalyst.GetReportsInfo(_sbFullReversalProject.id);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("summary_by_evaluation_bu"));
            #endregion

            #region Send Full Reversal Sb-Project to manager
            Debug.WriteLine("SEND FULL REVERSAL PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Full Reversal Sb-Project by Manager
            Debug.WriteLine("ASSIGNING OF FULL REVERSAL PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbFullReversalProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            #endregion

            #region Unassign Full Reversal Project by Admin
            Debug.WriteLine("UNASSIGNING OF FULL REVERSAL PAYMENT-PROJECT BY ADMIN");
            _projectHelperAdmin.UnassignProjectFromManager(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.assignee, Is.Null);
            Assert.That(_sbFullReversalProject.owners.Count == 1);
            #endregion

            #region Reassign Full Reversal Project to Manager
            Debug.WriteLine("REASSIGNING OF FULL REVERSAL PAYMENT-PROJECT TO MANAGER");
            _projectHelperAdmin.ReassignSbProjectToManager(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbFullReversalProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            #endregion

            #region Approve Full Reversal Sb-Project by Manager
            Debug.WriteLine("APPROVE FULL REVERSAL PROJECT BY MANAGER");
            _projectHelperAnalyst.PaymentsPackageOff();
            _projectHelperManager.ApproveProjectByManager(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_sbFullReversalProject.assignee, Is.Null);
            #endregion

            #region Approve Full Reversal SbProject by Client
            Debug.WriteLine("APPROVE FULL REVERSAL PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_sbFullReversalProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_sbFullReversalProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE FULL REVERSAL PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_sbFullReversalProject);
            _sbFullReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbFullReversalProject);
            Assert.That(_sbFullReversalProject.workflow_step, Is.Null);
            Assert.That(_sbFullReversalProject.is_closed);
            Assert.That(_sbFullReversalProject.assignee, Is.Null);
            #endregion
        }

        [Test]
        public void SmokeTestSbPartialRecalculationWorkflow()
        {

            #region Approve Sb-Project by Manager
            Debug.WriteLine("APPROVE SB-PROJECT BY MANAGER");
            _projectHelperAnalyst.PaymentsPackageOff();
            _projectHelperManager.ApproveProjectByManager(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion

            #region Approve SbProject by Client
            Debug.WriteLine("APPROVE SB-PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_sbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE SB PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.Null);
            Assert.That(_sbProject.is_closed);
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion

            #region Create Partial Recalculating SB-project 
            Debug.WriteLine("*** CREATING OF PARTIAL RECALCULATION SB-PROJECT ***");
            _sbPartialRecalculationProject = _projectHelperAnalyst.CreatePartialRecalculationProject(_sbProject);
            Debug.WriteLine("Created project id: " + _sbPartialRecalculationProject.id+ '\n');
            _projectHelperAnalyst.AssignProject(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.workflow_substep[0], Is.EqualTo("project_setup"));
            Assert.That(_sbPartialRecalculationProject.project_type, Is.EqualTo("sb_rebate_partial_recalculation"));
            Assert.That(_sbPartialRecalculationProject.assignee, Is.Null);
            #endregion

            #region Assign Partial Recalculation Sb-Project to Analyst
            Debug.WriteLine("ASSIGNING OF PARTIAL RECALCULATION PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignProject(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_sbPartialRecalculationProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Setup Partial Recalculation Sb-Project to Analyst
            Debug.WriteLine("PROJECT SETUP");
            _projectHelperAnalyst.SetupPartialProject(_sbPartialRecalculationProject, _sbProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.workflow_substep[0], Is.EqualTo("pending_calculation"));
            Assert.That(_sbPartialRecalculationProject.arrangement_name, Is.EqualTo(_sbPartialRecalculationProject.arrangement_name));
            Assert.That(_sbPartialRecalculationProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Calculate Partial Recalculation Sb-Project
            Debug.WriteLine("CALCULATING OF PARTIAL RECALCULATION PROJECT");
            _projectHelperAnalyst.CalculateSBproject(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.workflow_substep[0], Is.EqualTo("calculating"));
            _projectHelperAnalyst.WaitCalculatingSBproject(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.workflow_substep[0], Is.EqualTo("adjustments"));
            Assert.That(_sbPartialRecalculationProject.additional_attributes.total_amount, Is.Not.Null);
            #endregion

            #region Generate Report for Partial Recalculation Sb-Project
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReport(_sbPartialRecalculationProject);
            _generatedReports = _projectHelperAnalyst.GetReportsInfo(_sbPartialRecalculationProject.id);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("summary_by_evaluation_bu"));
            #endregion

            #region Send Partial Recalculation Sb-Project to manager
            Debug.WriteLine("SEND PARTIAL RECALCULATION PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Partial Recalculation Sb-Project by Manager
            Debug.WriteLine("ASSIGNING OF PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbPartialRecalculationProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            #endregion

            #region Unassign Partial Recalculation Project by Admin
            Debug.WriteLine("UNASSIGNING OF PARTIAL RECALCULATION PROJECT BY ADMIN");
            _projectHelperAdmin.UnassignProjectFromManager(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.assignee, Is.Null);
            Assert.That(_sbPartialRecalculationProject.owners.Count == 1);
            #endregion

            #region Reassign Partial Recalculation Project to Manager
            Debug.WriteLine("REASSIGNING OF PARTIAL RECALCULATION PROJECT TO MANAGER");
            _projectHelperAdmin.ReassignSbProjectToManager(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbPartialRecalculationProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));

            #endregion

            #region Approve Partial Recalculation Sb-Project by Manager
            Debug.WriteLine("APPROVE PARTIAL RECALCULATION PROJECT BY MANAGER");
            _projectHelperAnalyst.PaymentsPackageOff();
            _projectHelperManager.ApproveProjectByManager(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_sbPartialRecalculationProject.assignee, Is.Null);
            #endregion

            #region Approve Partial Recalculation SbProject by Client
            Debug.WriteLine("APPROVE PARTIAL RECALCULATION PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_sbPartialRecalculationProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_sbPartialRecalculationProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE PARTIAL RECALCULATION PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_sbPartialRecalculationProject);
            _sbPartialRecalculationProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialRecalculationProject);
            Assert.That(_sbPartialRecalculationProject.workflow_step, Is.Null);
            Assert.That(_sbPartialRecalculationProject.is_closed);
            Assert.That(_sbPartialRecalculationProject.assignee, Is.Null);
            #endregion
        }

        [Test]
        public void SmokeTestSbPartialReversalWorkflow()
        {

            #region Approve Sb-Project by Manager
            Debug.WriteLine("APPROVE SB-PROJECT BY MANAGER");
            _projectHelperAnalyst.PaymentsPackageOff();
            _projectHelperManager.ApproveProjectByManager(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion

            #region Approve SbProject by Client
            Debug.WriteLine("APPROVE SB-PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_sbProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_sbProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE SB PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_sbProject);
            _sbProject = _projectHelperAnalyst.GetProjectInfo(_sbProject);
            Assert.That(_sbProject.workflow_step, Is.Null);
            Assert.That(_sbProject.is_closed);
            Assert.That(_sbProject.assignee, Is.Null);
            #endregion

            #region Create Partial Reversal SB-project 
            Debug.WriteLine("*** CREATING OF PARTIAL REVERSAL SB-PROJECT ***");
            _sbPartialReversalProject = _projectHelperAnalyst.CreatePartialReversalProject(_sbProject);
            Debug.WriteLine("Created project id: " + _sbPartialReversalProject.id + '\n');
            _projectHelperAnalyst.AssignProject(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.workflow_substep[0], Is.EqualTo("project_setup"));
            Assert.That(_sbPartialReversalProject.project_type, Is.EqualTo("sb_rebate_partial_reversal"));
            Assert.That(_sbPartialReversalProject.assignee, Is.Null);
            #endregion

            #region Assign Partial Partial Sb-Project to Analyst
            Debug.WriteLine("ASSIGNING OF PARTIAL REVERSAL PROJECT BY ANALYST ");
            _projectHelperAnalyst.AssignProject(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            Assert.That(_sbPartialReversalProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Setup Partial Reversal Sb-Project to Analyst
            Debug.WriteLine("PROJECT SETUP");
            _projectHelperAnalyst.SetupPartialProject(_sbPartialReversalProject, _sbProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.workflow_substep[0], Is.EqualTo("pending_calculation"));
            Assert.That(_sbPartialReversalProject.arrangement_name, Is.EqualTo(_sbPartialReversalProject.arrangement_name));
            Assert.That(_sbPartialReversalProject.owners.FirstOrDefault().user, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Calculate Partial Revarsal Sb-Project
            Debug.WriteLine("CALCULATING OF PARTIAL REVERSAL PROJECT");
            _projectHelperAnalyst.CalculateSBproject(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.workflow_substep[0], Is.EqualTo("calculating"));
            _projectHelperAnalyst.WaitCalculatingSBproject(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.workflow_substep[0], Is.EqualTo("adjustments"));
            Assert.That(_sbPartialReversalProject.additional_attributes.total_amount, Is.Not.Null);
            #endregion

            #region Generate Report for Partial Reversal Sb-Project
            Debug.WriteLine("GENERATING OF SAMMARY REPORT");
            _projectHelperAnalyst.GenerateReport(_sbPartialReversalProject);
            _generatedReports = _projectHelperAnalyst.GetReportsInfo(_sbPartialReversalProject.id);
            Assert.That(_generatedReports.FirstOrDefault().report_type, Is.EqualTo("summary_by_evaluation_bu"));
            #endregion

            #region Send Partial Reversal Sb-Project to manager
            Debug.WriteLine("SEND PARTIAL REVERSAL PROJECT TO MANAGER");
            _projectHelperAnalyst.SendProjectToManager(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.workflow_substep[0], Is.EqualTo("manager_review"));
            #endregion

            #region Assign Partial Reversal Sb-Project by Manager
            Debug.WriteLine("ASSIGNING OF PROJECT BY MANAGER");
            _projectHelperManager.AssignSbProject(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbPartialReversalProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));
            #endregion

            #region Unassign Partial Reversal Project by Admin
            Debug.WriteLine("UNASSIGNING OF PARTIAL REVERSAL PROJECT BY ADMIN");
            _projectHelperAdmin.UnassignProjectFromManager(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.assignee, Is.Null);
            Assert.That(_sbPartialReversalProject.owners.Count == 1);
            #endregion

            #region Reassign Partial Reversal Project to Manager
            Debug.WriteLine("REASSIGNING OF PARTIAL REVERSAL PROJECT TO MANAGER");
            _projectHelperAdmin.ReassignSbProjectToManager(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.assignee, Is.EqualTo("rf_manager@cis-cust.lan"));
            Assert.That(_sbPartialReversalProject.owners[1].user, Is.EqualTo("rf_manager@cis-cust.lan"));

            #endregion

            #region Approve Partial Reversal Sb-Project by Manager
            Debug.WriteLine("APPROVE PARTIAL REVERSAL PROJECT BY MANAGER");
            _projectHelperAnalyst.PaymentsPackageOff();
            _projectHelperManager.ApproveProjectByManager(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.workflow_substep[0], Is.EqualTo("final_review"));
            Assert.That(_sbPartialReversalProject.assignee, Is.Null);
            #endregion

            #region Approve Partial Reversal SbProject by Client
            Debug.WriteLine("APPROVE PARTIAL REVERSAL PROJECT BY CLIENT");
            _projectHelperAnalyst.AproveProjectByClient(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.workflow_step, Is.EqualTo("payment"));
            Assert.That(_sbPartialReversalProject.workflow_substep[0], Is.EqualTo("manual_payment"));
            Assert.That(_sbPartialReversalProject.assignee, Is.EqualTo("rf_analyst@cis-cust.lan"));
            #endregion

            #region Enter manual payments and close project
            Debug.WriteLine("ENTER MANUAL PAYMENTS AND CLOSE PARTIAL REVERSAL PROJECT");
            _projectHelperAnalyst.EnterPaymentDetails(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            _projectHelperAnalyst.CloseProjectWithPaymentDetails(_sbPartialReversalProject);
            _sbPartialReversalProject = _projectHelperAnalyst.GetProjectInfo(_sbPartialReversalProject);
            Assert.That(_sbPartialReversalProject.workflow_step, Is.Null);
            Assert.That(_sbPartialReversalProject.is_closed);
            Assert.That(_sbPartialReversalProject.assignee, Is.Null);
            #endregion
        }

        [TearDown]
        public void PostConditions()
        { 
            Debug.WriteLine("DELETING OF PROJECT");
            _projectHelperAdmin.DeleteProject(_sbProject);
            _projectHelperAdmin.DeleteProject(_paymentProject);
            _projectHelperAdmin.DeleteProject(_sbFullRecalculationProject);
            _projectHelperAdmin.DeleteProject(_sbFullReversalProject);
            _projectHelperAdmin.DeleteProject(_sbPartialRecalculationProject);
            _projectHelperAdmin.DeleteProject(_sbPartialReversalProject);
        }
    }
}
