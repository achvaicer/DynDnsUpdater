using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using NLog;

namespace DynDnsUpdater
{
    public partial class DynDnsUpdater : ServiceBase
    {
        private static readonly string _domains = ConfigurationManager.AppSettings["domains"];
        private static readonly string _token = ConfigurationManager.AppSettings["token"];

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private static string _ip;

        
        private static Thread _thread;

        public DynDnsUpdater()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
           _thread = new Thread(LoopCheck);
           _thread.Start();
        }

        protected override void OnStop()
        {
            _thread.Abort();
        }

        internal void LoopCheck()
        {
            while (true)
            {
                var currentip = GetIP();
                _logger.Debug("Got IP {0}", currentip);
                if (currentip != _ip)
                    UpdateIP(currentip);
                
                Thread.Sleep(1800000);
            }
        }

        private static void UpdateIP(string ip)
        {
            var client = new RestClient("https://www.duckdns.org/");
            var req = new RestRequest("update/");
            req.AddParameter("domains", _domains);
            req.AddParameter("token", _token);
            req.AddParameter("ip", ip);

            var res = client.Get(req);
            if (VerifyResponse(res.Content))
                SetIP(ip);
        }

        private static void SetIP(string ip)
        {
            _ip = ip;
            _logger.Info("Updated to new ip {0}", ip);
        }

        private static bool VerifyResponse(string content)
        {
            return content == "OK";
        }

        private static string GetIP()
        {
            var check = new RestClient("http://jsonip.com/");
            var req = new RestRequest(Method.GET);
            var res = check.Get<JsonIp>(req);
            return res.Data.ip;
        }

        class JsonIp
        {
            public string ip { get; set; }
        }
    }
}
