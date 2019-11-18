using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VersionChanger.DataObjects
{
    internal class Parameter
    {
        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the list with the assembly info files
        /// </summary>
        public List<string> AssemblyInfoFiles { get; set; }

        /// <summary>
        /// Gets or sets the format (<see cref="Global.VersionNumberFormat.None"/> is the default value)
        /// </summary>
        public Global.VersionNumberFormat Format { get; set; } = Global.VersionNumberFormat.None;

        /// <summary>
        /// Gets or sets the version type (<see cref="Global.VersionType.None"/> is the default value)
        /// </summary>
        public Global.VersionType VersionType { get; set; } = Global.VersionType.None;

        /// <summary>
        /// Prints the parameters to the console
        /// </summary>
        public void Print()
        {
            Console.WriteLine("INFO > Parameters:" +
                              $"\r\n\t- Version.........: {Version}" +
                              $"\r\n\t- Assembly file(s): {string.Join(", ", AssemblyInfoFiles.Select(Path.GetFileName))}" +
                              $"\r\n\t- Format..........: {Format}" +
                              $"\r\n\t- Version type....: {VersionType}");
        }
    }
}
