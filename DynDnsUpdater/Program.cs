﻿using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DynDnsUpdater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                if (!args.Any())
                    new DynDnsUpdater().LoopCheck();
                else
                {
                    switch (args[0].Trim())
                    {
                        case "/i":
                        case "/install":
                            Install();
                            break;
                        case "/u":
                        case "/uninstall":
                            Uninstall();
                            break;
                    }
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new DynDnsUpdater() 
                };
                ServiceBase.Run(ServicesToRun);
            }

        }

        private static void Install()
        {
            try
            {
                var svc = new ServiceController(ProjectInstaller.ServiceName);
                Console.WriteLine(svc.DisplayName);
                if (svc.Status == ServiceControllerStatus.Running)
                    svc.Stop();
                Uninstall();
            }
            catch (InvalidOperationException) { }
            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
        }

        private static void Uninstall()
        {
            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
        }
    }
}
