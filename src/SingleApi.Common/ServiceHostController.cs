using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace SingleApi.Common
{
    public class ServiceHostController : MarshalByRefObject
    {
        private Dictionary<Type, ServiceHost> hostList;

        public void Start(ServiceHostControllerParameters controllerParameters, ServiceHostControllerEnpointParameters[] serviceConfig)
        {
            LogManager.Log.Info("Starting ServiceHostController");

            if (string.IsNullOrEmpty(controllerParameters.ControllerName))
            {
                throw new ArgumentNullException("Required parameter ControllerName is missing.");
            }

            if (serviceConfig == null)
            {
                throw new ArgumentNullException("Required parameter serviceConfig is null.");
            }


            hostList = new Dictionary<Type, ServiceHost>();

            foreach (var enpointParameters in serviceConfig)
            {
                var uri = new UriBuilder(controllerParameters.BaseUri);
                uri.Path += enpointParameters.ServiceName;
                uri.Path += "/" + enpointParameters.TargetTypeName;
                uri.Port = controllerParameters.Port;

                if (!hostList.ContainsKey(enpointParameters.TypeReference))
                {
                    var host = new ServiceHost(enpointParameters.TypeReference, uri.Uri);
                    hostList.Add(enpointParameters.TypeReference, host);
                    LogManager.Log.InfoFormat("Loaded ServiceHost: for {0} at {1}", enpointParameters.TargetTypeName, uri.Uri);
                }

                var iface = enpointParameters.TypeReference.GetInterfaces()[0];
                var endpoint = hostList[enpointParameters.TypeReference].AddServiceEndpoint(iface, new WebHttpBinding(), string.Empty);
                var httpBehavior = new WebHttpBehavior();
                endpoint.Behaviors.Add(httpBehavior);

                LogManager.Log.InfoFormat("Added endpoint for the contract: {0}", iface);
            }

            foreach (var host in hostList.Values)
            {
                host.Faulted += host_Faulted;
                host.Open();
            }
        }

        private void host_Faulted(object sender, EventArgs e)
        {
            LogManager.Log.ErrorFormat("ServiceHost '{0}' faulted.", sender);
        }

        ~ServiceHostController()
        {
            if (hostList != null)
            {
                LogManager.Log.Info("Shutting down ServiceHostController");

                foreach (var host in hostList.Values)
                {
                    if ((host.State != CommunicationState.Closed) || (host.State != CommunicationState.Faulted))
                    {
                        host.Close();
                        host.Abort();
                    }
                }

                hostList = null;
            }
        }
    }
}