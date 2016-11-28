using System;
using System.Globalization;
using System.Reflection;

namespace SingleApi.Common
{
    /// <summary>
    ///     Environment properties getter
    /// </summary>
    public static class EnvProps
    {
        static EnvProps()
        {
            IsCaseSensitive = IsLinux;
            IsMono = IsRunningOnMono();
            MonoVersion = GetMonoVersion();
            if (MonoVersion != null)
            {
                IsSGen = (MonoVersion.Major == 2) && (MonoVersion.Minor > 6);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is case sensitive.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is case sensitive; otherwise, <c>false</c>.
        /// </value>
        public static bool IsCaseSensitive { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is mono.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is mono; otherwise, <c>false</c>.
        /// </value>
        public static bool IsMono { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is S gen.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is S gen; otherwise, <c>false</c>.
        /// </value>
        public static bool IsSGen { get; private set; }

        /// <summary>
        ///     Gets the mono version.
        /// </summary>
        /// <value>
        ///     The mono version.
        /// </value>
        public static Version MonoVersion { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is linux.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is linux; otherwise, <c>false</c>.
        /// </value>
        public static bool IsLinux
        {
            get
            {
                var p = (int) Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        private static Version GetMonoVersion()
        {
            var type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                var dispalayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (dispalayName != null)
                {
                    var monoversionstring = (string) dispalayName.Invoke(null, null);
                    var parts = monoversionstring.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 1)
                    {
                        return null;
                    }

                    var major = 0;
                    int.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out major);
                    var minor = 0;
                    int.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out minor);
                    var revision = 0;
                    if (parts.Length > 2)
                    {
                        if (parts[2].IndexOf(' ') > 0)
                        {
                            revision = int.Parse(parts[2].Substring(0, parts[2].IndexOf(' ')), CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            int.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out revision);
                        }
                    }
                    return major > 0 ? new Version(major, minor, revision) : null;
                }
            }

            return null;
        }
    }
}