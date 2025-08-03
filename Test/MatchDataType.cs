using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public enum MemberCount
    {
        _1명 = 1,
        _2명,
        _3명,
        _4명,
        _5명,
        _6명,
        _7명,
        _8명,
        _9명,
        _10명,
    }
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
        public string Tag { get; set; } = string.Empty;
        public MainLine MainLine { get; set; } = MainLine.All;
    }
    public class Team
    {
        public bool CanAddMember(int newMemberCount)
        {
            if (Users.Count + newMemberCount > TeamMemberCount)
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
        public int TeamMemberCount = 5;
    }

    public class Match
    {
        public Team Red { get; set; } = new();
        public Team Blue { get; set; } = new();

        public List<User> MatchUserList { get; set; } = new();
        public bool UseLine = false;

        public string RedMember => string.Join("\n", Red.Users.Select((u, index) => ($"{(UseLine ? $"[{(MainLine)index + 1}]\t" : string.Empty)}{u.Name} [{u.Tier}]").Replace("LV", "")));
        public string BlueMember => string.Join("\n", Blue.Users.Select((u, index) => ($"{(UseLine ? $"[{(MainLine)index + 1}]\t" : string.Empty)}{u.Name} [{u.Tier}]").Replace("LV", "")));

        public void MakeTeam(bool useLine, int teamMemberCount)
        {
            Util.SortByTier(MatchUserList);
            Red.TeamMemberCount = Blue.TeamMemberCount = teamMemberCount;
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
}
