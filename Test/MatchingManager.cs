using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class MatchingManager
    {
        public static MatchingManager Instance = new();

        public MemberLoader MemberLoader = new();

        public List<User> CurrentUsers = new();

        public bool UseLineInfo { get; set; } = false;
        public int MatchingMemberCount { get; set; } = 5;

        public void Reset()
        {
            lastMatchResult = null;
        }

        public MatchResult CreateMatchResult()
        {
            lastMatchResult = MatchMaker.GetMatches(CurrentUsers, UseLineInfo, MatchingMemberCount);
            return lastMatchResult;
        }

        public MatchResult? GetLastMatchResult()
        {
            return lastMatchResult;
        }

        public MatchResult? lastMatchResult = null;

        public void Init()
        {
            MemberLoader.InitMemberHttp();
        }

        public bool InsertUser(User user)
        {
            return MemberLoader.AddMemberList(user);
        }

        public bool RemoveUser(User user)
        {
            return MemberLoader.RemoveMemberList(user);
        }

        public void AddCurrentUser(User user)
        {
            int idx = CurrentUsers.FindIndex(x => x.Name == user.Name);
            if (idx != -1)
                return;
            CurrentUsers.Add(user);
        }

        public void RemoveCurrentUser(User user)
        {
            int idx = CurrentUsers.FindIndex(x => x.Name == user.Name);
            if (idx == -1)
                return;
            CurrentUsers.RemoveAt(idx);
        }

        public void Save()
        {
            MemberLoader.Save();
        }
}
}
