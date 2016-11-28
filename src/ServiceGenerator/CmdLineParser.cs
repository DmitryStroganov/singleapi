// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CmdLineParser.cs" company="">
// Original code from "CommandLine.Utility" (C) GriffonRL
// http://www.codeproject.com/Articles/3111/C-NET-Command-Line-Arguments-Parser
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServiceGenerator
{
    internal class CmdLineParams
    {
        // Fields
        private readonly StringDictionary Parameters = new StringDictionary();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CmdLineParams" /> class.
        /// </summary>
        /// <param name="Args">
        ///     The args.
        /// </param>
        public CmdLineParams(string[] Args)
        {
            var regex = new Regex("^-{1,2}|^/|=|:", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var regex2 = new Regex("^['\"]?(.*?)['\"]?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string key = null;

            foreach (var strArray in Args.Select(str2 => regex.Split(str2, 3)))
            {
                switch (strArray.Length)
                {
                    case 1:
                        if (key != null)
                        {
                            if (!Parameters.ContainsKey(key))
                            {
                                strArray[0] = regex2.Replace(strArray[0], "$1");
                                Parameters.Add(key, strArray[0]);
                            }
                            key = null;
                        }
                        break;

                    case 2:
                        if ((key != null) && !Parameters.ContainsKey(key))
                        {
                            Parameters.Add(key, "true");
                        }
                        key = strArray[1];
                        break;

                    case 3:
                        if ((key != null) && !Parameters.ContainsKey(key))
                        {
                            Parameters.Add(key, "true");
                        }
                        key = strArray[1];
                        if (!Parameters.ContainsKey(key))
                        {
                            strArray[2] = regex2.Replace(strArray[2], "$1");
                            Parameters.Add(key, strArray[2]);
                        }
                        key = null;
                        break;
                }
            }

            if ((key != null) && !Parameters.ContainsKey(key))
            {
                Parameters.Add(key, "true");
            }
        }

        public string this[string Param]
        {
            get { return Parameters[Param]; }
        }
    }
}