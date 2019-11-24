using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Models.SbProject
{
    public class ReportsInfo
    {

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("project_uuid")]
        public string project_uuid { get; set; }

        [JsonProperty("document_uuid")]
        public string document_uuid { get; set; }

        [JsonProperty("report_type")]
        public string report_type { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("report_format")]
        public string report_format { get; set; }

        [JsonProperty("generation_date")]
        public DateTime generation_date { get; set; }

        [JsonProperty("generated_by")]
        public string generated_by { get; set; }

        [JsonProperty("generated_by_email")]
        public string generated_by_email { get; set; }

        [JsonProperty("hash")]
        public string hash { get; set; }

        [JsonProperty("group_type_title")]
        public string group_type_title { get; set; }

        [JsonProperty("origin_source_uuid")]
        public object origin_source_uuid { get; set; }

        [JsonProperty("source_type")]
        public object source_type { get; set; }
    }
}
