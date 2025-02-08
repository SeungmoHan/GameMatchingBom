using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandyControl.Tools.Extension;

namespace Test.Log
{
    public enum Level
    {
        Info, Error
    }

    public class Archive
    {
        public Level logLevl { get; set; } = Level.Info;
        public string who { get; set; } = string.Empty;
        public string data { get; set; } = string.Empty;
        public DateTime time { get; set; } = DateTime.Now;

        public Archive(Level logLevl, string who, string data)
        {
            this.logLevl = logLevl;
            this.who = who;
            this.data = data;
            this.time = DateTime.Now;
        }

        public string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"[{time:MM/dd/yyyy}-{time:hh:mm:ss}][{logLevl}]");
            if (false == who.IsNullOrEmpty())
                sb.AppendLine($"[{who}]:[{data}]");
            else
                sb.AppendLine($"[{data}]");
            return sb.ToString();
        }
    }

    public class Stash
    {
        private static List<Archive> StashLogView { get; set; } = new();

        public static List<string> GetStashLogString()
        {
            return StashLogView.OrderByDescending(x => x.time).Select(x => x.ToString()).ToList();
        }

        public static void LogError(string msg, string who="app")
        {
            AddLog(new Archive(Level.Error, who, msg));
        }

        public static void LogInfo(string msg, string who = "app")
        {
            AddLog(new Archive(Level.Info, who, msg));
        }

        private static void AddLog(Archive archive)
        {
            if (StashLogView.Count >= 20)
                StashLogView.RemoveAt(0);
            StashLogView.Add(archive);
        }

        public static void Clear()
        {
            StashLogView.Clear();
        }
    }
}
