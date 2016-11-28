using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace SingleApi.Common
{
    public class CmdLineParams
    {
        // Fields
        private readonly StringDictionary Parameters = new StringDictionary();

        // Methods
        public CmdLineParams(string[] Args)
        {
            var regex = new Regex("^-{1,2}|^/|=|:", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var regex2 = new Regex("^['\"]?(.*?)['\"]?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string key = null;
            foreach (var str2 in Args)
            {
                var strArray = regex.Split(str2, 3);
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

        // Properties
        public string this[string Param]
        {
            get { return Parameters[Param]; }
        }
    }
}