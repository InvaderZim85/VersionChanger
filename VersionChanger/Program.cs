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
            var parameters = Global.ExtractParameter(args);

            Global.PrintParameters(parameters);

            var givenVersion = parameters?.Version;
            var versionFormat = parameters?.Format ?? Global.VersionNumberFormat.Long;

            if (givenVersion == null)
            {
                Console.WriteLine("No version number was specified. Number is generated");
                givenVersion = Global.CreateVersion(versionFormat);
                generatedVersion = true;
            }

            try
            {
                string assemblyFile;
                if (string.IsNullOrEmpty(parameters?.AssemblyInfoFile))
                {
                    // Get the path of the assembly file
                    assemblyFile = FileHelper.GetFile("AssemblyInfo", pattern: "*.cs");
                    if (string.IsNullOrEmpty(assemblyFile))
                    {
                        Console.Error.WriteLine("Path of the assembly info file could not be determined.");
                        return;
                    }
                }
                else
                {
                    assemblyFile = parameters.AssemblyInfoFile;
                    if (!File.Exists(assemblyFile))
                    {
                        Console.Error.WriteLine("Path of the assembly info file could not be determined.");
                        return;
                    }
                }

                // Get the current version
                var currentVersion = FileHelper.GetCurrentVersion(assemblyFile);

                Version version;
                if (generatedVersion)
                {
                    var releaseNumber = currentVersion.Build < 0 ? 0 : currentVersion.Build;
                    var revisionNumber = givenVersion.Revision < 0 ? 0 : givenVersion.Revision;

                    if (givenVersion.Major == currentVersion.Major && givenVersion.Minor == currentVersion.Minor)
                        releaseNumber++;

                    version = new Version(givenVersion.Major, givenVersion.Minor, releaseNumber, revisionNumber);
                }
                else
                {
                    version = givenVersion;
                }

                var versionString = Global.GetVersionString(version, versionFormat);
                Console.WriteLine("Version numbers:" +
                                  $"\r\n\t- Current version: {currentVersion}" +
                                  $"\r\n\t- New version....: {versionString}");

                if (FileHelper.ChangeFileContent(assemblyFile, versionString))
                    Console.WriteLine("Version updated.");
                else
                    Console.Error.WriteLine("An error has occured while updating the version number.");
            }
            catch (Exception ex)
            {
                Console.Error.Write($"An error has occured: {ex.Message}");
            }
        }
    }
}
