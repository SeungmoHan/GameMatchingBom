using NewMatchingBom.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NewMatchingBom.Services
{
    public class ChampionService : IChampionService
    {
        private readonly ILoggingService _loggingService;
        private readonly object _lockObj = new();

        public ObservableCollection<ChampionInfo> AllChampions { get; } = new();
        public ObservableCollection<ChampionInfo> RemainedChampions { get; } = new();
        public ObservableCollection<ChampionInfo> UsedChampions { get; } = new();
        public ObservableCollection<ChampionInfo> SelectedChampions { get; } = new();

        public bool IsLoaded { get; private set; } = false;
        public int TotalChampionCount { get; private set; } = 0;
        public int SelectedCount { get; set; } = 10;

        public ChampionService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task InitializeChampionsAsync()
        {
            // 최신 버전 자동 확인
            string version = await GetLatestVersionAsync() ?? "14.24.1"; // 기본값으로 최신 버전 사용
            const string defaultUrl = "https://ddragon.leagueoflegends.com/cdn";
            const string localeTag = "ko_KR";
            const string championTag = "champion.json";
            const string imgTag = "img";
            const string dataTag = "data";

            string url = $"{defaultUrl}/{version}/{dataTag}/{localeTag}/{championTag}";

            try
            {
                _loggingService.LogInfo($"롤 챔피언 데이터 로딩 시작... (버전: {version})");

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30); // 타임아웃 30초로 증가
                
                // 재시도 로직 추가
                int maxRetries = 3;
                for (int retry = 0; retry < maxRetries; retry++)
                {
                    try
                    {
                        var response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonString = await response.Content.ReadAsStringAsync();
                            JObject data = JObject.Parse(jsonString);
                            TotalChampionCount = data["data"]?.Values().Count() ?? 0;

                            // 동시 처리를 위한 컬렉션 준비
                            var championInfos = new List<ChampionInfo>();
                            var championsData = data["data"]?.ToObject<Dictionary<string, JToken>>() ?? new Dictionary<string, JToken>();
                            
                            // 병렬 처리로 챔피언 정보 로드 (최대 동시성 제한)
                            var semaphore = new SemaphoreSlim(5, 5); // 동시 처리를 5개로 줄여 안정성 향상
                            var tasks = championsData.Select(async champion =>
                            {
                                await semaphore.WaitAsync();
                                try
                                {
                                    string? championNameEng = champion.Value["id"]?.ToString();
                                    string? championNameKor = champion.Value["name"]?.ToString();
                                    
                                    if (string.IsNullOrEmpty(championNameEng) || string.IsNullOrEmpty(championNameKor))
                                        return null;

                                    string championImgUrl = $"{defaultUrl}/{version}/{imgTag}/champion/{championNameEng}.png";
                                    BitmapImage? image = await LoadImageAsync(championImgUrl);

                                    return new ChampionInfo(championNameKor, championNameEng, championImgUrl, image);
                                }
                                catch (Exception ex)
                                {
                                    _loggingService.LogError($"챔피언 로딩 오류 ({champion.Key}): {ex.Message}");
                                    return null;
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }).ToArray();

                            var results = await Task.WhenAll(tasks);
                            
                            // UI 스레드에서 한번에 추가 (성능 향상)
                            foreach (var championInfo in results.Where(c => c != null))
                            {
                                AllChampions.Add(championInfo!);
                                RemainedChampions.Add(championInfo!);
                            }
                            
                            IsLoaded = true;
                            _loggingService.LogInfo($"롤 챔피언 로드 완료! (총 {AllChampions.Count}개, 버전: {version})");
                            
                            // 성공 시 캐시에 저장
                            await SaveToCacheAsync(version);
                            return; // 성공 시 메서드 종료
                        }
                        else
                        {
                            _loggingService.LogError($"챔피언 API 요청 실패 (시도 {retry + 1}/{maxRetries}): {response.StatusCode}");
                            if (retry < maxRetries - 1)
                            {
                                await Task.Delay(2000 * (retry + 1)); // 재시도 전 대기 (2초, 4초, 6초)
                            }
                        }
                    }
                    catch (HttpRequestException httpEx)
                    {
                        _loggingService.LogError($"네트워크 오류 (시도 {retry + 1}/{maxRetries}): {httpEx.Message}");
                        if (retry < maxRetries - 1)
                        {
                            await Task.Delay(2000 * (retry + 1));
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _loggingService.LogError($"요청 타임아웃 (시도 {retry + 1}/{maxRetries})");
                        if (retry < maxRetries - 1)
                        {
                            await Task.Delay(2000 * (retry + 1));
                        }
                    }
                }
                
                // 모든 재시도 실패 시 로컬 캐시 사용 시도
                await LoadFromLocalCacheAsync();
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"챔피언 초기화 실패: {ex.Message}");
                // 실패 시 로컬 캐시 시도
                await LoadFromLocalCacheAsync();
            }
        }

        private async Task<string?> GetLatestVersionAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync("https://ddragon.leagueoflegends.com/api/versions.json");
                
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var versions = JArray.Parse(jsonString);
                    return versions.FirstOrDefault()?.ToString();
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"최신 버전 확인 실패: {ex.Message}");
            }
            return null;
        }

        private async Task LoadFromLocalCacheAsync()
        {
            try
            {
                _loggingService.LogInfo("로컬 캐시에서 챔피언 데이터 로드 시도...");
                
                string cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NewMatchingBom", "cache");
                string cacheFile = Path.Combine(cacheDir, "champions.json");
                
                if (File.Exists(cacheFile))
                {
                    string jsonString = await File.ReadAllTextAsync(cacheFile);
                    JObject data = JObject.Parse(jsonString);
                    
                    var championsData = data["champions"]?.ToObject<List<JToken>>() ?? new List<JToken>();
                    
                    foreach (var champion in championsData)
                    {
                        string? nameKor = champion["nameKor"]?.ToString();
                        string? nameEng = champion["nameEng"]?.ToString();
                        string? imgUrl = champion["imgUrl"]?.ToString();
                        
                        if (!string.IsNullOrEmpty(nameKor) && !string.IsNullOrEmpty(nameEng))
                        {
                            // 캐시된 이미지 로드 시도
                            BitmapImage? image = null;
                            if (!string.IsNullOrEmpty(imgUrl))
                            {
                                image = await LoadImageAsync(imgUrl);
                            }
                            
                            var championInfo = new ChampionInfo(nameKor, nameEng, imgUrl ?? "", image);
                            AllChampions.Add(championInfo);
                            RemainedChampions.Add(championInfo);
                        }
                    }
                    
                    TotalChampionCount = AllChampions.Count;
                    IsLoaded = true;
                    _loggingService.LogInfo($"로컬 캐시에서 {AllChampions.Count}개 챔피언 로드 완료");
                }
                else
                {
                    _loggingService.LogError("로컬 캐시 파일이 존재하지 않습니다.");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"로컬 캐시 로드 실패: {ex.Message}");
            }
        }

        private async Task SaveToCacheAsync(string version)
        {
            try
            {
                string cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NewMatchingBom", "cache");
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }
                
                string cacheFile = Path.Combine(cacheDir, "champions.json");
                
                var cacheData = new JObject
                {
                    ["version"] = version,
                    ["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["champions"] = new JArray(
                        AllChampions.Select(c => new JObject
                        {
                            ["nameKor"] = c.ChampionNameKor,
                            ["nameEng"] = c.ChampionNameEng,
                            ["imgUrl"] = c.ChampionImageUrl
                        })
                    )
                };
                
                await File.WriteAllTextAsync(cacheFile, cacheData.ToString());
                _loggingService.LogInfo($"챔피언 데이터를 캐시에 저장했습니다 (버전: {version})");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"캐시 저장 실패: {ex.Message}");
            }
        }

        public bool CanSelect()
        {
            return SelectedChampions.Count < SelectedCount;
        }

        public void AddSelectedChampion(ChampionInfo champion)
        {
            if (!CanSelect())
            {
                _loggingService.LogError($"선택 가능한 챔피언 수를 초과했습니다 (최대 {SelectedCount}개)");
                return;
            }

            var championToMove = RemainedChampions.FirstOrDefault(c => c.ChampionNameKor == champion.ChampionNameKor);
            if (championToMove != null)
            {
                RemainedChampions.Remove(championToMove);
                SelectedChampions.Add(championToMove);
                _loggingService.LogInfo($"{championToMove.ChampionNameKor} 선택");
            }
        }

        public void RemoveSelectedChampion(ChampionInfo champion)
        {
            var championToMove = SelectedChampions.FirstOrDefault(c => c.ChampionNameKor == champion.ChampionNameKor);
            if (championToMove != null)
            {
                SelectedChampions.Remove(championToMove);
                RemainedChampions.Add(championToMove);
                _loggingService.LogInfo($"{championToMove.ChampionNameKor} 제거");
            }
        }

        public void RevertSelectedChampions()
        {
            StringBuilder sb = new();
            foreach (var champ in SelectedChampions)
            {
                RemainedChampions.Add(champ);
                sb.AppendLine($"[{champ.ChampionNameKor}]");
            }
            sb.Append("제거");
            _loggingService.LogInfo(sb.ToString());
            SelectedChampions.Clear();
        }

        public void CommitSelectedChampions()
        {
            StringBuilder sb = new();
            foreach (var champ in SelectedChampions)
            {
                UsedChampions.Add(champ);
                sb.AppendLine($"[{champ.ChampionNameKor}]");
            }
            sb.Append("사용 확정!");
            _loggingService.LogInfo(sb.ToString());
            SelectedChampions.Clear();
        }

        public void PickRandomChampions(int count)
        {
            if (RemainedChampions.Count < count)
            {
                _loggingService.LogError($"남은 챔피언이 부족합니다 (요청: {count}, 남은 수: {RemainedChampions.Count})");
                return;
            }

            StringBuilder sb = new();
            var random = new Random();
            
            for (int i = 0; i < count; i++)
            {
                if (!CanSelect() || RemainedChampions.Count == 0)
                    break;

                int randIdx = random.Next(0, RemainedChampions.Count);
                var champion = RemainedChampions[randIdx];
                
                RemainedChampions.RemoveAt(randIdx);
                SelectedChampions.Add(champion);
                
                sb.AppendLine($"[{champion.ChampionNameKor}]");
            }
            
            sb.Append("추가!");
            _loggingService.LogInfo(sb.ToString());
        }

        public void RevertUsedChampions()
        {
            foreach (var champion in UsedChampions)
            {
                RemainedChampions.Add(champion);
            }
            UsedChampions.Clear();
            _loggingService.LogInfo("사용된 챔피언 복구 완료");
        }

        public void RemoveUsedChampions(List<ChampionInfo> champions)
        {
            foreach (var champion in champions)
            {
                var usedChampion = UsedChampions.FirstOrDefault(c => c.ChampionNameKor == champion.ChampionNameKor);
                if (usedChampion != null)
                {
                    UsedChampions.Remove(usedChampion);
                    RemainedChampions.Add(usedChampion);
                }
            }
            _loggingService.LogInfo($"{champions.Count}개 챔피언 복구");
        }

        private async Task<BitmapImage?> LoadImageAsync(string imageUrl)
        {
            try
            {
                // 로컬 캐시 확인
                string cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NewMatchingBom", "cache", "images");
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }
                
                string fileName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
                string cachePath = Path.Combine(cacheDir, fileName);
                
                byte[] imageData;
                
                if (File.Exists(cachePath))
                {
                    // 캐시에서 로드
                    imageData = await File.ReadAllBytesAsync(cachePath);
                }
                else
                {
                    // 웹에서 다운로드
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(10); // 타임아웃 10초로 증가
                    imageData = await client.GetByteArrayAsync(imageUrl);
                    
                    // 캐시에 저장
                    await File.WriteAllBytesAsync(cachePath, imageData);
                }
                
                using var ms = new MemoryStream(imageData);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
                
                return bitmap;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"이미지 로딩 실패 ({imageUrl}): {ex.Message}");
                return null;
            }
        }
    }
}