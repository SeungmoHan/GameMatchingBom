using System;

namespace NewMatchingBom.Services
{
    public interface INavigationService
    {
        event Action? NavigateToMatchingResult;
        void RequestNavigateToMatchingResult();
    }
}