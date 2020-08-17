using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Logging.Objects;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace AmeisenBotX.Logging
{
    public class AmeisenLogger
    {
        private static readonly object padlock = new object();
        private static AmeisenLogger instance;
        private int timerBusy;

        private AmeisenLogger(bool deleteOldLogs = true)
        {
            LogBuilder = new ConcurrentQueue<LogEntry>();

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

        public static AmeisenLogger I
        {
            get
            {
                lock (padlock)
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

        private ConcurrentQueue<LogEntry> LogBuilder { get; }

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

        public void Log(string tag, string log, LogLevel logLevel = LogLevel.Debug, [CallerFilePath] string callingClass = "", [CallerMemberName] string callingFunction = "", [CallerLineNumber] int callingCodeline = 0)
        {
            Task.Run(() =>
            {
                if (logLevel <= ActiveLogLevel)
                {
                    LogBuilder.Enqueue(new LogEntry(logLevel, $"{$"[{tag}]",-24} {log}", Path.GetFileNameWithoutExtension(callingClass), callingFunction, callingCodeline));
                }
            });
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
            // only start one timer tick at a time
            if (Interlocked.CompareExchange(ref timerBusy, 1, 0) == 1)
            {
                return;
            }

            try
            {
                StringBuilder sb = new StringBuilder();

                while (!LogBuilder.IsEmpty && LogBuilder.TryDequeue(out LogEntry logEntry))
                {
                    sb.AppendLine(logEntry.ToString());
                }

                File.AppendAllText(LogFilePath, sb.ToString());
            }
            finally
            {
                timerBusy = 0;
            }
        }
    }
}