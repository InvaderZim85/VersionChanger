using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
            /// Short format with two places like 1.2
            /// </summary>
            Short,
            /// <summary>
            /// Middle format with three places like 1.2.3
            /// </summary>
            Middle,
            /// <summary>
            /// Long format with four places like 1.2.3.4 (default)
            /// </summary>
            Long
        }

        /// <summary>
        /// The different version types
        /// </summary>
        public enum VersionType
        {
            /// <summary>
            /// Creates a version with the following format: Year.CalendarWeek.ReleaseNumber.MinutesOfTheDay (e.G. 19.38.0.280)
            /// </summary>
            WithCalendarWeek,
            /// <summary>
            /// Creates a version with the following format: Year.DaysOfYear.ReleaseNumber.MinutesOfTheDay (e.G. 19.216.0.280)
            /// </summary>
            WithDaysAndMinutes
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
        /// Extracts the shipped parameters
        /// </summary>
        /// <param name="args">The parameters</param>
        /// <returns>The parameters</returns>
        public static Parameter ExtractParameter(string[] args)
        {
            if (!args.Any())
                return null;

            var parameter = new Parameter();

            var path = args.FirstOrDefault(f => f.ContainsIgnoreCase("-f="));
            if (!string.IsNullOrEmpty(path))
            {
                parameter.AssemblyInfoFile = path.Replace("-f=", "");
            }

            var versionFormat = args.FirstOrDefault(f => f.ContainsIgnoreCase("-vf="));
            if (!string.IsNullOrEmpty(versionFormat))
            {
                parameter.Format =
                    (VersionNumberFormat) versionFormat.Replace("-vf=", "").ToInt((int) VersionNumberFormat.Long);
            }

            var versionType = args.FirstOrDefault(f => f.ContainsIgnoreCase("-vt="));
            if (!string.IsNullOrEmpty(versionType))
            {
                parameter.VersionType =
                    (VersionType) versionType.Replace("-vt=", "").ToInt((int) VersionType.WithDaysAndMinutes);
            }
            parameter.Version = ExtractVersion(args);

            return parameter;
        }

        /// <summary>
        /// Extracts the shipped version
        /// </summary>
        /// <param name="args">The shipped arguments</param>
        /// <returns>The version</returns>
        private static Version ExtractVersion(string[] args)
        {
            // Check if something was transferred
            if (!args.Any())
                return null;

            var major = 0;
            var minor = 0;
            var build = 0;
            var revision = 0;
            foreach (var argument in args)
            {
                if (argument.ContainsIgnoreCase("-ma="))
                    major = argument.Replace("-ma=", "").ToInt();

                if (argument.ContainsIgnoreCase("-mi="))
                    minor = argument.Replace("-mi=", "").ToInt();

                if (argument.ContainsIgnoreCase("-b="))
                    build = argument.Replace("-b=", "").ToInt();

                if (argument.ContainsIgnoreCase("-r="))
                    revision = argument.Replace("-r=", "").ToInt();

                // A complete version number is given
                if (!argument.ContainsIgnoreCase("-f=") && argument.Contains("."))
                    return ExtractVersion(argument);
            }

            return major == 0 && minor == 0 && build == 0 && revision == 0
                ? null
                : new Version(major, minor, build, revision);
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
                    return type == VersionType.WithDaysAndMinutes
                        ? new Version(year, week, 0)
                        : new Version(year, daysOfYear, minuteOfDay);
                default:
                    return type == VersionType.WithDaysAndMinutes
                        ? new Version(year, week, 0, (int) DateTime.Now.TimeOfDay.TotalMinutes)
                        : new Version(year, daysOfYear, minuteOfDay, 0);
            }
        }

        /// <summary>
        /// Prints the parameters
        /// </summary>
        /// <param name="parameters">The parameters</param>
        public static void PrintParameters(Parameter parameters)
        {
            Console.WriteLine("Parameters:" +
                              $"\r\n\t- Version: {parameters?.Version?.ToString() ?? "/"}" +
                              $"\r\n\t- File...: {parameters?.AssemblyInfoFile ?? "/"}" +
                              $"\r\n\t- Format.: {parameters?.Format ?? VersionNumberFormat.Long}");
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
