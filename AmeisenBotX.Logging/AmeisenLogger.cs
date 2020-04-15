using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Logging.Objects;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AmeisenBotX.Logging
{
    public class AmeisenLogger
    {
        private static readonly object Padlock = new object();
        private static AmeisenLogger instance;

        private AmeisenLogger(bool deleteOldLogs = true)
        {
            LogQueue = new ConcurrentQueue<LogEntry>();

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

        private ConcurrentQueue<LogEntry> LogQueue { get; }

        private Thread LogWorker { get; set; }

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
            try
            {
                foreach (string file in Directory.GetFiles(LogFileFolder))
                {
                    FileInfo fileInfo = new FileInfo(file);

                    if (fileInfo.LastAccessTime < DateTime.Now.AddDays(daysToKeep * -1))
                    {
                        fileInfo.Delete();
                    }
                }
            }
            catch { }
        }

        public void Log(string tag, string message, LogLevel logLevel = LogLevel.Debug, [CallerFilePath] string callingClass = "", [CallerMemberName]string callingFunction = "", [CallerLineNumber] int callingCodeline = 0)
        {
            if (logLevel <= ActiveLogLevel)
            {
                LogQueue.Enqueue(new LogEntry(logLevel, $"{$"[{tag}]",-24} {message}", Path.GetFileNameWithoutExtension(callingClass), callingFunction, callingCodeline));
            }
        }

        public void Start()
        {
            Enabled = true;

            if (LogWorker?.IsAlive != true)
            {
                LogWorker = new Thread(new ThreadStart(DoLogWork));
                LogWorker.Start();
            }
        }

        public void Stop()
        {
            Enabled = false;
            if (LogWorker.IsAlive)
            {
                LogWorker.Join();
            }
        }

        private void DoLogWork()
        {
            while (Enabled || !LogQueue.IsEmpty)
            {
                if (LogQueue.TryDequeue(out LogEntry activeEntry))
                {
                    File.AppendAllText(LogFilePath, activeEntry.ToString() + "\n");
                }

                Thread.Sleep(1);
            }
        }
    }
}