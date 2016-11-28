using System;

namespace SingleApi.Common
{
    internal class EndpointTask
    {
        public Type EndpointType { get; set; }
        public string Command { get; set; }
        public string Parameters { get; set; }
    }
}