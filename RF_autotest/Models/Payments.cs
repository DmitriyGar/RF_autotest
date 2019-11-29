using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Models
{
    public class Payments
    {
        public string id { get; set; }
        public string project_uuid { get; set; }
        public string pay_to_bu { get; set; }
        public string source_pay_to_bu { get; set; }
        public string pay_to_bu_name { get; set; }
        public string source_pay_to_bu_name { get; set; }
        public string source_pay_to_bu_master_id { get; set; }
        public double payable_amount { get; set; }
        public double applied_credit { get; set; }
        public double total_payment { get; set; }
        public IList<Order> orders { get; set; }
        public object payment_request_id { get; set; }
        public object payment_request_date { get; set; }
        public object payment_status { get; set; }
        public object response_date { get; set; }
        public string pay_to { get; set; }
        public string ship_to { get; set; }
        public string sold_to { get; set; }
        public object check_number { get; set; }
        public object check_date { get; set; }
        public object paid_amount { get; set; }
        public string pay_to_identifier_type { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public IList<string> arrangement_uuid_list { get; set; }
        public object payment_method { get; set; }
    }

    public class Order
    {
        public string remarks { get; set; }
        public double unitPrice { get; set; }
        public int itemNumber { get; set; }
        public string conditionType { get; set; }
        public string materialNumber { get; set; }
    }
}
