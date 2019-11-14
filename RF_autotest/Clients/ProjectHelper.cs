using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Clients
{
    class ProjectHelper 
    {
        private string _createSbProjectResource =
            "http://rf-qa-app1.zkpsl3bk22wuvfspx0uhovklae.bx.internal.cloudapp.net:8080/rfprojects/v1/projects"; //post
        private string _assignSbProjectResource = "/rfworkflow/v1/workflows/{0}/add_payments/assign";//put
        private string _generateReportSbProjectResource = "/rfreports/v1/{0}/reports"; //post
        private string _calculateSbProjectResource = "/rfworkflow/v1/workflows/{0}/adjustments/complete"; //put
        private string _SendToManagerResource = "/rfworkflow/v1/workflows/{0}/add_payments/complete";//put
        private string _ApproveByManagerResource = "";

        void LoginAsAnalist()
        {

        }
        void CreateSBProject()
        {
            RequestBuilder request = new RequestBuilder();
            request.PostRequest("session_id","body");//todo
        }
        void AssignProject()
        {

        }
        void UnassignProject()
        {

        }
        void Calculate_SBproject()
        {

        }
        void SendProjectToManager()
        {

        }
        void PaymentsPackageOff()
        {

        }
        void RejectProjectByManager()
        {

        }
        void AproveProjectByManager()
        {

        }
        void ReturnProjectByClient()
        {

        }
        void AproveProjectByClient()
        {

        }
        void CloseProjectWithProjectDetails()
        {

        }

    }
}
