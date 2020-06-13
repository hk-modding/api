using System;
using System.Globalization;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable file UnusedMember.Global

namespace Modding
{
    /// <summary>
    ///     Shared logger for mods to use.
    /// </summary>
    [PublicAPI]
    // This is threadsafe, but it's blocking.  Hopefully mods don't try to log so much that it becomes an issue.  If it does we'll have to look at a better system.
    public static class Logger
    {
        private static readonly object Locker = new object();
        private static readonly StreamWriter Writer;

        private static LogLevel _logLevel;

        internal static readonly SimpleLogger APILogger = new SimpleLogger("API");

        /// <summary>
        ///     Logger Constructor.  Initializes file to write to.
        /// </summary>
        static Logger()
        {
            Debug.Log("Creating Mod Logger");
            _logLevel = LogLevel.Debug;

            string oldLogDir = Path.Combine(Application.persistentDataPath, "Old ModLogs");
            if (!Directory.Exists(oldLogDir))
            {
                Directory.CreateDirectory(oldLogDir);
            }

            foreach (string fileName in Directory.GetFiles(oldLogDir))
            {
                if (File.GetCreationTimeUtc(fileName) < DateTime.UtcNow.AddDays(-7))
                {
                    File.Delete(fileName);
                }
            }

            string currLogName = Path.Combine(Application.persistentDataPath, "ModLog.txt");
            if (File.Exists(currLogName))
            {
                string oldLogName = "ModLog " + File.GetCreationTimeUtc(currLogName)
                                        .ToString("MM dd yyyy (HH mm ss)", CultureInfo.InvariantCulture) + ".txt";
                File.Move(currLogName, Path.Combine(oldLogDir, oldLogName));
            }

            FileStream fileStream = new FileStream(currLogName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            Writer = new StreamWriter(fileStream, Encoding.UTF8) {AutoFlush = true};

            File.SetCreationTimeUtc(currLogName, DateTime.UtcNow);
        }

        internal static void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
        }

        /// <summary>
        ///     Checks to ensure that the logger level is currently high enough for this message, if it is, write it.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Level of Log</param>
        public static void Log(string message, LogLevel level)
        {
            if (_logLevel <= level)
            {
                string levelText = "[" + level.ToString().ToUpper() + "]:";
                WriteToFile(levelText + message.Replace("\n", "\n" + levelText) + Environment.NewLine);
            }
        }

        /// <summary>
        ///     Checks to ensure that the logger level is currently high enough for this message, if it is, write it.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Level of Log</param>
        public static void Log(object message, LogLevel level)
        {
            Log(message.ToString(), level);
        }


        /// <summary>
        ///     Finest/Lowest level of logging.  Usually reserved for developmental testing.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogFine(string message)
        {
            Log(message, LogLevel.Fine);
        }

        /// <summary>
        ///     Finest/Lowest level of logging.  Usually reserved for developmental testing.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogFine(object message)
        {
            Log(message.ToString(), LogLevel.Fine);
        }

        /// <summary>
        ///     Log at the debug level.  Usually reserved for diagnostics.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogDebug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        /// <summary>
        ///     Log at the debug level.  Usually reserved for diagnostics.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogDebug(object message)
        {
            Log(message, LogLevel.Debug);
        }

        /// <summary>
        ///     Log at the info level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Log(string message)
        {
            Log(message, LogLevel.Info);
        }

        /// <summary>
        ///     Log at the info level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Log(object message)
        {
            Log(message, LogLevel.Info);
        }

        /// <summary>
        ///     Log at the warning level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogWarn(string message)
        {
            Log(message, LogLevel.Warn);
        }

        /// <summary>
        ///     Log at the warning level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogWarn(object message)
        {
            Log(message, LogLevel.Warn);
        }

        /// <summary>
        ///     Log at the error level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogError(string message)
        {
            Log(message, LogLevel.Error);
        }

        /// <summary>
        ///     Log at the error level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogError(object message)
        {
            Log(message, LogLevel.Error);
        }

        /// <summary>
        ///     Locks file to write, writes to file, releases lock.
        /// </summary>
        /// <param name="text">Text to write</param>
        private static void WriteToFile(string text)
        {
            lock (Locker)
            {
                if (ModHooks.IsInitialized)
                {
                    ModHooks.Instance.LogConsole(text);
                }

                Writer.Write(text);
            }
        }
    }
}