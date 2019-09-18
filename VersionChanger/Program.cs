﻿using System;
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
            var versionType = parameters?.VersionType ?? Global.VersionType.WithDaysAndMinutes;

            if (givenVersion == null)
            {
                Console.WriteLine("No version number was specified. Number is generated");
                givenVersion = Global.CreateVersion(versionFormat, versionType);
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
                    version = CompareVersions(versionType, currentVersion, givenVersion);
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

        /// <summary>
        /// Compares the given and the current version and creates a new of the difference
        /// </summary>
        /// <param name="type">The type of the version</param>
        /// <param name="currentVersion">The current version</param>
        /// <param name="givenVersion">The given version</param>
        /// <returns>The new created version</returns>
        private static Version CompareVersions(Global.VersionType type, Version currentVersion, Version givenVersion)
        {
            var majorNumber = currentVersion.Major < 0 ? 0 : currentVersion.Major;
            var minorNumber = currentVersion.Minor < 0 ? 0 : currentVersion.Minor;
            var buildNumber = currentVersion.Build < 0 ? 0 : currentVersion.Build;
            var revisionNumber = currentVersion.Revision < 0 ? 0 : currentVersion.Revision;

            if (type == Global.VersionType.WithCalendarWeek)
            {
                if (givenVersion.Major == majorNumber && givenVersion.Minor == minorNumber)
                    buildNumber++;

                return new Version(givenVersion.Major, givenVersion.Minor, buildNumber, givenVersion.Revision);
            }
            else
            {
                if (majorNumber == givenVersion.Major && minorNumber == givenVersion.Minor && buildNumber == givenVersion.Build)
                    revisionNumber++;

                return new Version(givenVersion.Major, givenVersion.Minor, givenVersion.Build, revisionNumber);
            }
        }
    }
}
