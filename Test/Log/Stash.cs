using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandyControl.Tools;
using HandyControl.Tools.Extension;

namespace Test.Log
{
    public class Archive
    {
        public Logger.Level level { get; set; }
        public string who { get; set; }
        public string msg { get; set; }
        public DateTime time { get; set; }
        public Archive(Logger.Level level, string who, string msg)
        {
            this.level = level;
            this.who = who;
            this.msg = msg;
            this.time = DateTime.Now;
        }
        public override string ToString()
        {
            return $"{time:yyyy-MM-dd HH:mm:ss} [{level}] {who}: {msg}";
        }
    }

    public class Stash
    {
        private static List<Archive> StashLogView { get; set; } = new();

        public static List<string> GetStashLogString()
        {
            return StashLogView.OrderByDescending(x => x.time).Select(x => x.ToString()).ToList();
        }

        public static void LogError(string msg, string who = "app")
        {
            AddLog(new Archive(Logger.Level.Error, who, msg));
        }

        public static void LogInfo(string msg, string who = "app")
        {
            AddLog(new Archive(Logger.Level.Info, who, msg));
        }

        private static void AddLog(Archive archive)
        {
            if (StashLogView.Count >= 20)
            {
                //Task.Run(async () => { await File.WriteAsync(StashLogView[0]); });
                StashLogView.RemoveAt(0);
            }
            StashLogView.Add(archive);
        }

        public static void Clear()
        {
            StashLogView.Clear();
        }

        public static void Flush()
        {
            //File.WriteSync(StashLogView.ToList());
            StashLogView.Clear();
        }
    }
}
