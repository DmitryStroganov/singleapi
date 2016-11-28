using System;
using System.Diagnostics;
using log4net;

namespace SingleApi.Common
{
    /// <summary>Redirect web url to execute command</summary>
    public static class Execute
    {
        #region Fields

        /// <summary>Log channel</summary>
        private static readonly ILog Logger = LogManager.Log;

        #endregion

        #region Public methods

        /// <summary>Execute custom command</summary>
        /// <param name="cmd">Shell command</param>
        /// <param name="param">Command arguments</param>
        public static void Run(string endpointtypename, string cmd, string param)
        {
            try
            {
                var p =
                    Process.Start(
                        new ProcessStartInfo(cmd, param)
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        });
                var errors = p.StandardError.ReadToEnd();
                if (string.IsNullOrEmpty(errors) && ((p.ExitCode == 0) || (p.ExitCode == -1)))
                {
                    Logger.InfoFormat("{0} {1} {2} : OK", endpointtypename, cmd, param);
                }
                else
                {
                    Logger.Error(
                        string.Format(
                            "Error executing: {6} {0} {1}{2}{5}{2}Output:{2}{5}{2}{3}{2}{5}{2}Errors:{2}{5}{2}{4}",
                            cmd,
                            param,
                            Environment.NewLine,
                            p.StandardOutput.ReadToEnd(),
                            errors,
                            "*******",
                            endpointtypename));
                }
            }
            catch (Exception err)
            {
                Logger.Error(string.Format("{0} {1} {2}", endpointtypename, cmd, param), err);
            }
        }

        #endregion
    }
}