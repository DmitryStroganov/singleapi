using System;
using System.IO;

namespace ServiceGenerator
{
    internal class ServiceGeneratorConsoleOptions
    {
        public bool CheckConfig { get; set; }
        public string ConfigFile { get; set; }
        public string Namespace { get; set; }
        public string ServiceName { get; set; }
        public ServiceGeneratorTarget Target { get; set; }

        public string Reference { get; set; }
        public bool Debug { get; set; }
        public string OutPath { get; set; }

        internal static ServiceGeneratorConsoleOptions Parse(CmdLineParams cmdParams)
        {
            var options = new ServiceGeneratorConsoleOptions
            {
                ConfigFile = cmdParams["config"],
                Namespace = cmdParams["namespace"],
                ServiceName = cmdParams["serviceName"],
                Reference = cmdParams["reference"],
                OutPath = cmdParams["outPath"]
            };

            if (string.IsNullOrEmpty(options.ConfigFile))
            {
                throw new ArgumentNullException("Required ConfigFile parameter is missing.");
            }

            if (!string.IsNullOrEmpty(cmdParams["checkConfig"]))
            {
                var flag = false;
                bool.TryParse(cmdParams["checkConfig"], out flag);
                options.CheckConfig = flag;
            }

            options.Target = TryParseEnum(cmdParams["target"], ServiceGeneratorTarget.Library);

            //guess namespace from config file name
            if (string.IsNullOrEmpty(options.Namespace))
            {
                options.Namespace = Path.GetFileNameWithoutExtension(options.ConfigFile);
            }

            if (!string.IsNullOrEmpty(cmdParams["debug"]))
            {
                var flag = true;
                bool.TryParse(cmdParams["debug"], out flag);
                options.Debug = flag;
            }
            else
            {
                options.Debug = true;
            }

            if (string.IsNullOrEmpty(options.OutPath))
            {
                options.OutPath = Environment.CurrentDirectory;
            }

            return options;
        }

        public static TEnum TryParseEnum<TEnum>(string strEnumValue, TEnum defaultValue)
        {
            if (string.IsNullOrEmpty(strEnumValue) || !Enum.IsDefined(typeof(TEnum), strEnumValue))
            {
                return defaultValue;
            }

            return (TEnum) Enum.Parse(typeof(TEnum), strEnumValue);
        }

        internal enum ServiceGeneratorTarget
        {
            Source,
            Library
        }
    }
}