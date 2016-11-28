using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using SingleApi.Common;
using SingleApi.Common.Cofiguration;
using LogManager = SingleApi.Common.LogManager;

namespace SingleApi.Server.Loader
{
    public class Program
    {
        private const string Host = "localhost";
        private const string CFG_EndpointsFolder = "Endpoints";
        private const string CFG_PluginsFolder = "Plugins";

        private static readonly ILog Logger = LogManager.Log;

        static Program()
        {
            LogManager.Congifure();
            LogManager.SetupUnhandledExceptionLogging();
        }

        private static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("SingleAPI ServiceHost (Loader).");
            Console.WriteLine();

            var serviceConfig = SingleApiConfigurationSection.GetSettings(Assembly.GetExecutingAssembly().Location + ".config",
                "SingleApiConfigurationSection");

            if (serviceConfig == null)
            {
                throw new InvalidOperationException("Invalid service configuration.");
            }

            if (string.IsNullOrEmpty(serviceConfig.EndpointsFolder))
            {
                serviceConfig.EndpointsFolder = CFG_EndpointsFolder;
            }

            if (string.IsNullOrEmpty(serviceConfig.PluginsFolder))
            {
                serviceConfig.PluginsFolder = CFG_PluginsFolder;
            }

            var options = LoaderConsoleOptions.Parse(new CmdLineParams(args));

            if (EnvProps.IsMono)
            {
                Logger.Info("Mono detected");
                Logger.Info(EnvProps.MonoVersion);
            }
            else
            {
                Logger.Info("Windows detected");
            }

            if (serviceConfig.EndpointConfigurations != null)
            {
                var serviceHostControllerParameters = new ServiceHostControllerParameters
                {
                    Port = int.Parse(options.Port),
                    ControllerName = "ServiceHostController",
                    BaseUri = Host
                };

                var endpointConfigurations = new List<ServiceHostControllerEnpointParameters>();
                for (var i = 0; i < serviceConfig.EndpointConfigurations.Count; i++)
                {
                    var serviceHostControllerEnpointParameters = new ServiceHostControllerEnpointParameters
                    {
                        ServiceName = serviceConfig.EndpointConfigurations[i].ServiceName,
                        TargetTypeName = serviceConfig.EndpointConfigurations[i].Type,
                        AssemblyName = serviceConfig.EndpointConfigurations[i].Assembly
                    };

                    endpointConfigurations.Add(serviceHostControllerEnpointParameters);
                }

                Run(serviceHostControllerParameters, endpointConfigurations.ToArray());
            }

            Environment.Exit(0);
        }

        private static void Run(
            ServiceHostControllerParameters serviceHostControllerParameters,
            ServiceHostControllerEnpointParameters[] serviceConfig)
        {
            Console.WriteLine("Loading...");
            Console.WriteLine();

            var baseUri = Tools.BuildUri(serviceHostControllerParameters.BaseUri, serviceHostControllerParameters.Port);

            using (var manager = new ServiceHostControllerDomainManager(serviceHostControllerParameters, serviceConfig))
            {
                Console.WriteLine("ServiceHost started at: {0}", baseUri);
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
                Console.WriteLine("Unloading...");
                manager.ShutDown();
            }
        }
    }
}