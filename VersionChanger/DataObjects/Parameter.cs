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
    }
}
