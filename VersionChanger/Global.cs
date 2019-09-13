using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VersionChanger
{
    /// <summary>
    /// Provides several useful functions
    /// </summary>
    internal static class Global
    {
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
        /// <returns>The converted int value</returns>
        public static int ToInt(this string value)
        {
            return int.TryParse(value, out var result) ? result : 0;
        }

        /// <summary>
        /// Extracts the shipped version
        /// </summary>
        /// <param name="args">The shipped arguments</param>
        /// <returns>The version</returns>
        public static Version ExtractVersion(string[] args)
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
                if (argument.Contains("."))
                    return ExtractVersion(argument);
            }

            return new Version(major, minor, build, revision);
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
        public static Version CreateVersion()
        {
            var year = DateTime.Now.ToString("yy").ToInt();
            var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay,
                DayOfWeek.Monday);

            return new Version(year, week, 0, (int) DateTime.Now.TimeOfDay.TotalMinutes);
        }
    }
}
