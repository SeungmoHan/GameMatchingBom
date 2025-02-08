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

        public MatchResult MatchResult
        {
            get
            {
                lastMatchResult = UseLineInfo == true ? MatchResultWithLine : MatchResultWithoutLine;
                return lastMatchResult;
            }
        }

        public void Reset()
        {
            lastMatchResult = null;
        }

        public MatchResult? lastMatchResult = null;

        private MatchResult MatchResultWithLine => MatchMaker.GetMatches(CurrentUsers, true, MatchingMemberCount);
        private MatchResult MatchResultWithoutLine => MatchMaker.GetMatches(CurrentUsers, false, MatchingMemberCount);

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
