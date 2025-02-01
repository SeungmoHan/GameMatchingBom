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
        Iron,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Emerald,
        Diamond,
        Master,
        Challenger
    }
    public class User
    {
        public string Name { get; set; }
        public string NickName { get; set; }
        public UserTier Tier { get; set; } = UserTier.Invalid;
    }
    public class Team
    {
        public bool CanAddMember(int newMemberCount)
        {
            if (Users.Count + newMemberCount >= 5)
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

        public List<User> Users { get; set; } = new();
        public bool Valid { get; set; } = false;
    }

    public class Match
    { 
        public Team Red { get; set; } = new();
        public Team Blue { get; set; } = new();

        public string RedMember => string.Join(",", Red.Users.Select(u => u.Name));
        public string BlueMember => string.Join(",", Blue.Users.Select(u => u.Name));
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
        public static MatchResult GetMatches(List<User> nameList)
        {
            List<User> userList = new(nameList);
            Shuffle(userList);

            MatchResult result = new();
            while (true)
            {
                var match = GetMatch(userList);
                if (match == null || userList.Count < 10)
                    break;

                result.Matches.Add(match);
                OnMatchingUser(match, userList);
            }

            if (userList.Count != 0)
                result.RemainUsers.nonMatchedUser = new(userList);

            return result;
        }

        private static void OnMatchingUser(Match match, List<User> original)
        {
            foreach (var user in match.Blue.Users)
            {
                if (Util.HasName(user.Name, original))
                    Util.RemoveName(user.Name, ref original);
            }

            foreach (var user in match.Red.Users)
            {
                if (Util.HasName(user.Name, original))
                    Util.RemoveName(user.Name, ref original);
            }
        }

        private static void Shuffle(List<User> nameList)
        {
            Random rand = new Random();

            int n = nameList.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (nameList[i], nameList[j]) = (nameList[j], nameList[i]); // Swap
            }
        }

        private static Match? GetMatch(List<User> userList)
        {
            if (userList.Count < 10)
                return null;

            Match match = new();
            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                    match.Blue.Users.Add(userList[i]);
                else
                    match.Red.Users.Add(userList[i]);
            }

            if (match.Red.Users.Count != 5 || match.Blue.Users.Count != 5)
                return null;

            return match;
        }
    }
}
