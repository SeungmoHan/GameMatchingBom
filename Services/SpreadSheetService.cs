using NewMatchingBom.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewMatchingBom.Services
{
    public class SpreadSheetService : ISpreadSheetService
    {
        private readonly ILoggingService _loggingService;
        private const string MemberSheetId = "1twtASQQqxml0G8jZ20fKQaaJ89Ih0IFFuuhFJBzrg1w";
        private const string MemberSheetGid = "1612294575";
        private const string RecordSheetGid = "0";

        public SpreadSheetService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task<List<User>> LoadUsersFromSpreadSheetAsync()
        {
            var users = new List<User>();
            
            try
            {
                _loggingService.LogInfo("구글 시트에서 멤버 데이터 로딩 중...");
                var data = await LoadSheetDataAsync(MemberSheetId, MemberSheetGid);
                
                if (data.Count == 0)
                {
                    _loggingService.LogError("구글시트에서 데이터를 가져오지 못했습니다. 시트가 공개 상태인지 확인해주세요.");
                    return users;
                }

                _loggingService.LogInfo($"첫 번째 행 데이터 확인: {string.Join(", ", data[0])}");
                
                // 첫 번째 행은 헤더이므로 제외
                for (int i = 0; i < data.Count; i++)
                {
                    var row = data[i];
                    _loggingService.LogInfo($"행 {i}: {string.Join(", ", row)} (컬럼 수: {row.Count})");
                    
                    if (row.Count >= 1 && !string.IsNullOrWhiteSpace(row[0]))
                    {
                        var user = new User
                        {
                            Name = row[0].Trim().Replace("\"", ""),
                            NickName = row.Count > 1 ? row[1].Trim().Replace("\"", "") : "",
                            Tier = ParseTier(row.Count > 2 ? row[2].Trim().Replace("\"", "") : ""),
                            Tag = row.Count > 3 ? row[3].Trim().Replace("\"", "") : "",
                            MainLine = ParseMainLine(row.Count > 4 ? row[4].Trim().Replace("\"", "") : "")
                        };
                        users.Add(user);
                        _loggingService.LogInfo($"멤버 추가: {user.Name} [{user.Tier}] {user.MainLine}");
                    }
                }
                
                _loggingService.LogInfo($"멤버 데이터 로딩 완료: {users.Count}명");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"멤버 데이터 로딩 실패: {ex.Message}");
                _loggingService.LogError($"스택 트레이스: {ex.StackTrace}");
            }
            
            return users;
        }

        public async Task<List<PlayRecord>> LoadPlayRecordsFromSpreadSheetAsync()
        {
            var records = new List<PlayRecord>();
            
            try
            {
                var data = await LoadSheetDataAsync(MemberSheetId, RecordSheetGid);
                
                // 첫 번째 행은 헤더이므로 제외
                for (int i = 0; i < data.Count; i++)
                {
                    var row = data[i];
                    if (row.Count >= 3 && !string.IsNullOrWhiteSpace(row[0]))
                    {
                        var record = new PlayRecord
                        {
                            Name = row[0].Trim(),
                            Type = row.Count > 1 ? row[1].Trim() : "",
                            Date = row.Count > 2 ? row[2].Trim() : "",
                            Point = GetPointByType(row.Count > 1 ? row[1].Trim() : "")
                        };
                        records.Add(record);
                    }
                }
                
                _loggingService.LogInfo($"플레이 기록 로딩 완료: {records.Count}개");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"플레이 기록 로딩 실패: {ex.Message}");
            }
            
            return records;
        }

        public async Task SavePlayRecordToSpreadSheetAsync(PlayRecord record)
        {
            try
            {
                // 현재는 구글 시트에 직접 쓰기가 구현되지 않았으므로 로그만 남김
                _loggingService.LogInfo($"구글 시트 저장 요청: {record.Name} - {record.Type} - {record.Date}");
                
                // TODO: Google Sheets API를 사용하여 실제 데이터를 시트에 추가하는 로직 구현
                // 현재는 시뮬레이션으로 성공했다고 가정
                await Task.Delay(100); // 네트워크 지연 시뮬레이션
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"구글 시트 저장 실패: {ex.Message}");
                throw;
            }
        }

        public async Task ExportDataAsync(string data, string fileName)
        {
            try
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string directoryPath = Path.Combine(userProfile, "Documents", "GameMatchingBom");
                string filePath = Path.Combine(directoryPath, fileName);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string content = $"export datetime {DateTime.Now}\n{data}";
                await File.WriteAllTextAsync(filePath, content);

                _loggingService.LogInfo($"데이터 내보내기 완료: {fileName}");

                // 파일 열기
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = filePath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"데이터 내보내기 실패: {ex.Message}");
            }
        }

        private async Task<List<List<string>>> LoadSheetDataAsync(string sheetId, string sheetGid)
        {
            string sheetCsvUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={sheetGid}";
            var result = new List<List<string>>();

            _loggingService.LogInfo($"구글시트 URL 접근: {sheetCsvUrl}");

            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30); // 30초 타임아웃
                
                var response = await client.GetAsync(sheetCsvUrl);
                response.EnsureSuccessStatusCode();
                
                var csvData = await response.Content.ReadAsStringAsync();
                _loggingService.LogInfo($"CSV 데이터 크기: {csvData.Length} 문자");
                
                if (string.IsNullOrWhiteSpace(csvData))
                {
                    _loggingService.LogError("CSV 데이터가 비어있습니다.");
                    return result;
                }

                var rows = csvData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                _loggingService.LogInfo($"총 {rows.Length}개 행 발견");

                bool first = true;
                bool second = true;
                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row)) continue;
                    if (true == first)
                    {
                        first = false;
                        continue;
                    }
                    if (true == second)
                    {
                        second = false;
                        continue;
                    }
                    // 간단한 CSV 파싱 (따옴표 처리 포함)
                    var cols = ParseCsvRow(row);
                    result.Add(cols);
                }

                _loggingService.LogInfo($"파싱 완료: {result.Count}개 행");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _loggingService.LogError($"HTTP 요청 실패: {ex.Message}");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _loggingService.LogError($"요청 타임아웃: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"구글시트 데이터 로딩 중 오류: {ex.Message}");
                throw;
            }
        }

        private List<string> ParseCsvRow(string csvRow)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;
            
            for (int i = 0; i < csvRow.Length; i++)
            {
                char c = csvRow[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            
            result.Add(current.ToString().Trim());
            return result;
        }

        private static UserTier ParseTier(string tierString)
        {
            if (Enum.TryParse<UserTier>(tierString, true, out var tier))
                return tier;
            return UserTier.Invalid;
        }

        private static MainLine ParseMainLine(string lineString)
        {
            if (Enum.TryParse<MainLine>(lineString, true, out var line))
                return line;
            return MainLine.All;
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