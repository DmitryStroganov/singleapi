using System;

namespace SingleApi.Common
{
    [Serializable]
    public class ServiceHostControllerParameters
    {
        public int Port { get; set; }

        public string BaseUri { get; set; }

        public string ControllerName { get; set; }
    }
}