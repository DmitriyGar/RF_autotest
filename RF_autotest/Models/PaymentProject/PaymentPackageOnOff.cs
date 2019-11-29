namespace RF_autotest.Models.PaymentProject
{
    class PaymentPackageOnOff
    {
            public string service_name { get; set; }
            public bool is_payment_package_enabled { get; set; }
            public bool is_erp_negative_payments_enabled { get; set; }
            public bool is_erp_active { get; set; }
            public bool direct_transaction_upload { get; set; }
            public bool connect_auto_approve { get; set; }
    }
}
