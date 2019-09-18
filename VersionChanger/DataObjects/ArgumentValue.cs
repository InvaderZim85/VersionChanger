namespace VersionChanger.DataObjects
{
    /// <summary>
    /// Represents a command line parameter entry
    /// </summary>
    public class ArgumentValue
    {
        /// <summary>
        /// Gets or sets the key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ArgumentValue"/> and sets the key
        /// </summary>
        /// <param name="key">The key</param>
        public ArgumentValue(string key)
        {
            Key = key;
        }
    }
}
