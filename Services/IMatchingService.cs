using NewMatchingBom.Models;
using System.Collections.Generic;

namespace NewMatchingBom.Services
{
    public interface IMatchingService
    {
        MatchResult CreateMatches(List<User> users, int teamSize, bool useLine = false);
        void Reset();
        MatchResult? CurrentMatchResult { get; }
    }
}