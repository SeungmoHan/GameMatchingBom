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
    public class MatchMaker
    {
        public static MatchResult GetMatches(List<User> nameList, bool useLine, int teamMemberCount)
        {
            // 1. 버려진 유저 먼저 구함
            List<User> userList = new(nameList);
            List<User> remove = new();
            Util.Shuffle(ref userList);

            var removeUserCount = (userList.Count % (teamMemberCount * 2));
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
            int matchCount = userList.Count / (teamMemberCount * 2);
            var temp = new List<Match>();

            for (int i = 0; i < matchCount; i++)
            {
                var match = GetMatch(userList, i, matchCount, (teamMemberCount * 2)); 
                if (match == null)
                    break;

                match.MakeTeam(useLine, teamMemberCount);
                result.Matches.Add(match);
            }
            return result;
        }


        private static Match? GetMatch(List<User> userList, int currentMatchCnt, int totalMatchCount, int totalTeamMemberCount)
        {
            // 0 * 총매치수 + 현재 매치수
            Match match = new();
            for (int i = 0; i < totalTeamMemberCount; i++)
            {
                match.MatchUserList.Add(userList[i * totalMatchCount + currentMatchCnt]);
            }

            if (match.MatchUserList.Count != totalTeamMemberCount)
                return null;

            return match;
        }
    }
}
