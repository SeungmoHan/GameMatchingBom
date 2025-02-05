using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Documents;
using HandyControl.Interactivity;

namespace Test
{
    public enum UserTier
    {
        Invalid,
        LV1,
        LV2,
        LV3,
        LV4,
        LV5,
        LV6,
        LV7,
        LV8,
        LV9
    }

    public enum MainLine
    {
        All,
        탑,
        정글,
        미드,
        원딜,
        서폿
    }
    public class User
    {
        public string Name { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public UserTier Tier { get; set; } = UserTier.Invalid;
        public MainLine MainLine { get; set; } = MainLine.All;
    }
    public class Team
    {
        public bool CanAddMember(int newMemberCount)
        {
            if (Users.Count + newMemberCount > 5)
                return false;
            return true;
        }

        public bool AddMember(User name)
        {
            if (false == CanAddMember(1))
                return false;
            Users.Add(name);

            if (Users.Count >= 5)
                Valid = true;
            return true;
        }

        public int GetTeamTierPoint()
        {
            int ret = 0;
            foreach (User user in Users)
            {
                ret += (int)user.Tier;
            }
            return ret;
        }

        public void Shuffle()
        {
            Util.Shuffle(ref _users);
        }

        private List<User> _users = new();

        public List<User> Users
        {
            get => _users;
            set => _users = value;
        } 
        public bool Valid { get; set; } = false;
    }

    public class Match
    { 
        public Team Red { get; set; } = new();
        public Team Blue { get; set; } = new();

        public List<User> MatchUserList { get; set; } = new();
        public bool UseLine = false;

        public string RedMember  => string.Join("\n", Red.Users.Select((u, index)=> ($"{(UseLine ? $"[{(MainLine)index+1}]\t" : string.Empty)}{u.Name} [{u.Tier}]").Replace("LV","")));
        public string BlueMember => string.Join("\n", Blue.Users.Select((u, index) => ($"{(UseLine ? $"[{(MainLine)index + 1}]\t" : string.Empty)}{u.Name} [{u.Tier}]").Replace("LV", "")));

        public void MakeTeam(bool useLine)
        {
            Util.SortByTier(MatchUserList);
            for (int i = 0; i < MatchUserList.Count; i++)
            {
                int redTier = Red.GetTeamTierPoint();
                int blueTier = Blue.GetTeamTierPoint();

                // 블루팀이 점수가 더 많음
                if (redTier < blueTier)
                {
                    if (Red.CanAddMember(1))
                    {
                        Red.AddMember(MatchUserList[i]);
                    }
                    else
                    {
                        Blue.AddMember(MatchUserList[i]);
                    }
                }
                // 레드가 더 점수가 많음
                else
                {
                    if (Blue.CanAddMember(1))
                    {
                        Blue.AddMember(MatchUserList[i]);
                    }
                    else
                    {
                        Red.AddMember(MatchUserList[i]);
                    }
                }
            }

            UseLine = useLine;
            Red.Shuffle();
            Blue.Shuffle();
        }
    }

    public class Remains
    {
        public List<User> nonMatchedUser { get; set; } = new();
    }

    public class MatchResult
    {
        public List<Match> Matches { get; set; } = new();
        public Remains RemainUsers { get; set; } = new();
    }

    public class MatchMaker
    {
        public static MatchResult GetMatches(List<User> nameList, bool useLine)
        {
            // 1. 버려진 유저 먼저 구함
            List<User> userList = new(nameList);
            List<User> remove = new();
            Util.Shuffle(ref userList);

            var removeUserCount = (userList.Count % 10);
            if (removeUserCount != 0)
            {
                for (int i = 0; i < removeUserCount; i++)
                {
                    remove.Add(userList[i]);
                }
                userList.RemoveRange(0, removeUserCount);
            } 
            Util.SortByTier(userList);
            MatchResult result = new();
            result.RemainUsers.nonMatchedUser = remove;

            // 2. 매치 갯수만큼 매치 가져옴.
            int matchCount = userList.Count / 10;
            var temp = new List<Match>();

            for (int i = 0; i < matchCount; i++)
            {
                var match = GetMatch(userList, i, matchCount); 
                if (match == null)
                    break;

                match.MakeTeam(useLine);
                if (userList.Count < 10)
                    break;
                result.Matches.Add(match);
            }
            return result;
        }


        private static Match? GetMatch(List<User> userList, int currentMatchCnt, int totalMatchCount)
        {
            if (userList.Count < 10)
                return null;
            // 0 * 총매치수 + 현재 매치수
            Match match = new();
            for (int i = 0; i < 10; i++)
            {
                match.MatchUserList.Add(userList[i * totalMatchCount + currentMatchCnt]);
            }

            if (match.MatchUserList.Count != 10 )
                return null;

            return match;
        }
    }
}
