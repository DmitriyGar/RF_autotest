using RestSharp;
using RF_autotest.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Clients
{
    public class RequestBuilder : BaseClient
    {
        public RequestBuilder()
        {
           
        }
        public IRestResponse GetRequest(string resourse,  [Optional] Dictionary<string, string> _headers)
        {
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = resourse;
            request.RequestFormat = DataFormat.Json;

            var response = SendRequest(request, _headers);

            return Wait(response);
        }

        public IRestResponse PostRequest(string resourse, string body, Dictionary<string, string> _headers)
        {
            var request = new RestRequest();
            request.Resource = resourse;
            request.Method = Method.POST;
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(body);
            var response = SendRequest(request, _headers);
            return Wait(response);
        }

        public IRestResponse PutRequest(string resourse, string body, Dictionary<string, string> _headers)
        {
            var request = new RestRequest();
            request.Method = Method.PUT;
            request.Resource = resourse;
            request.AddJsonBody(body);
            var response = SendRequest(request, _headers);
            return Wait(response);
        }

        public IRestResponse DeleteRequest(string resourse, [Optional] Dictionary<string, string> _headers)
        {
            var request = new RestRequest();
            request.Method = Method.DELETE;
            request.Resource = resourse;
            var response = SendRequest(request, _headers);
            return Wait(response);
        }
        
    }
}






