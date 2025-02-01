using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Test
{
    public class MemberLoader
    {
        private List<User> memberList = new();
        public List<User> MemberList => memberList;

        public void Init()
        {
            string filePath = "MemberList.json";

            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);

                List<User>? users = JsonConvert.DeserializeObject<List<User>>(jsonContent);
                if (null != users)
                {
                    memberList = users;
                }
            }
        }
    }
}
