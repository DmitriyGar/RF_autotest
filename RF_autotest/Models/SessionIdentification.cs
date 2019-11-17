using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Models
{
    public class SessionIdentification
    {

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }
}
