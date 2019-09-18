using System;
using System.IO;

namespace VersionChanger
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point of the program
        /// </summary>
        /// <param name="args">The shipped arguments</param>
        private static void Main(string[] args)
        {
            var generatedVersion = false;
            var parameters = Global.LoadParameter(args);

            parameters?.Print();

            var givenVersion = parameters?.Version;
            var versionFormat = parameters?.Format ?? Global.VersionNumberFormat.Long;
            var versionType = parameters?.VersionType ?? Global.VersionType.WithDaysAndMinutes;

            if (givenVersion == null)
            {
                Console.WriteLine("INFO > No version number was specified. Number is generated");
                givenVersion = Global.CreateVersion(versionFormat, versionType);
                generatedVersion = true;
            }

            try
            {
                string assemblyFile;
                if (string.IsNullOrEmpty(parameters?.AssemblyInfoFile))
                {
                    Console.WriteLine("INFO > No assembly file specified. Determine path of the local assembly file.");
                    // Get the path of the assembly file
                    assemblyFile = FileHelper.GetFile("AssemblyInfo", pattern: "*.cs");
                    if (string.IsNullOrEmpty(assemblyFile))
                    {
                        Console.Error.WriteLine("ERROR > Path of the assembly info file could not be determined.");
                        return;
                    }
                }
                else
                {
                    assemblyFile = parameters.AssemblyInfoFile;
                    if (!File.Exists(assemblyFile))
                    {
                        Console.Error.WriteLine("ERROR > Path of the assembly info file could not be determined.");
                        return;
                    }
                }

                Console.WriteLine($"INFO > Assembly file: {assemblyFile}");

                // Get the current version
                var currentVersion = FileHelper.GetCurrentVersion(assemblyFile);

                Version version;
                if (generatedVersion)
                {
                    version = CompareVersions(currentVersion, givenVersion);
                }
                else
                {
                    version = givenVersion;
                }

                var versionString = Global.GetVersionString(version, versionFormat);
                Console.WriteLine("INFO > Version numbers:" +
                                  $"\r\n\t- Current version: {currentVersion}" +
                                  $"\r\n\t- New version....: {versionString}");

                if (FileHelper.ChangeFileContent(assemblyFile, versionString))
                    Console.WriteLine("INFO > Version updated.");
                else
                    Console.Error.WriteLine("ERROR > An error has occured while updating the version number.");
            }
            catch (Exception ex)
            {
                Console.Error.Write($"ERROR > An error has occured: {ex.Message}");
            }
        }

        /// <summary>
        /// Compares the given and the current version and creates a new of the difference
        /// </summary>
        /// <param name="currentVersion">The current version</param>
        /// <param name="givenVersion">The given version</param>
        /// <returns>The new created version</returns>
        private static Version CompareVersions(Version currentVersion, Version givenVersion)
        {
            var majorNumber = currentVersion.Major < 0 ? 0 : currentVersion.Major;
            var minorNumber = currentVersion.Minor < 0 ? 0 : currentVersion.Minor;
            var buildNumber = currentVersion.Build < 0 ? 0 : currentVersion.Build;

            // Check if the major (year) and the minor (calendar week) are equal
            if (givenVersion.Major == majorNumber && givenVersion.Minor == minorNumber)
                buildNumber++; // The number is equals, so update the build number
            else
                buildNumber = 0;

            return new Version(givenVersion.Major, givenVersion.Minor, buildNumber, givenVersion.Revision < 0 ? 0 : givenVersion.Revision);
        }
    }
}
