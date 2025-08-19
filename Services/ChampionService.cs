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
            const string version = "14.3.1";
            const string defaultUrl = "https://ddragon.leagueoflegends.com/cdn";
            const string localeTag = "ko_KR";
            const string championTag = "champion.json";
            const string imgTag = "img";
            const string dataTag = "data";

            string url = $"{defaultUrl}/{version}/{dataTag}/{localeTag}/{championTag}";

            try
            {
                _loggingService.LogInfo("롤 챔피언 데이터 로딩 시작...");

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10); // 10초 타임아웃 설정
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
                    var semaphore = new SemaphoreSlim(10, 10); // 최대 10개 동시 처리
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
                            _loggingService.LogError($"챔피언 로딩 오류: {ex.Message}");
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
                    _loggingService.LogInfo($"롤 챔피언 로드 완료! (총 {AllChampions.Count}개)");
                }
                else
                {
                    _loggingService.LogError("챔피언 API 요청 실패");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"챔피언 초기화 실패: {ex.Message}");
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
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5); // 5초 타임아웃 설정
                byte[] imageData = await client.GetByteArrayAsync(imageUrl);
                
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