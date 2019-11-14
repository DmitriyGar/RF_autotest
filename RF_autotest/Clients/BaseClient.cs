using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Clients
{
    public class BaseClient
    {
        private String _mainResource=""; //todo
        private RestClient _client;

        protected BaseClient()
        {
            _client = new RestClient(_mainResource);
        }

        protected IRestResponse SendRequest(RestRequest request)
        {
            var response =_client.Execute(request);
            return response;
        }
    }
}
