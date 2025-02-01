using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static bool InvalidName(string name)
        {
            if (name.IsNullOrEmpty())
                return true;
            if (name == "이름 추가")
                return true;


            return false;
        }

    }
}
