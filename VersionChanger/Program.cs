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
            var givenVersion = parameters?.Version;

            if (givenVersion == null)
            {
                Console.WriteLine("No version number was specified. Number is generated");
                givenVersion = Global.CreateVersion();
                generatedVersion = true;
            }

            try
            {
                var assemblyFile = "";
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
                    if (givenVersion.Major == currentVersion.Major && givenVersion.Minor == currentVersion.Minor)
                        releaseNumber++;

                    version = new Version(givenVersion.Major, givenVersion.Minor, releaseNumber,
                        givenVersion.Revision);
                }
                else
                {
                    version = givenVersion;
                }

                Console.WriteLine("Version numbers:" +
                                  $"\r\n\t- Current version: {currentVersion}" +
                                  $"\r\n\t- New version....: {version}");

                if (FileHelper.ChangeFileContent(assemblyFile, version.ToString()))
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
