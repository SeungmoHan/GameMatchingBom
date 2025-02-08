using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Test
{
    public class MemberLoader
    {
        public List<User> MemberList = new();

        public void InitMemberHttp()
        {
            using HttpClient client = new HttpClient();
            try
            {
                string url = "https://perfect-somersault-b5c.notion.site/9faec2a686334048a762afd011a968b3";
                string html = client.GetStringAsync(url).Result;

                int beginIdx = html.IndexOf("BeginUserList") + "BeginUserList".Length + 2;
                int endIdx = html.IndexOf("EndUserList");

                int stringLength = endIdx - beginIdx;

                string temp = html.Substring(beginIdx, stringLength);
                temp = temp.Replace("\\n", "\n");
                temp = temp.Replace("\\t", "");
                temp = temp.Replace("\\", "");
                JObject jObj = JObject.Parse(temp);

                var userListJson = jObj["MatchingBomUserList"];
                foreach (var token in userListJson)
                {
                    User newUser = new();
                    newUser.Name = token["nickname"].ToString();
                    newUser.NickName = token["username"].ToString();
                    newUser.Tier = (UserTier)token["level"].ToObject<int>();
                    newUser.MainLine = (MainLine)Enum.Parse(typeof(MainLine),token["mainline"].ToString());

                    MemberList.Add(newUser);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string SerializeMemberData()
        {
            List<string> userList = new();
            foreach (var user in MemberList)
            {
                userList.Add($"\t\t{{ \"username\": \"{user.NickName}\", \"nickname\": \"{user.Name}\", \"level\": \"{(int)user.Tier}\", \"mainline\": \"{user.MainLine}\" }}");
            }

            StringBuilder sb = new();
            string temp = string.Join(",\n", userList);
            
            sb.AppendLine("BeginUserList");
            sb.AppendLine("{");
            sb.AppendLine("\t\"MatchingBomUserList\":");
            sb.AppendLine("\t[");
            sb.AppendLine(temp);
            sb.AppendLine("\t]");
            sb.AppendLine("}");
            sb.AppendLine("EndUserList");

            return sb.ToString();
        }

        public void Reload()
        {
            MemberList.Clear();
            InitMemberHttp();
        }

        public bool AddMemberList(User user)
        {
            if (true == Util.HasName(user.Name, MemberList))
                return false;

            Util.AddNew(user, ref MemberList);

            Save();
            return true;
        }

        public bool RemoveMemberList(User user)
        {
            if (false == Util.HasName(user.Name, MemberList))
                return false;

            Util.RemoveName(user.Name, ref MemberList);

            Save();
            return true;
        }

        public void Save()
        {
            var str = SerializeMemberData();

            Util.WriteToFileAndOpen(str);
        }
    }
}
