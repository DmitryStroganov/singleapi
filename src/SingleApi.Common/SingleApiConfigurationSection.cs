using System.Configuration;

namespace SingleApi.Common.Cofiguration
{
    public class SingleApiConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("EndpointConfigurations")]
        public EndpointConfigurations EndpointConfigurations
        {
            get { return base["EndpointConfigurations"] as EndpointConfigurations; }
        }

        [ConfigurationProperty("EndpointsFolder", IsRequired = false)]
        public string EndpointsFolder
        {
            get { return (string) this["EndpointsFolder"]; }
            set { this["EndpointsFolder"] = value; }
        }

        [ConfigurationProperty("PluginsFolder", IsRequired = false)]
        public string PluginsFolder
        {
            get { return (string) this["PluginsFolder"]; }
            set { this["PluginsFolder"] = value; }
        }

        public static SingleApiConfigurationSection GetSettings(string configFilePath, string sectionName)
        {
            var configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFilePath;

            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            return config.GetSection(sectionName) as SingleApiConfigurationSection;
        }
    }

    [ConfigurationCollection(typeof(EndpointConfiguration), AddItemName = "EndpointConfiguration")]
    public class EndpointConfigurations : ConfigurationElementCollection
    {
        public EndpointConfiguration this[int idx]
        {
            get { return BaseGet(idx) as EndpointConfiguration; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new EndpointConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as EndpointConfiguration).ServiceName;
        }
    }

    public class EndpointConfiguration : ConfigurationElement
    {
        #region Public properties

        [ConfigurationProperty("assembly", IsRequired = true)]
        public string Assembly
        {
            get { return (string) this["assembly"]; }
            set { this["assembly"] = value; }
        }

        [ConfigurationProperty("servicename", IsRequired = true)]
        public string ServiceName
        {
            get { return (string) this["servicename"]; }
            set { this["servicename"] = value; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return (string) this["type"]; }
            set { this["type"] = value; }
        }

        #endregion
    }
}