using NewMatchingBom.Commands;
using NewMatchingBom.Models;
using NewMatchingBom.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NewMatchingBom.ViewModels
{
    public class MatchingResultViewModel : ViewModelBase
    {
        private readonly IMatchingService _matchingService;
        private readonly ISpreadSheetService _spreadSheetService;
        private readonly ILoggingService _loggingService;
        
        private MatchResult? _currentMatchResult;

        public ObservableCollection<Match> Matches { get; } = new();
        public ObservableCollection<User> RemainUsers { get; } = new();

        public MatchResult? CurrentMatchResult
        {
            get => _currentMatchResult;
            set
            {
                if (SetProperty(ref _currentMatchResult, value))
                {
                    UpdateDisplayData();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ExportResultCommand { get; }
        public ICommand ResetMatchingCommand { get; }

        public MatchingResultViewModel(
            IMatchingService matchingService,
            ISpreadSheetService spreadSheetService,
            ILoggingService loggingService)
        {
            _matchingService = matchingService;
            _spreadSheetService = spreadSheetService;
            _loggingService = loggingService;

            RefreshCommand = new RelayCommand(RefreshData);
            ExportResultCommand = new RelayCommand(async () => await ExportResultAsync());
            ResetMatchingCommand = new RelayCommand(ResetMatching);

            RefreshData();
        }

        // ViewModel이 활성화될 때 호출되는 메서드
        public void OnNavigatedTo()
        {
            RefreshData();
        }

        private void RefreshData()
        {
            CurrentMatchResult = _matchingService.CurrentMatchResult;
        }

        private void UpdateDisplayData()
        {
            Matches.Clear();
            RemainUsers.Clear();

            if (CurrentMatchResult != null)
            {
                foreach (var match in CurrentMatchResult.Matches)
                {
                    Matches.Add(match);
                }

                foreach (var user in CurrentMatchResult.RemainUsers.NonMatchedUsers)
                {
                    RemainUsers.Add(user);
                }

                if (Matches.Count > 0)
                {
                    _loggingService.LogInfo($"매칭 결과 표시: {Matches.Count}개 게임");
                }
            }
        }

        private async Task ExportResultAsync()
        {
            if (CurrentMatchResult == null || !CurrentMatchResult.Matches.Any())
            {
                _loggingService.LogError("내보낼 매칭 결과가 없습니다.");
                return;
            }

            try
            {
                StringBuilder sb = new();
                sb.AppendLine("매칭 결과");
                sb.AppendLine("==================");

                for (int i = 0; i < CurrentMatchResult.Matches.Count; i++)
                {
                    var match = CurrentMatchResult.Matches[i];
                    sb.AppendLine($"\n게임 {i + 1}:");
                    sb.AppendLine($"블루팀:\n{match.BlueMember}");
                    sb.AppendLine($"레드팀:\n{match.RedMember}");
                    sb.AppendLine();
                }

                if (CurrentMatchResult.RemainUsers.NonMatchedUsers.Any())
                {
                    sb.AppendLine("남은 인원:");
                    sb.AppendLine(string.Join(", ", CurrentMatchResult.RemainUsers.NonMatchedUsers.Select(u => u.Name)));
                }

                await _spreadSheetService.ExportDataAsync(sb.ToString(), "MatchingResult.txt");
                _loggingService.LogInfo("매칭 결과 내보내기 완료");
            }
            catch (System.Exception ex)
            {
                _loggingService.LogError($"매칭 결과 내보내기 실패: {ex.Message}");
            }
        }

        private void ResetMatching()
        {
            _matchingService.Reset();
            CurrentMatchResult = null;
            _loggingService.LogInfo("매칭 결과 초기화 완료");
        }
    }
}