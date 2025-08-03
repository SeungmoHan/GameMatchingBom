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
                var result = SpreadSheet.SpreadSheetLoader.LoadSheetData("1twtASQQqxml0G8jZ20fKQaaJ89Ih0IFFuuhFJBzrg1w", "1612294575");

                bool readComment = false;
                bool readColumnName = false;
                foreach (var row in result)
                {
                    if (readComment == false)
                    {
                        readComment = true;
                        continue;
                    }
                    if (readColumnName == false)
                    {
                        readColumnName = true;
                        continue;
                    }

                    User newUser = new();
                    newUser.Name = row[0];
                    newUser.NickName = row[1];
                    newUser.Tier = (UserTier)Enum.Parse(typeof(UserTier), row[2]);
                    newUser.Tag = row[3] == "null" ? string.Empty : row[3];
                    newUser.MainLine = (MainLine)Enum.Parse(typeof(MainLine), row[4]);
                    MemberList.Add(newUser);
                }
                //string url = "https://perfect-somersault-b5c.notion.site/9faec2a686334048a762afd011a968b3";
                //string html = client.GetStringAsync(url).Result;

                //int beginIdx = html.IndexOf("BeginUserList") + "BeginUserList".Length + 2;
                //int endIdx = html.IndexOf("EndUserList");

                //int stringLength = endIdx - beginIdx;

                //string temp = html.Substring(beginIdx, stringLength);
                //temp = temp.Replace("\\n", "\n");
                //temp = temp.Replace("\\t", "");
                //temp = temp.Replace("\\", "");
                //JObject jObj = JObject.Parse(temp);

                //var userListJson = jObj["MatchingBomUserList"];
                //foreach (var token in userListJson)
                //{
                //    User newUser = new();
                //    newUser.Name = token["nickname"].ToString();
                //    newUser.NickName = token["username"].ToString();
                //    newUser.Tier = (UserTier)token["level"].ToObject<int>();
                //    newUser.MainLine = (MainLine)Enum.Parse(typeof(MainLine),token["mainline"].ToString());

                //    MemberList.Add(newUser);
                //}
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

            return true;
        }
    }
}
