using System;
using System.Collections.Generic;
using System.Linq;

namespace NewMatchingBom.Models
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

        public override string ToString()
        {
            return $"{Name} [{Tier}]";
        }
    }

    public class Team
    {
        private List<User> _users = new();

        public List<User> Users
        {
            get => _users;
            set => _users = value;
        }

        public bool Valid { get; set; } = false;
        public int TeamMemberCount { get; set; } = 5;

        public bool CanAddMember(int newMemberCount)
        {
            return Users.Count + newMemberCount <= TeamMemberCount;
        }

        public bool AddMember(User user)
        {
            if (!CanAddMember(1))
                return false;

            Users.Add(user);

            if (Users.Count >= 5)
                Valid = true;

            return true;
        }

        public int GetTeamTierPoint()
        {
            return Users.Sum(user => (int)user.Tier);
        }

        public void Shuffle()
        {
            var random = new Random();
            for (int i = _users.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_users[i], _users[j]) = (_users[j], _users[i]);
            }
        }
    }

    public class Match
    {
        public Team Team1_Red { get; set; } = new();
        public Team Team2_Red { get; set; } = new();
        public List<User> MatchUserList { get; set; } = new();
        public bool UseLine { get; set; } = false;

        public string RedMember => string.Join("\n", 
            Team2_Red.Users.Select((u, index) => 
                $"{(UseLine ? $"[{(MainLine)index + 1}]\t" : string.Empty)}{u.Name} [{u.Tier}]"
                    .Replace("LV", "")));

        public string BlueMember => string.Join("\n", 
            Team1_Red.Users.Select((u, index) => 
                $"{(UseLine ? $"[{(MainLine)index + 1}]\t" : string.Empty)}{u.Name} [{u.Tier}]"
                    .Replace("LV", "")));

        public void MakeTeam(bool useLine, int teamMemberCount)
        {
            // Sort by tier
            MatchUserList = MatchUserList.OrderByDescending(u => (int)u.Tier).ToList();
            
            Team2_Red.TeamMemberCount = Team1_Red.TeamMemberCount = teamMemberCount;

            for (int i = 0; i < MatchUserList.Count; i++)
            {
                int redTier = Team2_Red.GetTeamTierPoint();
                int blueTier = Team1_Red.GetTeamTierPoint();

                // 블루팀이 점수가 더 많음
                if (redTier < blueTier)
                {
                    if (Team2_Red.CanAddMember(1))
                    {
                        Team2_Red.AddMember(MatchUserList[i]);
                    }
                    else
                    {
                        Team1_Red.AddMember(MatchUserList[i]);
                    }
                }
                // 레드가 더 점수가 많음
                else
                {
                    if (Team1_Red.CanAddMember(1))
                    {
                        Team1_Red.AddMember(MatchUserList[i]);
                    }
                    else
                    {
                        Team2_Red.AddMember(MatchUserList[i]);
                    }
                }
            }

            UseLine = useLine;
            Team2_Red.Shuffle();
            Team1_Red.Shuffle();
        }
    }

    public class Remains
    {
        public List<User> NonMatchedUsers { get; set; } = new();
    }

    public class MatchResult
    {
        public List<Match> Matches { get; set; } = new();
        public Remains RemainUsers { get; set; } = new();
    }
}