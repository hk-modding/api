using System;
using System.Globalization;
using System.IO;
using System.Linq;
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
        private static readonly object Locker = new();
        private static StreamWriter Writer;

        private static LogLevel _logLevel;
        private static bool _shortLoggingLevel;
        private static bool _includeTimestamps;

        private static string OldLogDir = Path.Combine(Application.persistentDataPath, "Old ModLogs");

        internal static readonly SimpleLogger APILogger = new("API");

        internal static void InitializeFileStream()
        {
            Debug.Log("Creating Mod Logger");
            
            _logLevel = LogLevel.Debug;

            Directory.CreateDirectory(OldLogDir);

            string current = Path.Combine(Application.persistentDataPath, "ModLog.txt");

            BackupLog(current, OldLogDir);
            
            var fs = new FileStream(current, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            
            lock (Locker) 
                Writer = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = true };

            File.SetCreationTimeUtc(current, DateTime.UtcNow);
        }

        private static void BackupLog(string path, string dir)
        {
            if (!File.Exists(path)) 
                return;
            
            string time = File.GetCreationTimeUtc(path).ToString("MM dd yyyy (HH mm ss)", CultureInfo.InvariantCulture);
            
            File.Move(path, Path.Combine(dir, $"ModLog {time}.txt"));
        }

        internal static void ClearOldModlogs()
        {
            string oldLogDir = Path.Combine(Application.persistentDataPath, "Old ModLogs");
            
            APILogger.Log($"Deleting modlogs older than {ModHooks.GlobalSettings.ModlogMaxAge} days ago");

            DateTime limit = DateTime.UtcNow.AddDays(-ModHooks.GlobalSettings.ModlogMaxAge);
            
            foreach (string file in Directory.GetFiles(oldLogDir).Where(f => File.GetCreationTimeUtc(f) < limit))
            {
                File.Delete(file);
            }
        }

        internal static void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
        }

        internal static void SetUseShortLogLevel(bool value)
        {
            _shortLoggingLevel = value;
        }

        internal static void SetIncludeTimestampt(bool value)
        {
            _includeTimestamps = value;
        }

        /// <summary>
        ///     Checks to ensure that the logger level is currently high enough for this message, if it is, write it.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Level of Log</param>
        public static void Log(string message, LogLevel level)
        {
            if (_logLevel > level) 
                return;
            
            string timeText = "[" + DateTime.Now.ToUniversalTime().ToString("HH:mm:ss") + "]:"; // uses ISO 8601
            string levelText = _shortLoggingLevel ? $"[{LogLevelExt.ToShortString(level).ToUpper()}]:" : $"[{level.ToString().ToUpper()}]:";
            string prefixText = _includeTimestamps ? timeText + levelText : levelText;
            
            WriteToFile(prefixText + String.Join(Environment.NewLine + prefixText, message.Split('\n')) + Environment.NewLine, level);
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
        /// <param name="level">Level of Log</param>
        private static void WriteToFile(string text, LogLevel level)
        {
            lock (Locker)
            {
                ModHooks.LogConsole(text, level);

                Writer?.Write(text);
            }
        }
    }
}
