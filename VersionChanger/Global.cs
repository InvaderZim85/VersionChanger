using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Newtonsoft.Json;
using VersionChanger.DataObjects;

namespace VersionChanger
{
    /// <summary>
    /// Provides several useful functions
    /// </summary>
    internal static class Global
    {
        /// <summary>
        /// The different formats of the version number
        /// </summary>
        public enum VersionNumberFormat
        {
            /// <summary>
            /// The default value to indicate that no value was given
            /// </summary>
            None = 0,
            /// <summary>
            /// Long format with four places like 1.2.3.4 (default)
            /// </summary>
            Long = 1,
            /// <summary>
            /// Middle format with three places like 1.2.3
            /// </summary>
            Middle = 2,
            /// <summary>
            /// Short format with two places like 1.2
            /// </summary>
            Short = 3
        }

        /// <summary>
        /// The different version types
        /// </summary>
        public enum VersionType
        {
            /// <summary>
            /// The default value to indicate that no value was given
            /// </summary>
            None = 0,
            /// <summary>
            /// Creates a version with the following format: Year.DaysOfYear.ReleaseNumber.MinutesOfTheDay (e.G. 19.216.0.280) (default)
            /// </summary>
            WithDaysOfTheYear = 1,
            /// <summary>
            /// Creates a version with the following format: Year.CalendarWeek.ReleaseNumber.MinutesOfTheDay (e.G. 19.38.0.280)
            /// </summary>
            WithCalendarWeek = 2
        }

        /// <summary>
        /// Gets the path of the base folder
        /// </summary>
        /// <returns>The path of the base folder</returns>
        public static string GetBaseFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Search for a substring in a string and returns true if the substring was found
        /// </summary>
        /// <param name="value">The original string</param>
        /// <param name="substring">The substring</param>
        /// <returns>true if the substring was found, otherwise false</returns>
        public static bool ContainsIgnoreCase(this string value, string substring)
        {
            if (string.IsNullOrEmpty(substring))
                return false;

            return value.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Converts the given string into an int value. When the string value is not a valid int, a 0 will be returned
        /// </summary>
        /// <param name="value">The string value</param>
        /// <param name="fallback">The desired fallback</param>
        /// <returns>The converted int value</returns>
        private static int ToInt(this string value, int fallback = 0)
        {
            return int.TryParse(value, out var result) ? result : fallback;
        }

        /// <summary>
        /// Gets the command line parameters or the config file parameters
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>The loaded parameters, if nothing is available null will be returned</returns>
        public static Parameter LoadParameter(string[] args)
        {
            // Load the config
            var config = LoadConfig();

            // Extract the command line arguments
            return ExtractArguments(args, config);
        }

        /// <summary>
        /// Extracts the command line arguments and compares them with the config
        /// </summary>
        /// <param name="args">The list with the arguments</param>
        /// <param name="config">The config values</param>
        /// <returns>The parameters</returns>
        private static Parameter ExtractArguments(IEnumerable<string> args, Parameter config)
        {
            var result = new Parameter();

            Parser.Default.ParseArguments<Arguments>(args).WithParsed(a =>
            {
                result.VersionType = a.VersionType != 0 // version not equal 0
                    ? (VersionType) a.VersionType // Take the value from the arguments
                    : config.VersionType != VersionType.None // Check the config value
                        ? config.VersionType // Take the value from the config
                        : VersionType.WithDaysOfTheYear; // Set the default value
                result.Format = a.Format != 0 // Check
                    ? (VersionNumberFormat) a.Format // Argument
                    : config.Format != VersionNumberFormat.None // Check
                        ? config.Format // Config value
                        : VersionNumberFormat.Long; // Default value
                result.Version = string.IsNullOrEmpty(a.Version) ? config.Version : ExtractVersion(a.Version);
                result.AssemblyInfoFiles = a.AssemblyFiles.Any() ? a.AssemblyFiles.ToList() : config.AssemblyInfoFiles;
            }).WithNotParsed(w => result = config);

            return result;
        }

        /// <summary>
        /// Loads the parameters of the config file if it's exists
        /// </summary>
        /// <returns>The parameters. If no parameters available, null will be returned</returns>
        private static Parameter LoadConfig()
        {
            var path = Path.Combine(GetBaseFolder(), "VersionChangerConfig.json");

            if (!File.Exists(path))
                return null;

            try
            {
                var content = File.ReadAllText(path);

                return string.IsNullOrEmpty(content) ? null : JsonConvert.DeserializeObject<Parameter>(content);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR > An error has occured while loading the config file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts a given version number
        /// </summary>
        /// <param name="version">The version number</param>
        /// <returns>The version</returns>
        public static Version ExtractVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return null;

            try
            {
                return Version.Parse(version);
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.Error.WriteLine($"Negative value in {version}");
                return new Version();
            }
            catch (ArgumentException)
            {
                Console.Error.WriteLine($"Bad number of components in {version}");
                return new Version();
            }
            catch (FormatException)
            {
                Console.Error.WriteLine($"Non-integer value in {version}");
                return new Version();
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine($"Number out of range in {version}");
                return new Version();
            }
        }

        /// <summary>
        /// Creates a new version number
        /// </summary>
        /// <returns>The version number</returns>
        public static Version CreateVersion(VersionNumberFormat format, VersionType type)
        {
            var year = DateTime.Now.ToString("yy").ToInt();
            var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay,
                DayOfWeek.Monday);

            var daysOfYear = DateTime.Now.DayOfYear;
            var minuteOfDay = (int) DateTime.Now.TimeOfDay.TotalMinutes;

            switch (format)
            {
                case VersionNumberFormat.Short:
                    return type == VersionType.WithCalendarWeek
                        ? new Version(year, week)
                        : new Version(year, daysOfYear);
                case VersionNumberFormat.Middle:
                    return type == VersionType.WithCalendarWeek
                        ? new Version(year, week, 0)
                        : new Version(year, daysOfYear, 0);
                default:
                    return type == VersionType.WithCalendarWeek
                        ? new Version(year, week, 0, minuteOfDay)
                        : new Version(year, daysOfYear, 0, minuteOfDay);
            }
        }

        /// <summary>
        /// Gets the desired version string
        /// </summary>
        /// <param name="version">The version</param>
        /// <param name="format">The version format</param>
        /// <returns>The formatted version</returns>
        public static string GetVersionString(Version version, VersionNumberFormat format)
        {
            switch (format)
            {
                case VersionNumberFormat.Short:
                    return $"{version.Major}.{version.Minor}";
                case VersionNumberFormat.Middle:
                    return $"{version.Major}.{version.Minor}.{version.Build}";
                default:
                    return version.ToString();
            }
        }
    }
}
