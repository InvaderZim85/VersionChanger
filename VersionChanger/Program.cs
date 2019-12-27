using System;
using System.IO;
using System.Linq;

namespace VersionChanger
{
    /// <summary>
    /// Provides the main entry point of the program
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Contains the value which indicates if the version number was generated or shipped with the command line arguments
        /// </summary>
        private static bool _generatedVersion;

        /// <summary>
        /// The main entry point of the program
        /// </summary>
        /// <param name="args">The shipped arguments</param>
        private static void Main(string[] args)
        {
            var parameters = Global.LoadParameter(args);

            if (parameters == null)
            {
                Console.Error.WriteLine("ERROR > Settings missing.");
                return;
            }

            parameters.Print();

            var givenVersion = parameters.Version;
            var versionFormat = parameters.Format;
            var versionType = parameters.VersionType;

            if (givenVersion == null)
            {
                Console.WriteLine("INFO > No version number was specified. Number is generated");
                givenVersion = Global.CreateVersion(versionFormat, versionType);
                _generatedVersion = true;
            }

            try
            {
                var files = FileHelper.GetAssemblyFiles(parameters.AssemblyInfoFiles);
                if (files == null)
                {
                    Console.Error.WriteLine("ERROR > Path of the assembly info file could not be determined.");
                    return;
                }

                foreach (var file in files)
                {
                    UpdateVersion(file, givenVersion, versionFormat);
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write($"ERROR > An error has occured: {ex.Message}");
            }
        }

        /// <summary>
        /// Changes the version of the version file
        /// </summary>
        /// <param name="filepath">The file path</param>
        /// <param name="givenVersion"></param>
        /// <param name="versionFormat"></param>
        private static void UpdateVersion(string filepath, Version givenVersion, Global.VersionNumberFormat versionFormat)
        {
            Console.WriteLine($"INFO > Update version. File: {filepath}");

            // Get the current version
            var currentVersion = FileHelper.GetCurrentVersion(filepath);

            var version = _generatedVersion ? CompareVersions(currentVersion, givenVersion) : givenVersion;

            var versionString = Global.GetVersionString(version, versionFormat);
            Console.WriteLine("INFO > Version numbers:" +
                              $"\r\n\t- Current version: {currentVersion}" +
                              $"\r\n\t- New version....: {versionString}");

            if (FileHelper.ChangeFileContent(filepath, versionString))
                Console.WriteLine("INFO > Version updated.");
            else
                Console.Error.WriteLine("ERROR > An error has occured while updating the version number.");
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
