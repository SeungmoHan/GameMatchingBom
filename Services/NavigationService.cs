using System;

namespace NewMatchingBom.Services
{
    public class NavigationService : INavigationService
    {
        public event Action? NavigateToMatchingResult;

        public void RequestNavigateToMatchingResult()
        {
            NavigateToMatchingResult?.Invoke();
        }
    }
}