using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleApi.Common
{
    [Serializable]
    public sealed class ServiceHostControllerDomainManager : DomainManager<ServiceHostController>
    {
        public ServiceHostControllerDomainManager(ServiceHostControllerParameters parameters, ServiceHostControllerEnpointParameters[] serviceConfig)
            : base(parameters.ControllerName)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("ServiceHostControllerParameters");
            }

            if (string.IsNullOrEmpty(parameters.ControllerName))
            {
                throw new ArgumentException("Required parameter ControllerName is missing.");
            }

            if (serviceConfig == null)
            {
                throw new ArgumentNullException("serviceConfig");
            }


            Domain.DoCallBack(LogManager.Congifure);
            Domain.DoCallBack(LogManager.SetupUnhandledExceptionLogging);

            var typeList = new List<ServiceHostControllerEnpointParameters>();
            foreach (var enpointParameters in serviceConfig)
            {
                try
                {
                    var obj = Domain.CreateInstanceFromAndUnwrap(enpointParameters.AssemblyName, enpointParameters.TargetTypeName);
                    enpointParameters.TypeReference = obj.GetType();
                    typeList.Add(enpointParameters);
                }
                catch (Exception)
                {
                    LogManager.Log.ErrorFormat("Unable to load endpoint: '{0}' from assembly '{1}'",
                        enpointParameters.TargetTypeName,
                        enpointParameters.AssemblyName);
                }
            }

            if (typeList.Count < 1)
            {
                LogManager.Log.Error("Failed to load servicehost.");
                return;
            }

            Value.Start(parameters, typeList.ToArray());

            LogManager.Log.InfoFormat("Started ServiceHostController for: '{0}'", string.Join(", ", typeList.Select(e => e.TargetTypeName).ToArray()));
        }
    }
}