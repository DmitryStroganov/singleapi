using System;

namespace SingleApi.Common
{
    public static class Tools
    {
        #region Private methods

        private static void Nop()
        {
        }

        #endregion

        #region Public methods

        /// <summary>Builds the URI.</summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <returns>String view as http://host:port</returns>
        public static string BuildUri(string host, int port)
        {
            return string.Format("http://{0}:{1}", host, port);
        }

        /// <summary>
        ///     Supress exception for the action
        /// </summary>
        /// <param name="action">The action.</param>
        public static void NoThrow(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                Nop();
            }
        }

        #endregion
    }
}