using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Clients
{
    public class RequestBuilder : BaseClient
    {
        public RequestBuilder()
        {

        }

        public IRestResponse GetRequest(string session_id)
        {
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Authorization", "Session "+ session_id);
            request.AddHeader("Content-Type", "application/json");
           
            var response = SendRequest(request );
            return response;
        }

        public IRestResponse PostRequest(string session_id,string body)
        {
            var request = new RestRequest();
            request.Method = Method.POST;
            request.AddHeader("Authorization", "Session " + session_id);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(body);
            var response = SendRequest(request);
            return response;
        }
        IRestResponse PutRequest(string session_id, string body)
        {
            var request = new RestRequest();
            request.Method = Method.PUT;
            request.AddHeader("Authorization", "Session " + session_id);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(body);
            var response = SendRequest(request);
            return response;
        }
    }
}
