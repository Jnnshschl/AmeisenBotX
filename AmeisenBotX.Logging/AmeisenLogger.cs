using AmeisenBotX.Common.Utils;
using AmeisenBotX.Logging.Enums;
using System;
using System.IO;
using System.Text;

namespace AmeisenBotX.Logging
{
    public class AmeisenLogger
    {
        private static readonly object padlock = new();
        private static readonly object stringBuilderLock = new();
        private static AmeisenLogger instance;

        private AmeisenLogger(bool deleteOldLogs = false)
        {
            StringBuilder = new();
            Enabled = false;
            ActiveLogLevel = LogLevel.Debug;

            // default log path
            ChangeLogFolder(AppDomain.CurrentDomain.BaseDirectory + "log/", false);

            if (deleteOldLogs)
            {
                DeleteOldLogs();
            }

            LockedTimer logFileWriter = new(1000, LogFileWriterTick);
        }

        public event Action<LogLevel, string> OnLog;

        public event Action<string, string, LogLevel> OnLogRaw;

        public static AmeisenLogger I
        {
            get
            {
                lock (padlock)
                {
                    instance ??= new(true);
                    return instance;
                }
            }
        }

        public LogLevel ActiveLogLevel { get; set; }

        public bool Enabled { get; private set; }

        public string LogFileFolder { get; private set; }

        public string LogFilePath { get; private set; }

        private StringBuilder StringBuilder { get; }

        public void ChangeLogFolder(string logFolderPath, bool createFolder = true, bool deleteOldLogs = true)
        {
            LogFileFolder = logFolderPath;

            if (createFolder && !Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            LogFilePath = LogFileFolder + $"AmeisenBot.{DateTime.Now:dd.MM.yyyy}-{DateTime.Now:HH.mm}.txt";

            if (deleteOldLogs)
            {
                DeleteOldLogs();
            }
        }

        public void DeleteOldLogs(int daysToKeep = 1)
        {
            if (Directory.Exists(LogFileFolder))
            {
                string[] files = Directory.GetFiles(LogFileFolder);

                for (int i = 0; i < files.Length; ++i)
                {
                    string file = files[i];
                    FileInfo fileInfo = new(file);

                    if (fileInfo.LastAccessTime < DateTime.Now.AddDays(daysToKeep * -1))
                    {
                        fileInfo.Delete();
                    }
                }
            }
        }

        public void Log(string tag, string log, LogLevel logLevel = LogLevel.Debug) // [CallerFilePath] string callingClass = "", [CallerMemberName] string callingFunction = "", [CallerLineNumber] int callingCodeline = 0
        {
            if (Enabled && logLevel <= ActiveLogLevel)
            {
                lock (stringBuilderLock)
                {
                    OnLogRaw?.Invoke(tag, log, logLevel);

                    string line = $"[{DateTime.UtcNow.ToLongTimeString()}] {$"[{logLevel}]",-9} {$"[{tag}]",-24} {log}";

                    StringBuilder.AppendLine(line);
                    OnLog?.Invoke(logLevel, line);
                }
            }
        }

        public void Start()
        {
            Enabled = true;
        }

        public void Stop()
        {
            if (Enabled)
            {
                Enabled = false;
                LogFileWriterTick();
            }
        }

        private void LogFileWriterTick()
        {
            if (Enabled)
            {
                lock (stringBuilderLock)
                {
                    File.AppendAllText(LogFilePath, StringBuilder.ToString());
                    StringBuilder.Clear();
                }
            }
        }
    }
}