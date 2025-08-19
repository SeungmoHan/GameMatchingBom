using NewMatchingBom.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewMatchingBom.Views
{
    public static class EnumHelper
    {
        public static IEnumerable<MemberCount> MemberCountValues =>
            Enum.GetValues(typeof(MemberCount)).Cast<MemberCount>();

        public static IEnumerable<UserTier> UserTierValues =>
            Enum.GetValues(typeof(UserTier)).Cast<UserTier>();

        public static IEnumerable<MainLine> MainLineValues =>
            Enum.GetValues(typeof(MainLine)).Cast<MainLine>();
    }
}