using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Settings
{
    public class Settings
    {
        public static readonly string UrlQaEnvironment= "http://rf-qa-app1.zkpsl3bk22wuvfspx0uhovklae.bx.internal.cloudapp.net/randf/#/umbrella/projects/unassigned";
        public static readonly string UrlIntEnvironment = "QA";
        public static readonly string PortQaEnvironment = "8080";
        public static readonly string LoginRfAnalyst = "rf_analyst";
        public static readonly string LoginRfManager = "rf_manager";
        public static readonly string PasswordRf = "Dummy#123";
        public static readonly string Configuration;

        Settings()
        {

        }
    }
}
