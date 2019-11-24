﻿using RestSharp;
using RF_autotest.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace RF_autotest.Clients
{
    public class BaseClient
    {
        private string _mainResource= Configuration.UrlQaEnvironment+":"+Configuration.Port8080; //todo
        private IRestClient _client;

        protected BaseClient()
        {
            
            _client = new RestClient(_mainResource);
            _client.FollowRedirects = true;
        }

        protected IRestResponse SendRequest(IRestRequest request, [Optional]Dictionary<string, string> headers)
        {
          
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.AddHeader(header.Key, header.Value);
                        //Debug.WriteLine($"Request Headers: {header.Key} {header.Value}");
                    }
                }
            
            var response =_client.Execute(request);

            return response;
        }

        public IRestResponse Wait(IRestResponse response, [Optional]int time_seconds)
        {
            
            int timer = 30000;
            if (time_seconds == 0)
                time_seconds = timer;
            Stopwatch stopTimer = Stopwatch.StartNew();
            stopTimer.Reset();
            stopTimer.Start();
            while (Convert.ToInt32(stopTimer.ElapsedMilliseconds) < time_seconds) {
               
                if (response.IsSuccessful) break; 
            }
            stopTimer.Stop();
           // Thread.Sleep(2000);
            return response;
        }
    }
}
