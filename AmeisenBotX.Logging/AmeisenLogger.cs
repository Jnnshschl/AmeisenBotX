using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Logging.Objects;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

namespace AmeisenBotX.Logging
{
    public class AmeisenLogger
    {
        private static readonly object Padlock = new object();
        private static readonly object fileLock = new object();
        private static AmeisenLogger instance;

        private AmeisenLogger(bool deleteOldLogs = true)
        {
            LogBuilder = new StringBuilder();

            LogFileWriter = new Timer(1000);
            LogFileWriter.Elapsed += LogFileWriter_Elapsed;

            Enabled = true;
            ActiveLogLevel = LogLevel.Debug;

            // default log path
            ChangeLogFolder(AppDomain.CurrentDomain.BaseDirectory + "log/", false);

            if (deleteOldLogs)
            {
                DeleteOldLogs();
            }
        }

        public static AmeisenLogger Instance
        {
            get
            {
                lock (Padlock)
                {
                    if (instance == null)
                    {
                        instance = new AmeisenLogger();
                    }

                    return instance;
                }
            }
        }

        public LogLevel ActiveLogLevel { get; set; }

        public bool Enabled { get; private set; }

        public string LogFileFolder { get; private set; }

        public string LogFilePath { get; private set; }

        private StringBuilder LogBuilder { get; }

        private Timer LogFileWriter { get; set; }

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
                    FileInfo fileInfo = new FileInfo(file);

                    if (fileInfo.LastAccessTime < DateTime.Now.AddDays(daysToKeep * -1))
                    {
                        fileInfo.Delete();
                    }
                }
            }
        }

        public void Log(string tag, string message, LogLevel logLevel = LogLevel.Debug, [CallerFilePath] string callingClass = "", [CallerMemberName]string callingFunction = "", [CallerLineNumber] int callingCodeline = 0)
        {
            if (logLevel <= ActiveLogLevel)
            {
                LogBuilder.AppendLine(new LogEntry(logLevel, $"{$"[{tag}]",-24} {message}", Path.GetFileNameWithoutExtension(callingClass), callingFunction, callingCodeline).ToString());
            }
        }

        public void Start()
        {
            Enabled = true;
            LogFileWriter.Enabled = true;
        }

        public void Stop()
        {
            Enabled = false;
            LogFileWriter.Enabled = false;
            LogFileWriter_Elapsed(null, null);
        }

        private void LogFileWriter_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (fileLock)
            {
                File.AppendAllText(LogFilePath, LogBuilder.ToString());
                LogBuilder.Clear();
            }
        }
    }
}