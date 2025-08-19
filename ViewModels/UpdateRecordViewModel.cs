using NewMatchingBom.Commands;
using NewMatchingBom.Models;
using NewMatchingBom.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NewMatchingBom.ViewModels
{
    public class UpdateRecordViewModel : ViewModelBase
    {
        private readonly IMemberService _memberService;
        private readonly ISpreadSheetService _spreadSheetService;
        private readonly ILoggingService _loggingService;
        
        private string _selectedGameType = "내전";
        private string _selectedDate = DateTime.Today.ToString("yyyy-MM-dd");

        public ObservableCollection<User> Members => _memberService.AllMembers;
        public ObservableCollection<User> SelectedMembersForRecord => _memberService.SelectedMembers;
        public ObservableCollection<RecordViewData> RankingData { get; } = new();

        public string SelectedGameType
        {
            get => _selectedGameType;
            set => SetProperty(ref _selectedGameType, value);
        }

        public string SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        public string[] GameTypes { get; } = { "내전", "자랭", "칼바람" };

        public ICommand AddRecordCommand { get; }
        public ICommand RefreshRankingCommand { get; }
        public ICommand ExportRankingCommand { get; }
        public ICommand LoadFromGoogleSheetsCommand { get; }
        public ICommand ShowMemberDetailsCommand { get; }

        public UpdateRecordViewModel(
            IMemberService memberService,
            ISpreadSheetService spreadSheetService,
            ILoggingService loggingService)
        {
            _memberService = memberService;
            _spreadSheetService = spreadSheetService;
            _loggingService = loggingService;

            AddRecordCommand = new RelayCommand(AddRecord, CanAddRecord);
            RefreshRankingCommand = new RelayCommand(RefreshRanking);
            ExportRankingCommand = new RelayCommand(async () => await ExportRankingAsync());
            LoadFromGoogleSheetsCommand = new RelayCommand(async () => await LoadFromGoogleSheetsAsync());
            ShowMemberDetailsCommand = new RelayCommand<RecordViewData>(ShowMemberDetails);

            // 멤버 데이터 변경 감지를 위한 이벤트 구독
            _memberService.AllMembers.CollectionChanged += (s, e) =>
            {
                // UI 스레드에서 랭킹 갱신
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    RefreshRanking();
                    _loggingService.LogInfo("멤버 데이터 변경 감지 - 랭킹 데이터 갱신 완료");
                });
            };
        }

        public async Task OnNavigatedToAsync()
        {
            try
            {
                _loggingService.LogInfo("멤버 점수 탭 진입 - 데이터 자동 로드 시작");
                
                // 1. Google Sheet로부터 PlayRecord 정보 로드
                _loggingService.LogInfo("1. 구글 시트에서 플레이 기록 로딩 중...");
                var records = await _spreadSheetService.LoadPlayRecordsFromSpreadSheetAsync();
                
                // 기존 기록과 중복 체크하여 새 기록만 추가
                var existingRecords = await _memberService.LoadPlayRecords();
                var newRecords = records.Where(r => !existingRecords.Any(er => 
                    er.Name == r.Name && er.Date == r.Date && er.Type == r.Type)).ToList();
                
                foreach (var record in newRecords)
                {
                    _memberService.SavePlayRecord(record);
                }
                _loggingService.LogInfo($"플레이 기록 로딩 완료: {newRecords.Count}개 새 기록");
                
                // 2. 캐싱된 PlayRecord를 기반으로 Ranking 정보 갱신
                _loggingService.LogInfo("2. 캐싱된 데이터로 랭킹 갱신 중...");
                RefreshRanking();
                _loggingService.LogInfo("랭킹 데이터 갱신 완료");
                
                _loggingService.LogInfo("멤버 점수 탭 데이터 로드 완료");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"멤버 점수 탭 데이터 로드 실패: {ex.Message}");
            }
        }

        private bool CanAddRecord()
        {
            return SelectedMembersForRecord.Count > 0 && 
                   !string.IsNullOrWhiteSpace(SelectedGameType) && 
                   !string.IsNullOrWhiteSpace(SelectedDate);
        }

        private void AddRecord()
        {
            if (!CanAddRecord())
            {
                _loggingService.LogError("모든 필드를 올바르게 입력해주세요.");
                return;
            }

            try
            {
                // 텍스트 파일 내용 생성
                var sb = new StringBuilder();
                var addedCount = 0;

                foreach (var member in SelectedMembersForRecord.ToList())
                {
                    var record = new PlayRecord
                    {
                        Name = member.Name,
                        Date = SelectedDate,
                        Type = SelectedGameType,
                        Point = GetPointByType(SelectedGameType)
                    };

                    // 데이터베이스에 저장
                    _memberService.SavePlayRecord(record);
                    
                    // 텍스트 파일 내용에 추가
                    sb.AppendLine($"{record.Name}\t{record.Type}\t{record.Date}");
                    addedCount++;
                }

                // 데스크톱에 텍스트 파일 저장
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var fileName = "PlayRecord.txt";
                var filePath = Path.Combine(desktopPath, fileName);

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                // 파일 실행 (메모장으로 열기)
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });

                RefreshRanking();
                
                _loggingService.LogInfo($"기록 추가 완료: {addedCount}명 - {SelectedGameType} - 파일 저장: {fileName}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"기록 추가 실패: {ex.Message}");
            }
        }

        private void RefreshRanking()
        {
            RankingData.Clear();
            var ranking = _memberService.GetRankingData();
            
            foreach (var data in ranking)
            {
                RankingData.Add(data);
            }
            
            _loggingService.LogInfo($"랭킹 데이터 새로고침: {RankingData.Count}명");
        }

        private async Task ExportRankingAsync()
        {
            try
            {
                StringBuilder sb = new();
                sb.AppendLine("순위\t이름\t총점\t등급");
                
                foreach (var data in RankingData)
                {
                    sb.AppendLine($"{data.Rank}\t{data.Name}\t{data.TotalPoint}");
                }

                await _spreadSheetService.ExportDataAsync(sb.ToString(), "Ranking.txt");
                _loggingService.LogInfo("랭킹 데이터 내보내기 완료");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"랭킹 데이터 내보내기 실패: {ex.Message}");
            }
        }

        private async Task LoadFromGoogleSheetsAsync()
        {
            try
            {
                _loggingService.LogInfo("플레이 정보 새로고침 시작...");
                
                // 1. Google Sheet로부터 PlayRecord 정보 로드
                _loggingService.LogInfo("1. 구글 시트에서 플레이 기록 로딩 중...");
                var records = await _spreadSheetService.LoadPlayRecordsFromSpreadSheetAsync();

                // 기존 기록과 중복 체크하여 새 기록만 추가
                var existingRecords = await _memberService.LoadPlayRecords();
                var newRecords = records.Where(r => !existingRecords.Any(er =>
                    er.Name == r.Name && er.Date == r.Date && er.Type == r.Type)).ToList();

                foreach (var record in newRecords)
                {
                    _memberService.SavePlayRecord(record);
                }
                // 2. 캐싱된 PlayRecord를 기반으로 Ranking 정보 갱신
                _loggingService.LogInfo("2. 캐싱된 데이터로 랭킹 갱신 중...");
                RefreshRanking();
                _loggingService.LogInfo("랭킹 데이터 갱신 완료");
                
                _loggingService.LogInfo("플레이 정보 새로고침 모든 과정 완료");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"플레이 정보 새로고침 실패: {ex.Message}");
            }
        }

        private void ShowMemberDetails(RecordViewData? recordData)
        {
            if (recordData == null) return;

            try
            {
                _loggingService.LogInfo($"{recordData.Name}의 상세 기록 창 열기");
                
                // MemberDetailViewModel 생성
                var memberDetailViewModel = new MemberDetailViewModel(_memberService, _loggingService);
                
                // MemberDetailWindow 생성 및 표시
                var detailWindow = new Views.MemberDetailWindow(memberDetailViewModel);
                
                // 멤버 상세 데이터 로드
                memberDetailViewModel.LoadMemberDetails(recordData.Name);
                
                // 모달 창으로 표시
                detailWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"멤버 상세 창 열기 실패: {ex.Message}");
            }
        }

        private static int GetPointByType(string type)
        {
            return type switch
            {
                "내전" => 3,
                "자랭" or "칼바람" => 1,
                _ => 0
            };
        }
    }
}