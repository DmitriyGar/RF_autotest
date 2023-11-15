using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Settings
{
    public static class Configuration
    {
        public static readonly string UrlQaEnvironment= "http://rf-qa-app1.cloudapp.net";
        public static readonly string UrlIntEnvironment = "http://cbd-int-gateway1.cloudapp.net";
        public static readonly string HostQaDataBaseEnvironment = "rf-qa-db1.cloudapp.net";
        public static readonly string HostINTDataBaseEnvironment = "rf-qa-db1.cloudapp.net";
        public static readonly string Port8080 = "8080";
        public static readonly string LoginRfAnalyst = "analyst1";
        public static readonly string NameRfAnalyst = "analyst@cis-cust.lan";
        public static readonly string LoginRfManager = "manager";
        public static readonly string LoginRfAdmin = "admin";
        public static readonly string Client = "umbrella";
        public static readonly string Password = "dummy#123";
              
    }
}
