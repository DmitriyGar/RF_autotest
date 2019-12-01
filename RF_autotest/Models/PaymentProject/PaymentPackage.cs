using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Models.PaymentProject
{
    public class Order
    {
        public string remarks { get; set; }
        public double unitPrice { get; set; }
        public int itemNumber { get; set; }
        public string conditionType { get; set; }
        public string materialNumber { get; set; }
    }

    public class Payment
    {
        public string id { get; set; }
        public object payment_package_uuid { get; set; }
        public object payment_package_name { get; set; }
        public object payment_package_project_type { get; set; }
        public object paid_amount { get; set; }
        public object payment_method { get; set; }
        public object payment_status { get; set; }
        public string pay_to_bu_uuid { get; set; }
        public string pay_to_identifier_type { get; set; }
        public string source_project_uuid { get; set; }
        public IList<string> arrangement_uuid_list { get; set; }
        public double payable_amount { get; set; }
        public double applied_credit { get; set; }
        public double total_payment { get; set; }
        public IList<Order> orders { get; set; }
        public string pay_to { get; set; }
        public string ship_to { get; set; }
        public string sold_to { get; set; }
        public string source_bu_uuid { get; set; }
        public string pay_to_bu_name { get; set; }
        public string pay_to_bu_master_id { get; set; }
        public object bill_to_address { get; set; }
        public string source_project_type { get; set; }
        public string source_project_name { get; set; }
        public string source_project_start_date { get; set; }
        public string source_project_end_date { get; set; }
        public string source_project_due_date { get; set; }
        public object source_project_period { get; set; }
        public IList<string> arrangement_name_list { get; set; }
        public IList<string> arrangement_identifier_list { get; set; }
        public IList<string> arrangement_entity_list { get; set; }
        public IList<string> arrangement_entity_identifier_list { get; set; }
    }

    public class PaymentPackage
    {
        public string pay_to_bu_uuid { get; set; }
        public string pay_to_bu_name { get; set; }
        public string pay_to_bu_master_id { get; set; }
        public double payable_amount { get; set; }
        public double applied_credit { get; set; }
        public double total_payment { get; set; }
        public IList<Payment> payments { get; set; }
    }

}
