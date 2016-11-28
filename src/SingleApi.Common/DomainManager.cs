using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace SingleApi.Common
{
    /// <summary>
    ///     App Domain Manager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class DomainManager<T> : IDisposable
        where T : MarshalByRefObject
    {
        #region Constructors

        public DomainManager(string friendlyName)
        {
            var setup = AppDomain.CurrentDomain.SetupInformation;
            var permissionSet = new PermissionSet(PermissionState.Unrestricted);
            permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            Domain = AppDomain.CreateDomain(friendlyName, AppDomain.CurrentDomain.Evidence, setup, permissionSet);

            LogManager.Log.Info("Started AppDomain");

            //this._domain.DoCallBack(log4net.Config.XmlConfigurator.Configure);

            Domain.AssemblyResolve += (sender, args) =>
            {
                LogManager.Log.Info("AssemblyResolve");

                if (SourcePath != null)
                {
                    var shortName = args.Name.Split(',')[0].Trim();
                    var possiblePathToAssembly = Path.Combine(SourcePath, shortName + ".dll");
                    if (File.Exists(possiblePathToAssembly))
                    {
                        return Assembly.LoadFile(possiblePathToAssembly);
                    }
                }
                return null;
            };

            //this._domain.AssemblyLoad += (sender, args) =>
            //  {
            //    var e = 0;
            //  };

            var type = typeof(T);

            Value = (T) Domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (Domain != null)
            {
                Domain = null;
            }
        }

        #endregion

        #region Public methods

        public Assembly LoadAssembly(string assemblyPath)
        {
            return Assembly.LoadFrom(assemblyPath);
        }

        #endregion

        ~DomainManager()
        {
            if ((Domain != null) && !Domain.IsFinalizingForUnload())
            {
                LogManager.Log.Info("Unloading AppDomain");
                Tools.NoThrow(() => AppDomain.Unload(AppDomain.CurrentDomain));
            }
        }

        public void ShutDown()
        {
            LogManager.Log.Info("Shutting down AppDomain");
            Dispose();
        }

        #region Fields

        #endregion

        #region Public properties

        public string SourcePath { get; set; }

        public T Value { get; }

        public AppDomain Domain { get; private set; }

        #endregion
    }
}