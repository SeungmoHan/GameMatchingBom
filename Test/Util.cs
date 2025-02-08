using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Tools.Extension;

namespace Test
{

    public class Util
    {
        public static void AddNew(User newUser,ref List<User> list)
        {
            List<User> newList = new();
            newList.Add(newUser);
            newList.AddRange(list);
            list = newList;
        }
        public static void RemoveName(string name,ref List<User> list)
        {
            int idx = list.FindIndex(x => x.Name == name);
            if (idx == -1)
                return;
            list.RemoveAt(idx);
        }

        public static User? FindUser(string name, List<User> list)
        {
            return list.Find(u => u.Name == name);
        }

        public static bool HasName(string name, List<User> list)
        {
            foreach (var user in list)
            {
                if (user.Name == name)
                    return true;
            }

            return false;
        }

        public static User? GetUser(string name, List<User> list)
        {
            foreach (var user in list)
            {
                if (user.Name == name)
                    return user;
            }
            return null;
        }

        public static bool InvalidName(string name)
        {
            if (name.IsNullOrEmpty())
                return true;
            if (name == "이름 추가")
                return true;
            if (name == "이 름   추 가...")
                return true;

            return false;
        }

        public static void SortByTier(List<User> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].Tier <= list[j].Tier)
                    {
                        (list[i], list[j]) = (list[j], list[i]);
                    }
                }
            }
        }

        public static void WriteToFileAndOpen(string writeMessage)
        {
            StringBuilder msgSb = new();
            msgSb.AppendLine("아래 링크에 복붙해 넣으세여");
            msgSb.AppendLine("https://www.notion.so/9faec2a686334048a762afd011a968b3");
            msgSb.AppendLine(writeMessage);
            string filePath = "JsonSerialize.json";
            File.Delete(filePath);
            File.WriteAllText(filePath, msgSb.ToString());

            HandyControl.Controls.MessageBox.Show($"열리는 파일에 있는 내용을 취업밤 노션에 옮겨적으세요", "Notice!!", MessageBoxButton.OK);

            var fullPath = Path.GetFullPath(filePath);
            try
            {
                var vsCodePath = Path.GetFullPath("%UserProfile%\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe");
                Process.Start(vsCodePath, fullPath);
            }
            catch (Exception e)
            {
                Process.Start("notepad.exe", fullPath);
            }
        }

        public static void Shuffle(ref List<User> nameList)
        {
            Random rand = new Random();

            int n = nameList.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (nameList[i], nameList[j]) = (nameList[j], nameList[i]); // Swap
            }
        }
    }
}
