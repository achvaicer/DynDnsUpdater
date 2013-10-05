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

namespace DynDnsUpdater
{
    public partial class DynDnsUpdater : ServiceBase
    {
        private static readonly string _username = ConfigurationManager.AppSettings["username"];
        private static readonly string _password = ConfigurationManager.AppSettings["password"];
        private static readonly string _hostname = ConfigurationManager.AppSettings["hostname"];


        private static string _ip;

        private static readonly RestClient Client = new RestClient("http://members.dyndns.org/")
            {
                Authenticator = new HttpBasicAuthenticator(_username, _password)
            };


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
                if (currentip != _ip)
                    UpdateIP(currentip);
                
                Thread.Sleep(1800000);
            }
        }

        private static void UpdateIP(string ip)
        {
            var req = new RestRequest("nic/update/");
            req.AddParameter("hostname", _hostname);
            req.AddParameter("myip", ip);

            var res = Client.Get(req);
            if (VerifyResponse(ip, res.Content))
                _ip = ip;
        }

        private static bool VerifyResponse(string ip, string content)
        {
            return content == string.Format("good {0}", ip) || content == string.Format("nochg {0}", ip);
        }

        private static string GetIP()
        {
            var check = new RestClient("http://checkip.dyndns.com/");
            var req = new RestRequest(Method.GET);
            var res = check.Get(req);
            return new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b").Match(res.Content).Value;
        }
    }
}
