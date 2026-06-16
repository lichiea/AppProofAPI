using System;
using System.IO;
using System.Threading;

namespace ProofAPI.Services
{
    public class Logger : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _lock = new();

        public Logger(string logFilePath = "proofapi.log")
        {
            _logFilePath = logFilePath;
        }

        public void Info(string message) => Log("INFO", message);
        public void Warning(string message) => Log("WARN", message);
        public void Error(string message, Exception? ex = null) => Log("ERROR", $"{message} {ex?.ToString()}");

        private void Log(string level, string message)
        {
            var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] [{Thread.CurrentThread.ManagedThreadId}] {message}";
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
                Console.WriteLine(logLine); // также в консоль
            }
        }
    }

    public interface ILogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
    }
}
