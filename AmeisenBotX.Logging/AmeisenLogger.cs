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

        private AmeisenLogger()
        {
            LogQueue = new ConcurrentQueue<LogEntry>();

            Enabled = true;
            ActiveLogLevel = LogLevel.Debug;

            // default log path
            ChangeLogFolder(AppDomain.CurrentDomain.BaseDirectory + "log/");
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

        public void ChangeLogFolder(string logFolderPath)
        {
            LogFileFolder = logFolderPath;
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            LogFilePath = LogFileFolder + $"AmeisenBot.{DateTime.Now.ToString("dd.MM.yyyy")}.{DateTime.Now.ToString("HH.mm")}.txt";
        }

        public void Log(string message, LogLevel logLevel = LogLevel.Debug, [CallerFilePath] string callingClass = "", [CallerMemberName]string callingFunction = "", [CallerLineNumber] int callingCodeline = 0)
        {
            if (logLevel <= ActiveLogLevel)
            {
                LogQueue.Enqueue(new LogEntry(logLevel, message, Path.GetFileNameWithoutExtension(callingClass), callingFunction, callingCodeline));
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
