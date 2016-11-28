using System;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Config;

namespace ServiceGenerator
{
    public class Program
    {
        internal static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        static Program()
        {
            XmlConfigurator.Configure();
        }

        [STAThread]
        private static void Main(string[] args)
        {
            if ((args.Length < 1) || ((args.Length == 1) && StringContains(args[0], new[] {"?", "/?", "/h", "help", "/help"})))
            {
                var exeName = string.Format("{0}.exe", Assembly.GetExecutingAssembly().GetName().Name);
                Console.WriteLine("SingleAPI Service endpoint compiler.");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine();
                Console.WriteLine("{0} [options]", exeName);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine();
                Console.WriteLine("ConfigFile\t service endpoint definition file");
                Console.WriteLine("CheckConfig\t true/false; validates configuration file for anomalies");
                Console.WriteLine("Namespace\t service endpoint namespace");
                Console.WriteLine("ServiceName\t service endpoint class name");
                Console.WriteLine("Target\t\t Source or Library");
                Console.WriteLine("Reference\t referenced assemblies (single or comma separated)");
                Console.WriteLine("Debug\t\t true/false; compile in debug mode");
                Console.WriteLine("OutPath\t\t optional out path, default value – current path");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine();
                Console.WriteLine(@"{0} /config=service.xml /checkConfig=true", exeName);
                Console.WriteLine();
                Console.WriteLine(
                    @"{0} /config=service.xml /target=Library /namespace=SingleApi.ObjectModel /serviceName=SampleService /reference=.\libs\SingleApi.Common.dll",
                    exeName);
                Console.ReadLine();
                Environment.Exit(0);
            }

            Logger.Info("Service endpoint compiler started.");
            Console.WriteLine("Service endpoint compiler started.");

            var cmdParams = new CmdLineParams(args);

            try
            {
                var options = ServiceGeneratorConsoleOptions.Parse(cmdParams);
                var builder = new CodeBuilder(options);
                builder.Run();
            }
            catch (Exception err)
            {
                Console.WriteLine();
                Console.WriteLine(err.Message);
                Logger.Error(err.StackTrace);
                Environment.Exit(-1);
            }
            Logger.Info("Service endpoint compiler finished.");
            Environment.Exit(0);
        }

        //NOTE: .net 3.5 compatibility for mongo
        private static bool StringContains(string str, string[] values)
        {
            return values.Any(value => str.Contains(value));
        }
    }
}