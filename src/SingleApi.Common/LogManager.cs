using System;
using System.Diagnostics;
using System.Threading;
using log4net;
using log4net.Config;

namespace SingleApi.Common
{
    public static class LogManager
    {
        #region Fields

        private static ILog log;

        #endregion

        #region Public properties

        public static ILog Log
        {
            get
            {
                GlobalContext.Properties["ExecutionContext"] = AppDomain.CurrentDomain.FriendlyName;
                GlobalContext.Properties["ManagedThreadId"] = Thread.CurrentThread.ManagedThreadId;

                var stack = new StackTrace();
                return log ?? (log = log4net.LogManager.GetLogger(stack.GetFrame(stack.FrameCount - 1).GetMethod().DeclaringType));
                //return log ?? (log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType));
            }
        }

        public static ILog GetLogger(Type type)
        {
            return GetLogger(type);
        }

        public static void Congifure()
        {
            XmlConfigurator.Configure();
        }

        public static void SetupUnhandledExceptionLogging()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                Log.ErrorFormat("{0}Unhandled Exception:{1}{0}{2}", Environment.NewLine, ex.Message, ex.StackTrace);
            };
        }

        #endregion
    }
}