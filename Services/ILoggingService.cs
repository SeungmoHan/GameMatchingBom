using System.Collections.ObjectModel;

namespace NewMatchingBom.Services
{
    public interface ILoggingService
    {
        ObservableCollection<string> LogEntries { get; }
        void LogInfo(string message);
        void LogError(string message);
        void Clear();
    }
}