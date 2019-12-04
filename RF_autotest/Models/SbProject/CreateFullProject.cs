namespace RF_autotest.Models.SbProject
{
    class CreateFullProject
    {
        public string project_name { get; set; }
        public string project_description { get; set; }
        public string project_type { get; set; }
        public string client_short_name { get; set; }
        public string due_date { get; set; }
        public bool is_assign { get; set; }
        public string parent_project_uuid { get; set; }
    }

}
