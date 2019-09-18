using System;

namespace VersionChanger.DataObjects
{
    internal class Parameter
    {
        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the path of the assembly info file
        /// </summary>
        public string AssemblyInfoFile { get; set; }

        /// <summary>
        /// Gets or sets the format (<see cref="Global.VersionNumberFormat.Long"/> is the default value)
        /// </summary>
        public Global.VersionNumberFormat Format { get; set; } = Global.VersionNumberFormat.Long;

        /// <summary>
        /// Gets or sets the version type
        /// </summary>
        public Global.VersionType VersionType { get; set; } = Global.VersionType.WithDaysAndMinutes;

        /// <summary>
        /// Prints the parameters to the console
        /// </summary>
        public void Print()
        {
            Console.WriteLine("INFO > Parameters:" +
                              $"\r\n\t- Version......: {Version}" +
                              $"\r\n\t- Assembly file: {AssemblyInfoFile}" +
                              $"\r\n\t- Format.......: {Format}" +
                              $"\r\n\t- Version type.: {VersionType}");
        }
    }
}
