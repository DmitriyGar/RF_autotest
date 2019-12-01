using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Models
{
    public class Datum
    {
        public string service_name { get; set; }
        public DateTime effective_date { get; set; }
        public string service_code { get; set; }
        public bool subscribe { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public object integration_date { get; set; }
        public object integration_contact { get; set; }
        public IList<string> roles { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public bool? active { get; set; }
        public bool? subscribe_as_saas { get; set; }
        public bool? enable_template_reports { get; set; }
        public string test_key { get; set; }
        public bool? subscribes { get; set; }
        public bool? show_system_error_details { get; set; }
        public bool? is_payment_package_enabled { get; set; }
        public bool? is_erp_negative_payments_enabled { get; set; }
        public bool? is_erp_active { get; set; }
        public bool? direct_transaction_upload { get; set; }
        public bool? connect_auto_approve { get; set; }
    }

    public class RFServices
    {
        public IList<Datum> data { get; set; }
        public DateTime response_datetime { get; set; }
        public string status { get; set; }
    }


}
