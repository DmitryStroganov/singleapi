using SingleApi.Common;

namespace SingleApi.Server.Loader
{
    internal class LoaderConsoleOptions
    {
        public string Port { get; set; }

        internal static LoaderConsoleOptions Parse(CmdLineParams cmdParams)
        {
            var options = new LoaderConsoleOptions {Port = cmdParams["port"]};

            if (string.IsNullOrEmpty(options.Port))
            {
                options.Port = "8090";
            }

            return options;
        }
    }
}