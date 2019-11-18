using System.Collections.Generic;
using CommandLine;

namespace VersionChanger.DataObjects
{
    /// <summary>
    /// Represents the command line arguments
    /// </summary>
    internal sealed class Arguments
    {
        /// <summary>
        /// Gets or sets the version
        /// </summary>
        [Option('v', Required = false, HelpText = "The version number")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the list with the assembly files
        /// </summary>
        [Option('p', "path", Required = false, HelpText = "The path of the assembly files.")]
        public IEnumerable<string> AssemblyFiles { get; set; }

        /// <summary>
        /// Gets or sets the format of the version number (default = 1)
        /// </summary>
        [Option('f', "format", Required = false, HelpText = "The format type of the version number.")]
        public int Format { get; set; }

        /// <summary>
        /// Gets or sets the type of the version number (default = 1)
        /// </summary>
        [Option('t', "type", Required = false, HelpText = "The type of the version number.")]
        public int VersionType { get; set; }
    }
}
