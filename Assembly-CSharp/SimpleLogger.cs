namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Provides access to the logging system with a formatted prefix of a given name "[Name] - Message".  This
    ///     is useful when you have a class that can't inherit from Loggable where you want easy logging.
    /// </summary>
    public class SimpleLogger : Loggable
    {
        /// <inheritdoc />
        /// <summary>
        ///     Constructs a Loggable Class with a given Name
        /// </summary>
        /// <param name="name"></param>
        public SimpleLogger(string name)
        {
            ClassName = name;
        }
    }
}
