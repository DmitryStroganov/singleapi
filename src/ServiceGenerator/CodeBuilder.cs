using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using ConsoleLogger = Microsoft.Build.Logging.ConsoleLogger;

namespace ServiceGenerator
{
    /// <summary>
    ///     Builds code from configuration
    /// </summary>
    internal class CodeBuilder
    {
        #region Constructors

        public CodeBuilder(ServiceGeneratorConsoleOptions serviceGeneratorConsoleOptions)
        {
            this.serviceGeneratorConsoleOptions = serviceGeneratorConsoleOptions;
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Code builder
        /// </summary>
        public void Run()
        {
            if (serviceGeneratorConsoleOptions == null)
            {
                throw new ArgumentNullException("Configuration is not initialized.");
            }

            if (string.IsNullOrEmpty(serviceGeneratorConsoleOptions.ConfigFile))
            {
                throw new ArgumentNullException("Empty configFile not allowed!");
            }

            var isSimplePath = serviceGeneratorConsoleOptions.ConfigFile.Contains(";") == false;
            if (!serviceGeneratorConsoleOptions.CheckConfig)
            {
                Console.WriteLine("(BuildMode)");
                if (isSimplePath)
                {
                    LoadConfig(serviceGeneratorConsoleOptions.ConfigFile, BuildService);
                }
                else
                {
                    throw new ArgumentException("Multiple configFiles  are not allowed for build mode.");
                }
            }
            else
            {
                Console.WriteLine("(CheckMode)");

                var files = serviceGeneratorConsoleOptions.ConfigFile.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                if (files.Length == 0)
                {
                    return;
                }

                var endpoints = new List<ConfigInfo>();

                for (int i = 0, c = files.Length; i < c; i++)
                {
                    var item = files[i];

                    if (!File.Exists(item))
                    {
                        throw new Exception(string.Format("File:{0} does not exist!", item));
                    }

                    try
                    {
                        LoadConfig(item, obj => endpoints.Add(new ConfigInfo {FileName = item, Config = obj}));
                    }
                    catch (Exception err)
                    {
                        throw new Exception(
                            string.Format(
                                "Check file:{1} error!{0}Message:{2}{0}Stack:{3}",
                                Environment.NewLine,
                                item,
                                err.Message,
                                err.StackTrace));
                    }
                }
                var findError = CheckConfig(endpoints.ToArray());

                if (findError)
                {
                    throw new Exception("Configuration file contains errors.");
                }

                Console.WriteLine("Configuration check OK.");
            }
        }

        #endregion

        private class ConfigInfo
        {
            /// <summary>
            ///     Gets or sets the name of the file.
            /// </summary>
            /// <value>
            ///     The name of the file.
            /// </value>
            public string FileName { get; set; }

            /// <summary>
            ///     Gets or sets the configuration reference.
            /// </summary>
            /// <value>
            ///     The config.
            /// </value>
            public EndpointModuleConfiguration Config { get; set; }
        }

        #region Fields

        private static readonly Encoding TargetEncoding = Encoding.UTF8;
        private readonly ServiceGeneratorConsoleOptions serviceGeneratorConsoleOptions;

        #endregion

        #region Private methods

        /// <summary>
        ///     Checks configuration(s) for anomalies
        /// </summary>
        /// <param name="configs">Array of loaded configs</param>
        /// <returns>True if any errors</returns>
        private bool CheckConfig(ConfigInfo[] configs)
        {
            var result = false;
            //Console.WriteLine("Self file check...");
            // for first step find equals in self file
            foreach (var origin in configs.Where(origin => CheckMethods(origin, origin)))
            {
                result = true;
            }

            //Console.Write("Compare file check...");
            // for second step find equals in compared files
            for (var i = 0; i < configs.Length - 1; i++)
            {
                var origin = configs[i];

                for (var j = i + 1; j < configs.Length; j++)
                {
                    var compare = configs[j];

                    if (CheckMethods(origin, compare))
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Checks methods in two configs
        /// </summary>
        /// <param name="config1">First config</param>
        /// <param name="config2">Second config</param>
        /// <returns>True if any errors</returns>
        private bool CheckMethods(ConfigInfo config1, ConfigInfo config2)
        {
            var result = false;
            var cfg1 = config1.Config;
            var cfg2 = config2.Config;

            if ((cfg1.Method == null) || (cfg1.Method.Length == 0))
            {
                return result;
            }
            if ((cfg2.Method == null) || (cfg2.Method.Length == 0))
            {
                return result;
            }

            var selfFileCheck = config1.Config == config2.Config;
            var count1 = selfFileCheck ? cfg1.Method.Length - 1 : cfg1.Method.Length;

            for (var i = 0; i < count1; i++)
            {
                var meth1 = cfg1.Method[i];
                for (var j = selfFileCheck ? i + 1 : 0; j < cfg2.Method.Length; j++)
                {
                    var meth2 = cfg2.Method[j];

                    if (meth1.Name == meth2.Name)
                    {
                        if (CheckMethodSignatures(config1, config2, meth1, meth2))
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Compares method signatures (Variables order, name and type)
        /// </summary>
        /// <param name="cfg1">Config for method1</param>
        /// <param name="cfg2">Config for method2</param>
        /// <param name="meth1">Method first</param>
        /// <param name="meth2">Method second</param>
        /// <returns>True if any errors</returns>
        private bool CheckMethodSignatures(
            ConfigInfo cfg1,
            ConfigInfo cfg2,
            EndpointModuleConfiguration.EndpointModuleConfigurationMethod meth1,
            EndpointModuleConfiguration.EndpointModuleConfigurationMethod meth2)
        {
            var result = false;

            Action echo;
            echo = () =>
            {
                if (cfg1.Config == cfg2.Config)
                {
                    Console.WriteLine(
                        "{0}Error: Duplicate method '{1}' in file:{0}'{2}'",
                        Environment.NewLine,
                        meth1.Name,
                        cfg1.FileName);
                }
                else
                {
                    Console.WriteLine(
                        "{0}Error: Duplicate method '{1}' in files:{0}'{2}'{0}'{3}'",
                        Environment.NewLine,
                        meth1.Name,
                        cfg1.FileName,
                        cfg2.FileName);
                }
                result = true;
            };

            var var1 = (meth1.Variables == null) || (meth1.Variables.Length == 0) ? 0 : meth1.Variables.Length;
            var var2 = (meth2.Variables == null) || (meth2.Variables.Length == 0) ? 0 : meth2.Variables.Length;

            if (var1 == var2)
            {
                // find 2 equal empty variables method 
                if (var1 == 0)
                {
                    echo();
                }
                else
                {
                    // signature must be safe order of variables
                    var equals = true;
                    for (var i = 0; i < meth1.Variables.Length; i++)
                    {
                        var variable1 = meth1.Variables[i];
                        var variable2 = meth2.Variables[i];

                        if (((variable1.Name == variable2.Name) && (variable1.Type == variable2.Type)) == false)
                        {
                            // found difference, exit from loop
                            equals = false;
                            break;
                        }
                    }
                    if (equals)
                    {
                        echo();
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Loads config and performs a callback with loaded endpoint
        /// </summary>
        /// <param name="action">Action for callback</param>
        private void LoadConfig(string fileName, Action<EndpointModuleConfiguration> action)
        {
            var setting = new XmlReaderSettings {CloseInput = true};
            setting.ValidationFlags = setting.ValidationFlags | XmlSchemaValidationFlags.ReportValidationWarnings;
            setting.ValidationType = ValidationType.Schema;

            //setting.Schemas.Add(XmlSchema.Read(reader, this.ValidationCallBack));
            //setting.ValidationEventHandler += this.ValidationCallBack;
            using (var xmlReader = XmlReader.Create(fileName, setting))
            {
                var serializer = new XmlSerializer(typeof(EndpointModuleConfiguration));
                var obj = serializer.Deserialize(xmlReader) as EndpointModuleConfiguration;

                if (action != null)
                {
                    action(obj);
                }
            }
        }

        /// <summary>
        ///     Builds code from endpointModuleConfiguration
        /// </summary>
        /// <param name="endpointModuleConfiguration"></param>
        private void BuildService(EndpointModuleConfiguration endpointModuleConfiguration)
        {
            var cuImplementation = new CodeCompileUnit();
            var cuInterface = new CodeCompileUnit();

            var defaultNs = new CodeNamespace(serviceGeneratorConsoleOptions.Namespace);
            var defaultNsIface = new CodeNamespace(serviceGeneratorConsoleOptions.Namespace);

            cuImplementation.Namespaces.Add(defaultNs);
            cuInterface.Namespaces.Add(defaultNsIface);

            var name = Path.GetFileNameWithoutExtension(serviceGeneratorConsoleOptions.ServiceName);
            if (name.Contains("."))
            {
                throw new Exception("Char '.' is not allowed in Service Class name.");
            }
            var classDef = new CodeTypeDeclaration(name);
            var classDefIface = new CodeTypeDeclaration(string.Format("I{0}", name));

            //classDef.Sumary("SingleService Service");
            classDef.Attributes = MemberAttributes.Public;
            classDef.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializableAttribute))));
            classDef.BaseTypes.Add(new CodeTypeReference(classDefIface.Name));

            //classDefIface.Sumary("ISingleService Service");
            classDefIface.Attributes = MemberAttributes.Public;
            classDefIface.IsInterface = true;
            classDefIface.CustomAttributes.Add(
                new CodeAttributeDeclaration(new CodeTypeReference(typeof(ServiceContractAttribute))));

            var import = new[]
            {
                "System",
                "System.Collections.Generic",
                "System.ServiceModel",
                "System.ServiceModel.Web"
            };

            for (int i = 0, c = import.Length; i < c; i++)
            {
                var item = import[i];
                defaultNs.Imports.Add(new CodeNamespaceImport(item));
                defaultNsIface.Imports.Add(new CodeNamespaceImport(item));
            }

            defaultNs.Types.Add(classDef);
            defaultNsIface.Types.Add(classDefIface);

            foreach (var method in endpointModuleConfiguration.Method)
            {
                var methDef = new CodeMemberMethod {Name = method.Name, Attributes = MemberAttributes.Public};
                var methDefIface = new CodeMemberMethod {Name = method.Name};
                var expr = new List<CodeExpression>();
                var methUri = method.Name;

                var arguments = method.Shell.Arguments;

                if ((method.Variables != null) && (method.Variables.Length > 0))
                {
                    var n = 0;

                    foreach (var variable in method.Variables)
                    {
                        if (n == 0)
                        {
                            methUri += "?";
                        }

                        if (n > 0)
                        {
                            methUri += "&";
                        }

                        methUri += string.Format("{0}=", variable.Name) + "{" + variable.Name + "}";
                        methDefIface.Parameters.Add(new CodeParameterDeclarationExpression(variable.Type, variable.Name));
                        methDef.Parameters.Add(new CodeParameterDeclarationExpression(variable.Type, variable.Name));
                        arguments = arguments.Replace("{" + variable.Name + "}", "{" + n + "}");
                        expr.Add(new CodeVariableReferenceExpression(variable.Name));
                        n++;
                    }
                }

                methDefIface.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(OperationContractAttribute))));
                methDefIface.CustomAttributes.Add(
                    new CodeAttributeDeclaration(
                        new CodeTypeReference(typeof(WebGetAttribute)),
                        new CodeAttributeArgument("UriTemplate", new CodePrimitiveExpression(methUri)),
                        new CodeAttributeArgument("BodyStyle", new CodeFieldReferenceExpression(null, "WebMessageBodyStyle.WrappedRequest")),
                        new CodeAttributeArgument("RequestFormat", new CodeFieldReferenceExpression(null, "WebMessageFormat.Json")),
                        new CodeAttributeArgument("ResponseFormat", new CodeFieldReferenceExpression(null, "WebMessageFormat.Json"))));

                classDef.Members.Add(methDef);
                classDefIface.Members.Add(methDefIface);
                methDef.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(string)), "arguments"));

                if ((expr.Count == 0) && !string.IsNullOrEmpty(method.Shell.Arguments))
                {
                    methDef.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("arguments"),
                        new CodePrimitiveExpression(method.Shell.Arguments)));
                }
                else if ((expr.Count == 0) && string.IsNullOrEmpty(method.Shell.Arguments))
                {
                    methDef.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("arguments"), new CodePrimitiveExpression("")));
                }
                else
                {
                    expr.Insert(0, new CodePrimitiveExpression(arguments));
                    methDef.Statements.Add(
                        new CodeAssignStatement(
                            new CodeVariableReferenceExpression("arguments"),
                            new CodeMethodInvokeExpression(null, "String.Format", expr.ToArray())));
                }

