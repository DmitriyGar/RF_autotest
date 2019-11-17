using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Models
{
        public class CreateProject
        {
            public string project_type { get; set; }
            public string project_name { get; set; }
            public string project_description { get; set; }
            public string due_date { get; set; }
            public bool is_assign { get; set; }
            public string[] price_group_uuid_list { get; set; }
            public object[] program_uuid_list { get; set; }
            public string[] arrangement_uuid_list { get; set; }
            public string arrangement_uuid { get; set; }
            public Programs programs { get; set; }
            public string project_date_start { get; set; }
            public string project_date_end { get; set; }
        }

}
