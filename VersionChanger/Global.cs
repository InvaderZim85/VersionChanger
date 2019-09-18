using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
            /// Creates a version with the following format: Year.DaysOfYear.ReleaseNumber.MinutesOfTheDay (e.G. 19.216.0.280)
            /// </summary>
            WithDaysAndMinutes = 1,
            /// <summary>
            /// Creates a version with the following format: Year.CalendarWeek.ReleaseNumber.MinutesOfTheDay (e.G. 19.38.0.280)
            /// </summary>
            WithCalendarWeek = 2
        }

        /// <summary>
        /// Contains the different argument keys
        /// </summary>
        private enum ArgumentKeys
        {
            /// <summary>
            /// The major version number
            /// </summary>
            Major,

            /// <summary>
            /// The minor version number
            /// </summary>
            Minor,

            /// <summary>
            /// The build number
            /// </summary>
            Build,

            /// <summary>
            /// The revision number
            /// </summary>
            Revision,

            /// <summary>
            /// The path of the assembly file
            /// </summary>
            File,

            /// <summary>
            /// The desired version format
            /// </summary>
            Format,

            /// <summary>
            /// The desired version type
            /// </summary>
            Type,

            /// <summary>
            /// The complete version
            /// </summary>
            Version
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
            // Check if the user added some parameters
            return args.Any() ? ExtractParameter(args) : LoadParameter();
        }

        /// <summary>
        /// Loads the parameters of the config file if it's exists
        /// </summary>
        /// <returns>The parameters. If no parameters available, null will be returned</returns>
        private static Parameter LoadParameter()
        {
            var path = Path.Combine(GetBaseFolder(), "VersionChangerConfig.json");

            if (!File.Exists(path))
                return null;

            try
            {
                var content = File.ReadAllText(path);

                if (string.IsNullOrEmpty(content))
                    return null;

                return JsonConvert.DeserializeObject<Parameter>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error has occured while loading the config file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts the parameters from the given list
        /// </summary>
        /// <param name="args">The list with the arguments</param>
        /// <returns>The parameters</returns>
        private static Parameter ExtractParameter(string[] args)
        {
            var parameter = new Parameter();

            var parameterList = new List<ArgumentValue>();
            ArgumentValue value = null;

            foreach (var entry in args)
            {
                if (entry.StartsWith("-"))
                {
                    if (value != null)
                        parameterList.Add(value);

                    value = new ArgumentValue(entry.Replace("-", ""));
                }
                else if (value != null)
                {
                    value.Value += $"{entry} ";
                }
            }

            // Add the last argument value
            if (value != null)
                parameterList.Add(value);

            // Nothing there, return null
            if (!parameterList.Any())
                return null;

            // Get the version
            Version version = null;
            if (parameterList.Any(a =>
                a.Key.Equals(ArgumentKeys.Version.ToString(), StringComparison.InvariantCultureIgnoreCase)))
            {
                version = ExtractVersion(GetEntry<string>(ArgumentKeys.Version, parameterList));
            }
            else
            {
                var major = GetEntry<int>(ArgumentKeys.Major, parameterList);
                var minor = GetEntry<int>(ArgumentKeys.Minor, parameterList);
                var build = GetEntry<int>(ArgumentKeys.Build, parameterList);
                var revision = GetEntry<int>(ArgumentKeys.Revision, parameterList);

                // Only when there is an number
                if (major > 0 || minor > 0 || build > 0 || revision > 0)
                    version = new Version(major, minor, build, revision);
            }

            parameter.Version = version;
            parameter.AssemblyInfoFile = GetEntry<string>(ArgumentKeys.File, parameterList);

            var format = GetEntry<int>(ArgumentKeys.Format, parameterList);
            parameter.Format = (VersionNumberFormat) (format > 0 ? format : 1);

            var type = GetEntry<int>(ArgumentKeys.Type, parameterList);
            parameter.VersionType = (VersionType) (type > 0 ? type : 1);

            return parameter;
        }

        /// <summary>
        /// Gets the entry of the given key and tries to convert it into the desired type
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key</param>
        /// <param name="arguments">The list with the arguments</param>
        /// <returns>The converted type</returns>
        private static T GetEntry<T>(ArgumentKeys key, List<ArgumentValue> arguments) where T : IConvertible
        {
            var entry = arguments.FirstOrDefault(f => f.Key.Equals(key.ToString(), StringComparison.InvariantCultureIgnoreCase));
            if (entry == null)
                return default;

            if (string.IsNullOrEmpty(entry.Value))
                return default;

            try
            {
                return (T) Convert.ChangeType(entry.Value.Trim(), typeof(T));
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"ERROR > An error has occured while extracting the command line parameters. Message: {ex.Message}");
                return default;
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
                        : new Version(year, daysOfYear, minuteOfDay);
                default:
                    return type == VersionType.WithCalendarWeek
                        ? new Version(year, week, 0, (int) DateTime.Now.TimeOfDay.TotalMinutes)
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
