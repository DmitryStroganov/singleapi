using System.Xml.Serialization;

namespace ServiceGenerator
{
    [XmlRoot("EndpointModuleConfiguration")]
    public class EndpointModuleConfiguration
    {
        [XmlElement("Method")]
        public EndpointModuleConfigurationMethod[] Method { get; set; }

        public class EndpointModuleConfigurationMethod
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlArray("Variables")]
            [XmlArrayItem("Variable")]
            public EndpointModuleConfigurationVariable[] Variables { get; set; }

            [XmlElement("Shell")]
            public EndpointModuleConfigurationShell Shell { get; set; }
        }

        [XmlRoot("Variable")]
        public class EndpointModuleConfigurationVariable
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public string Type { get; set; }
        }

        public class EndpointModuleConfigurationShell
        {
            [XmlAttribute]
            public string Command { get; set; }

            [XmlAttribute]
            public string Arguments { get; set; }
        }
    }
}