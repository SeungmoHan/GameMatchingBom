using NewMatchingBom.Commands;
using NewMatchingBom.Models;
using NewMatchingBom.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace NewMatchingBom.ViewModels
{
    public class MemberDetailViewModel : ViewModelBase
    {
        private readonly IMemberService _memberService;
        private readonly ILoggingService _loggingService;
        
        private string _memberName = string.Empty;
        private int _totalGames = 0;
        private int _totalPoints = 0;

        public string MemberName
        {
            get => _memberName;
            set => SetProperty(ref _memberName, value);
        }

        public int TotalGames
        {
            get => _totalGames;
            set => SetProperty(ref _totalGames, value);
        }

        public int TotalPoints
        {
            get => _totalPoints;
            set => SetProperty(ref _totalPoints, value);
        }

        public ObservableCollection<PlayRecord> MemberRecords { get; } = new();

        public ICommand CloseCommand { get; }

        public MemberDetailViewModel(IMemberService memberService, ILoggingService loggingService)
        {
            _memberService = memberService;
            _loggingService = loggingService;
            
            CloseCommand = new RelayCommand<System.Windows.Window>(CloseWindow);
        }

        public async void LoadMemberDetails(string memberName)
        {
            try
            {
                MemberName = memberName;
                _loggingService.LogInfo($"{memberName}의 상세 기록 로딩 중...");

                // 캐시된 플레이 기록에서 해당 멤버의 기록만 필터링
                var allRecords = await _memberService.LoadPlayRecords();
                var memberRecords = allRecords.Where(r => r.Name == memberName).OrderByDescending(r => r.Date).ToList();

                MemberRecords.Clear();
                foreach (var record in memberRecords)
                {
                    MemberRecords.Add(record);
                }

                // 같은 날짜별 최고 점수만 반영하여 통계 계산
                var maxPointsByDate = memberRecords
                    .GroupBy(r => r.Date)
                    .Select(g => g.OrderByDescending(r => r.Point).First())
                    .ToList();
                
                TotalGames = maxPointsByDate.Count;
                TotalPoints = maxPointsByDate.Sum(r => r.Point);

                _loggingService.LogInfo($"{memberName} 기록 로딩 완료: {TotalGames}게임, {TotalPoints}점");
            }
            catch (System.Exception ex)
            {
                _loggingService.LogError($"멤버 상세 기록 로딩 실패: {ex.Message}");
            }
        }

        private void CloseWindow(System.Windows.Window? window)
        {
            window?.Close();
        }
    }
}