                methDef.Statements.Add(
                    new CodeMethodInvokeExpression(
                        null,
                        "SingleApi.Common.Execute.Run",
                        new CodePrimitiveExpression(string.Format("{0}.{1}", defaultNs.Name, classDef.Name)),
                        new CodePrimitiveExpression(method.Shell.Command),
                        new CodeVariableReferenceExpression("arguments")));
            }

            Compile(cuImplementation, cuInterface);
        }

        /// <summary>
        ///     Code gen from source code graph
        /// </summary>
        /// <param name="compileunit">Graf</param>
        /// <param name="outFile">File</param>
        private void BuildOut(CodeCompileUnit compileunit, string outFile)
        {
            var sourceFile = outFile;
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var writer = new IndentedTextWriter(new StreamWriter(sourceFile, false, TargetEncoding), "    ");
            provider.GenerateCodeFromCompileUnit(compileunit, writer, new CodeGeneratorOptions());
            writer.Close();
        }

        /// <summary>
        ///     Compiles CodeDom to Dll
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        private void Compile(CodeCompileUnit service, CodeCompileUnit iface)
        {
            if (serviceGeneratorConsoleOptions == null)
            {
                throw new ArgumentNullException("serviceGeneratorConsoleOptions");
            }

            var projectOutFolder = Path.GetTempPath();
            var projectName = serviceGeneratorConsoleOptions.ServiceName;
            projectName = Path.GetFileNameWithoutExtension(projectName);
            projectOutFolder = Path.Combine(projectOutFolder, projectName);

            if (Directory.Exists(projectOutFolder) == false)
            {
                Directory.CreateDirectory(projectOutFolder);
            }

            var platform = "AnyCPU";
            var platformTarget = "AnyCPU";
            var projFile = Path.Combine(projectOutFolder, string.Format("{0}.csproj", projectName));
            //var docFile = Path.Combine(projectOutFolder,  string.Format("{0}.xml", projectName));

            var outputAssemblyName = string.Format("{0}.{1}", serviceGeneratorConsoleOptions.Namespace, projectName);

            Uri startUri;
            Uri.TryCreate(Path.Combine(projectOutFolder, projFile), UriKind.Absolute, out startUri);
            Console.WriteLine("Build: {0} ... ", projFile);

            var engine = new Engine {DefaultToolsVersion = "3.5"};
            engine.RegisterLogger(new ConsoleLogger(LoggerVerbosity.Minimal));

            var proj = new Project(engine);
            proj.DefaultTargets = "Build";
            proj.DefaultToolsVersion = "3.5";

            proj.SetProperty("Configuration", "Debug");
            proj.SetProperty("Platform", platform);
            proj.SetProperty("ProductVersion", "8.0.30703");
            proj.SetProperty("SchemaVersion", "2.0");
            proj.SetProperty("ProjectGuid", Guid.NewGuid().ToString());
            proj.SetProperty("OutputType", "Library");
            proj.SetProperty("RootNamespace", serviceGeneratorConsoleOptions.Namespace);
            proj.SetProperty("AssemblyName", outputAssemblyName);
            proj.SetProperty("TargetFrameworkVersion", "v3.5");
            proj.SetProperty("SignAssembly", "false");

            BuildPropertyGroup group;

            if (serviceGeneratorConsoleOptions.Debug)
            {
                group = proj.AddNewPropertyGroup(false);
                group.AddNewProperty("OutputPath", projectOutFolder);
                group.AddNewProperty("PlatformTarget", platformTarget);
                group.AddNewProperty("DebugSymbols", "true");
                group.AddNewProperty("Optimize", "false");
                group.AddNewProperty("DefineConstants", "TRACE;DEBUG;");
                group.AddNewProperty("ErrorReport", "prompt");
                group.AddNewProperty("WarningLevel", "4");
                //group.AddNewProperty("DocumentationFile", docFile);
            }
            else
            {
                group = proj.AddNewPropertyGroup(false);
                group.AddNewProperty("OutputPath", projectOutFolder);
                group.AddNewProperty("PlatformTarget", platformTarget);
                group.AddNewProperty("DebugType", "pdbonly");
                group.AddNewProperty("Optimize", "true");
                group.AddNewProperty("DefineConstants", "TRACE");
                group.AddNewProperty("ErrorReport", "prompt");
                group.AddNewProperty("WarningLevel", "4");
                //group.AddNewProperty("DocumentationFile", docFile);
            }

            proj.AddNewItem("Reference", "System");
            proj.AddNewItem("Reference", "System.Runtime.Serialization");
            proj.AddNewItem("Reference", "System.ServiceModel");
            proj.AddNewItem("Reference", "System.ServiceModel.Web");
            proj.AddNewItem("Reference", "System.Xml");

            var references = serviceGeneratorConsoleOptions.Reference.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0, c = references.Length; i < c; i++)
            {
                var item = Path.GetFileName(references[i]);
                var src = references[i];

                if (!File.Exists(references[i]))
                {
                    src = Path.Combine(Environment.CurrentDirectory, item);
                    if (!File.Exists(src))
                    {
                        throw new ArgumentException(string.Format("Specified reference assembly not found: {0}", references[i]));
                    }
                }

                var dst = Path.Combine(projectOutFolder, item);

                File.Copy(src, dst, true);

                if (File.Exists(dst))
                {
                    Uri uri;
                    Uri.TryCreate(dst, UriKind.Absolute, out uri);
                    var rel = startUri.MakeRelativeUri(uri);
                    dst = Uri.UnescapeDataString(rel.ToString());
                    dst = dst.Replace("/", "\\");

                    if (dst.StartsWith("\\"))
                    {
                        dst = dst.Substring(1);
                    }

                    if (string.IsNullOrEmpty(dst))
                    {
                        continue;
                    }

                    var newItem = proj.AddNewItem("Reference", Path.GetFileNameWithoutExtension(dst));
                    newItem.SetMetadata("SpecificVersion", "false");
                    newItem.SetMetadata("HintPath", dst);
                }
            }

            var outInterface = Path.Combine(projectOutFolder, string.Format("I{0}.cs", projectName));
            var outService = Path.Combine(projectOutFolder, string.Format("{0}.cs", projectName));
            var fileList = new[] {outInterface, outService};

            for (int i = 0, c = fileList.Length; i < c; i++)
            {
                var path = fileList[i];
                Uri uri;
                Uri.TryCreate(path, UriKind.Absolute, out uri);
                var rel = startUri.MakeRelativeUri(uri);
                path = Uri.UnescapeDataString(rel.ToString());
                path = path.Replace("/", "\\");

                if (path.StartsWith("\\"))
                {
                    path = path.Substring(1);
                }

                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                proj.AddNewItem("Compile", path);
            }

            proj.AddNewImport(@"$(MSBuildToolsPath)\Microsoft.CSharp.targets", string.Empty);
            proj.Save(projFile);

            BuildOut(service, outService);
            BuildOut(iface, outInterface);

            switch (serviceGeneratorConsoleOptions.Target)
            {
                case ServiceGeneratorConsoleOptions.ServiceGeneratorTarget.Source:
                {
                    for (int i = 0, c = fileList.Length; i < c; i++)
                    {
                        var src = Path.Combine(projectOutFolder, fileList[i]);
                        var dst = Path.Combine(serviceGeneratorConsoleOptions.OutPath, Path.GetFileName(fileList[i]));
                        File.Copy(src, dst, true);
                    }
                    Directory.Delete(projectOutFolder, true);
                }
                    break;
                case ServiceGeneratorConsoleOptions.ServiceGeneratorTarget.Library:
                {
                    var result = proj.Build();

                    if (!result)
                    {
                        Console.WriteLine("Error in project:{0}", projectOutFolder);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        var src = Path.Combine(projectOutFolder, outputAssemblyName + ".dll");
                        var dst = Path.Combine(serviceGeneratorConsoleOptions.OutPath, outputAssemblyName + ".dll");
                        File.Copy(src, dst, true);
                        Directory.Delete(projectOutFolder, true);
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.WriteLine("Build for target '{0}' Done.", serviceGeneratorConsoleOptions.Target);
        }

        #endregion
    }
}