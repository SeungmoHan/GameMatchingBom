using NewMatchingBom.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NewMatchingBom.Services
{
    public class MemberService : IMemberService
    {
        private readonly ILoggingService _loggingService;
        private readonly ISpreadSheetService _spreadSheetService;
        private const string MemberListFileName = "MemberList.json";
        private const string PlayRecordsFileName = "PlayRecords.json";

        // 캐시된 플레이 기록 데이터
        private List<PlayRecord>? _cachedPlayRecords;
        private DateTime _playRecordsCacheTime;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(2);

        public ObservableCollection<User> AllMembers { get; } = new();
        public ObservableCollection<User> SelectedMembers { get; } = new();

        public MemberService(ILoggingService loggingService, ISpreadSheetService spreadSheetService)
        {
            _loggingService = loggingService;
            _spreadSheetService = spreadSheetService;
        }

        public void InitializeAsync()
        {
            _loggingService.LogInfo("멤버 서비스 초기화 시작...");
            
            // 백그라운드에서 Google Sheets 데이터 로드 (순서대로)
            _ = Task.Run(async () =>
            {
                try
                {
                    // 1. Google Sheet로부터 멤버정보 로드
                    _loggingService.LogInfo("1. 구글 시트에서 멤버 정보 로딩 중...");
                    await LoadFromGoogleSheetsAsync();
                    _loggingService.LogInfo("멤버 정보 로딩 완료");
                    
                    // 2. Google Sheet로부터 PlayRecord 로드
                    _loggingService.LogInfo("2. 구글 시트에서 플레이 기록 로딩 중...");
                    var records = await _spreadSheetService.LoadPlayRecordsFromSpreadSheetAsync();
                    
                    // 기존 기록과 중복 체크하여 새 기록만 추가
                    var existingRecords = await LoadPlayRecords();
                    var newRecords = records.Where(r => !existingRecords.Any(er => 
                        er.Name == r.Name && er.Date == r.Date && er.Type == r.Type)).ToList();
                    
                    foreach (var record in newRecords)
                    {
                        SavePlayRecord(record);
                    }
                    _loggingService.LogInfo($"플레이 기록 로딩 완료: {newRecords.Count}개 새 기록");
                    
                    // 3. 캐시 갱신 완료
                    _loggingService.LogInfo("3. 초기 데이터 로딩 모든 과정 완료");
                }
                catch (Exception ex)
                {
                    _loggingService.LogError($"백그라운드 데이터 로딩 실패: {ex.Message}");
                }
            });
            
            _loggingService.LogInfo("멤버 서비스 초기화 완료 - 백그라운드 로딩 시작됨");
        }

        public async Task LoadMembersAsync()
        {
            // Google Sheets에서만 로드
            await LoadFromGoogleSheetsAsync();
        }

        public async Task LoadFromGoogleSheetsAsync()
        {
            try
            {
                var members = await _spreadSheetService.LoadUsersFromSpreadSheetAsync();
                
                AllMembers.Clear();
                foreach (var member in members)
                {
                    AllMembers.Add(member);
                }
                
                _loggingService.LogInfo($"구글 시트에서 멤버 로딩 완료: {AllMembers.Count}명");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"구글 시트 멤버 로딩 실패: {ex.Message}");
            }
        }

        public void AddMember(User user)
        {
            if (AllMembers.Any(m => m.Name == user.Name))
            {
                _loggingService.LogError($"이미 존재하는 멤버입니다: {user.Name}");
                return;
            }

            AllMembers.Add(user);
            _loggingService.LogInfo($"멤버 추가: {user.Name}");
        }

        public void RemoveMember(User user)
        {
            AllMembers.Remove(user);
            SelectedMembers.Remove(user);
            _loggingService.LogInfo($"멤버 제거: {user.Name}");
        }

        public void AddToSelected(User user)
        {
            // Contains 대신 직접 체크로 성능 향상
            bool alreadyExists = false;
            foreach (var member in SelectedMembers)
            {
                if (ReferenceEquals(member, user) || member.Name == user.Name)
                {
                    alreadyExists = true;
                    break;
                }
            }
            
            if (!alreadyExists)
            {
                SelectedMembers.Add(user);
                _loggingService.LogInfo($"매칭 대상 추가: {user.Name}");
            }
        }

        public void RemoveFromSelected(User user)
        {
            SelectedMembers.Remove(user);
            _loggingService.LogInfo($"매칭 대상 제거: {user.Name}");
        }

        public void ClearSelected()
        {
            SelectedMembers.Clear();
            _loggingService.LogInfo("매칭 대상 초기화");
        }

        public async Task SaveMembersAsync()
        {
            try
            {
                var json = JsonConvert.SerializeObject(AllMembers.ToList(), Formatting.Indented);
                await File.WriteAllTextAsync(MemberListFileName, json);
                _loggingService.LogInfo("멤버 목록 저장 완료");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"멤버 목록 저장 실패: {ex.Message}");
            }
        }

        public async Task<List<PlayRecord>> LoadPlayRecords()
        {
            try
            {
                // Google Sheets에서 PlayRecord 로드 (타임아웃 설정)
                var task = Task.Run(async () => await _spreadSheetService.LoadPlayRecordsFromSpreadSheetAsync());
                await task;
                var records = task.Result;
                // 캐시 업데이트
                _cachedPlayRecords = records;
                _playRecordsCacheTime = DateTime.Now;

                return records;
            }
            catch (TaskCanceledException)
            {
                _loggingService.LogError("플레이 기록 로딩이 시간 초과되었습니다. 캐시된 데이터를 사용합니다.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"구글 시트에서 플레이 기록 로딩 실패: {ex.Message}");
            }

            // 오류 발생 시 빈 목록 반환
            var emptyRecords = new List<PlayRecord>();
            _cachedPlayRecords = emptyRecords;
            _playRecordsCacheTime = DateTime.Now;
            return emptyRecords;
        }

        public void SavePlayRecord(PlayRecord record)
        {
            
        }

        public List<RecordViewData> GetRankingData()
        {
            // 캐싱된 데이터만 사용 (Google Sheets에서 매번 로드하지 않음)
            var records = _cachedPlayRecords ?? new List<PlayRecord>();
            
            // Dictionary를 사용하여 그룹핑 성능 향상
            var pointsByName = new Dictionary<string, int>();
            
            foreach (var record in records)
            {
                if (pointsByName.TryGetValue(record.Name, out var currentPoints))
                {
                    pointsByName[record.Name] = currentPoints + record.Point;
                }
                else
                {
                    pointsByName[record.Name] = record.Point;
                }
            }
            
            var grouped = pointsByName.Select(kvp => new RecordViewData
                {
                    Name = kvp.Key,
                    TotalPoint = kvp.Value
                })
                .OrderByDescending(r => r.TotalPoint)
                .ToList();

            // 순위 매기기
            for (int i = 0; i < grouped.Count; i++)
            {
                grouped[i].Rank = i + 1;
            }

            return grouped;
        }
    }
}