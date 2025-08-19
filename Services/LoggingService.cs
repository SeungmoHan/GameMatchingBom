using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace NewMatchingBom.Services
{
    public class LoggingService : ILoggingService
    {
        private ObservableCollection<string> _logEntries = new();
        public ObservableCollection<string> LogEntries => _logEntries;

        public void LogInfo(string message)
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _logEntries.Insert(0, message);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => {
                    _logEntries.Insert(0, message);
                });
            }
        }

        public void LogError(string message)
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss}] ERROR: {message}";

            if (Application.Current.Dispatcher.CheckAccess())
            {
                _logEntries.Insert(0, message);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => {
                    _logEntries.Insert(0, message);
                });
            }
        }

        public void Clear()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _logEntries.Clear();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => _logEntries.Clear());
            }
        }
    }
}