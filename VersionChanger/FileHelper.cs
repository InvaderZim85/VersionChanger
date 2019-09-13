using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VersionChanger
{
    /// <summary>
    /// Provides several functions to interact with files
    /// </summary>
    internal static class FileHelper
    {
        /// <summary>
        /// Contains the regex for the version number
        /// </summary>
        private static readonly Regex VersionNumberRegex = new Regex(@"\d+\.{1}\d+\.{1}\d+\.{1}\d+");

        /// <summary>
        /// Gets the path of a specified file starting from the given directory
        /// </summary>
        /// <param name="filename">The name of the file</param>
        /// <param name="directory">The path of the start directory (optional, when empty the current directory will be chosen)</param>
        /// <param name="pattern">The search pattern (optional)</param>
        /// <returns>The full path of the file to be searched for</returns>
        public static string GetFile(string filename, string directory = "", string pattern = "*.*")
        {
            if (string.IsNullOrEmpty(directory))
                directory = Global.GetBaseFolder();

            var dirInfo = new DirectoryInfo(directory);
            if (!dirInfo.Exists)
                return "";

            var files = dirInfo.GetFiles(pattern, SearchOption.AllDirectories);

            var file = files.FirstOrDefault(f => f.FullName.ContainsIgnoreCase(filename));
            if (file == null)
                return dirInfo.Parent != null ? GetFile(filename, dirInfo.Parent.FullName, pattern) : "";

            return file.FullName;
        }

        /// <summary>
        /// Changes the content of the given file according to the given regex pattern and the replace string
        /// </summary>
        /// <param name="file">The path of the file</param>
        /// <param name="version">The new version number</param>
        /// <returns>true when successful, otherwise false</returns>
        /// <exception cref="ArgumentNullException">Will be thrown when the file or pattern parameter is empty</exception>
        /// <exception cref="FileNotFoundException">Will be thrown when the file doesn't exist</exception>
        public static bool ChangeFileContent(string file, string version)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            if (!File.Exists(file))
                throw new FileNotFoundException("The given file doesn't exist.", file);

            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException(nameof(version));

            var content = File.ReadAllText(file);
            if (string.IsNullOrEmpty(content))
                return false;

            content = VersionNumberRegex.Replace(content, version);

            File.WriteAllText(file, content);

            return File.Exists(file);
        }

        /// <summary>
        /// Gets the current version number
        /// </summary>
        /// <param name="file">The path of the file</param>
        /// <returns>The current version number</returns>
        /// <exception cref="ArgumentNullException">Will be thrown when the file parameter is empty</exception>
        /// <exception cref="FileNotFoundException">Will be thrown when the file doesn't exist</exception>
        public static Version GetCurrentVersion(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            if (!File.Exists(file))
                throw new FileNotFoundException("The given file doesn't exist.", file);

            var content = File.ReadAllText(file);
            if (string.IsNullOrEmpty(content))
                return new Version();

            var version = VersionNumberRegex.Match(content).Value;

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
    }
}
