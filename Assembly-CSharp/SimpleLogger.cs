namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Provides access to the logging system with a formatted prefix of a given name "[ExampleClass] - My Message".  This
    ///     is usefull when you have a class that you can't inherit from Loggable in but that you still want tailored logging
    ///     for.
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