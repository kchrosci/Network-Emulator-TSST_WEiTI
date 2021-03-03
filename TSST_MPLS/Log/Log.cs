using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LogNamespace
{
    public class Log
    {
        private struct LogEntry
        {
            public DateTime time { get; set; }
            public string log { get; set; }
        }
        public Log()
        {
            Task.Run(() => WriteLog());
        }

        public List<LogEntry> log = new List<LogEntry>();
        public void AddLog(DateTime _time, string _log)
        {
            log.Add(new LogEntry() { log = _log, time = _time });
        }
        
        public void WriteLog()
        {
            while (true)
            {
                if (log.Count > 0)
                {
                    LogEntry logen = log[0];
                    log.RemoveAt(0);
                    if (log.Count > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("[" + "{0:HH:mm:ss.fff}" + "] ", logen.time);
                        Console.ResetColor();
                        Console.WriteLine(logen.log);
                    }
                }
                Thread.Sleep(10);
            }
        }
    }
}
