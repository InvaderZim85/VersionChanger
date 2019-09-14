using System;

namespace VersionChanger.DataObjects
{
    public class Parameter
    {
        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the path of the assembly info file
        /// </summary>
        public string AssemblyInfoFile { get; set; }
    }
}
