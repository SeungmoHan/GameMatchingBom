using NewMatchingBom.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewMatchingBom.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly ILoggingService _loggingService;
        
        public MatchResult? CurrentMatchResult { get; private set; }

        public MatchingService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public MatchResult CreateMatches(List<User> nameList, int teamSize, bool useLine = false)
        {
            _loggingService.LogInfo($"매칭 시작 - 참가자: {nameList.Count}명, 팀 크기: {teamSize}명");

            // 1. 참가자 리스트 복사 및 제외될 인원 계산
            List<User> userList = new(nameList);
            List<User> remove = new();
            Shuffle(userList);

            var totalPlayersPerMatch = teamSize * 2;
            var removeUserCount = userList.Count % totalPlayersPerMatch;
            
            if (removeUserCount != 0)
            {
                // GetRange로 한번에 제거할 인원 추출 (성능 향상)
                remove = userList.GetRange(0, removeUserCount);
                userList.RemoveRange(0, removeUserCount);
                _loggingService.LogInfo($"남은 인원: {string.Join(", ", remove.Select(u => u.Name))}");
            }

            SortByTier(userList);
            MatchResult result = new() { RemainUsers = { NonMatchedUsers = remove } };

            // 2. 매치 생성 (반복 최적화)
            int matchCount = userList.Count / totalPlayersPerMatch;
            result.Matches.Capacity = matchCount; // 컬렉션 크기 미리 할당
            
            for (int i = 0; i < matchCount; i++)
            {
                var match = GetMatch(userList, i, matchCount, totalPlayersPerMatch);
                if (match == null)
                    break;

                match.MakeTeam(useLine, teamSize);
                result.Matches.Add(match);
                
                _loggingService.LogInfo($"매치 {i + 1} 생성 완료");
            }

            CurrentMatchResult = result;
            _loggingService.LogInfo($"총 {result.Matches.Count}개 매치 생성 완료");
            
            return result;
        }

        public void Reset()
        {
            CurrentMatchResult = null;
            _loggingService.LogInfo("매칭 결과 초기화");
        }

        private static Match? GetMatch(List<User> userList, int currentMatchCnt, int totalMatchCount, int totalTeamMemberCount)
        {
            Match match = new();
            for (int i = 0; i < totalTeamMemberCount; i++)
            {
                match.MatchUserList.Add(userList[i * totalMatchCount + currentMatchCnt]);
            }

            if (match.MatchUserList.Count != totalTeamMemberCount)
                return null;

            return match;
        }

        private static void Shuffle(List<User> list)
        {
            // Fisher-Yates 알고리즘 최적화 (Random 객체 재사용)
            var random = new Random(Environment.TickCount);
            int n = list.Count;
            
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        private static void SortByTier(List<User> list)
        {
            list.Sort((x, y) => y.Tier.CompareTo(x.Tier));
        }
    }
}