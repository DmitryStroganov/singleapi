using System;

namespace SingleApi.Common
{
    [Serializable]
    public class ServiceHostControllerEnpointParameters
    {
        public string ServiceName { get; set; }

        public string TargetTypeName { get; set; }

        public string AssemblyName { get; set; }

        internal Type TypeReference { get; set; }

        public long MaxThreads { get; set; }

        public long MaxRetries { get; set; }

        public long MaxFrequency { get; set; }
    }
}