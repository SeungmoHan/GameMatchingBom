using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Test
{
    public class ChampionInfo
    {
        public ChampionInfo(string championNameKor, string championNameEng, string championImageUrl, BitmapImage championImage)
        {
            ChampionNameKor = championNameKor;
            ChampionNameEng = championNameEng;
            ChampionImageUrl = championImageUrl;
            ChampionImage = championImage;
        }

        public string ChampionNameKor { get; set; } = string.Empty;
        public string ChampionNameEng { get; set; } = string.Empty;
        public string ChampionImageUrl { get; set; } = string.Empty;
        public BitmapImage ChampionImage { get; set; }
    }

    public class PeerlessManager
    {
        public static PeerlessManager Instance { get; private set; } = new PeerlessManager();

        private PeerlessManager() { }

        public List<ChampionInfo> AllChampion { get; set; } = new();
        public List<ChampionInfo> RemainedChampions { get; set; } = new();
        public List<ChampionInfo> UsedChampions { get; set; } = new();
        public List<ChampionInfo> SelectedChampionInThisGame { get; set; } = new();
        public bool IsLoaded { get; set; } = false;
        public bool CanPick { get; set; } = false;
        public int CurrentChampionCount
        {
            get
            {
                lock (lockObj)
                {
                    return AllChampion.Count;
                }
            }
        }

        public int TotalChampionCount { get; set; } = 0;
        public int SelectedCount { get; set; } = 10;

        private object lockObj = new();
        
        public bool IsEnalbeSelect()
        {
            if (SelectedChampionInThisGame.Count >= SelectedCount)
                return false;
            return true;
        }

        public void AddSelectedChampion(ChampionInfo info, bool multiple = false)
        {
            int idx = RemainedChampions.FindIndex(o => o.ChampionNameKor == info.ChampionNameKor);
            if (idx == -1)
                return;
            //if (false == multiple)
            //    Log.Stash.LogInfo($"{RemainedChampions[idx].ChampionNameKor}선택");
            SelectedChampionInThisGame.Add(RemainedChampions[idx]);
            RemainedChampions.RemoveAt(idx);
            return;
        }

        public void RevertSelectedChampion()
        {
            StringBuilder sb = new();
            foreach(var champ in SelectedChampionInThisGame)
            {
                RemainedChampions.Add(champ);
                sb.AppendLine($"[{champ.ChampionNameKor}]");
            }
            sb.Append("제거");
            //Log.Stash.LogInfo(sb.ToString());
            SelectedChampionInThisGame.Clear();
        }

        public void RevertUsedItems()
        {
            foreach (var item in UsedChampions)
            {
                RemainedChampions.Add(item);
            }
            UsedChampions.Clear();
        }

        public void RemoveUsedItem(List<ChampionInfo> items)
        {
            foreach(var item in items)
            {
                int idx = UsedChampions.FindIndex(o => o.ChampionNameKor == item.ChampionNameKor);
                if (idx == -1)
                    continue;

                RemainedChampions.Add(UsedChampions[idx]);
                UsedChampions.RemoveAt(idx);
            }
        }

        public void CommitSelectedChampion()
        {
            StringBuilder sb = new();
            foreach(var champ in SelectedChampionInThisGame)
            {
                UsedChampions.Add(champ);
                sb.AppendLine($"[{champ.ChampionNameKor}]");
            }
            sb.Append("사용 확정!");
            Log.Stash.LogInfo(sb.ToString());
            SelectedChampionInThisGame.Clear();
        }

        public void RemoveSelectedChampion(ChampionInfo info)
        {
            int idx = SelectedChampionInThisGame.FindIndex(o => o.ChampionNameKor == info.ChampionNameKor);
            if(idx == -1) 
                return;
            Log.Stash.LogInfo($"{SelectedChampionInThisGame[idx].ChampionNameKor} 제거!");
            RemainedChampions.Add(SelectedChampionInThisGame[idx]);
            SelectedChampionInThisGame.RemoveAt(idx);
        }

        public void PickRandomChampion(int pickCount)
        {
            StringBuilder sb = new();
            for (int i = 0; i < pickCount; i++)
            {
                int randIdx = Random.Shared.Next(0, RemainedChampions.Count);
                sb.AppendLine($"[{RemainedChampions[randIdx].ChampionNameKor}]");
                AddSelectedChampion(RemainedChampions[randIdx], true);
            }
            sb.Append("추가!");
            Log.Stash.LogInfo(sb.ToString());
        }



        public void InitChampions()
        {
            string version = "14.3.1"; // 최신 버전 확인 필요
            string defaultUrl = "https://ddragon.leagueoflegends.com/cdn";
            string localeTag = "ko_KR";
            string championTag = "champion.json";
            string imgTag = "img";
            string dataTag = "data";

            string url = $"{defaultUrl}/{version}/{dataTag}/{localeTag}/{championTag}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = response.Content.ReadAsStringAsync().Result;
                    JObject data = JObject.Parse(jsonString);
                    if (data == null)
                        return;
                    TotalChampionCount = data["data"].Values().Count();
                    foreach (var champion in data["data"])
                    {
                        Task.Run(() =>
                        {
                            string championNameEng = champion.First["id"].ToString();
                            string championNameKor = champion.First["name"].ToString();
                            string championImgUrl = $"{defaultUrl}/{version}/{imgTag}/champion/{championNameEng}.png";

                            BitmapImage? image = LoadImageAsync(championImgUrl);
                            if (null == image)
                                return;

                            ChampionInfo info = new(championNameKor, championNameEng, championImgUrl, image);
                            if (null == info)
                                return;

                            lock(lockObj)
                            {
                                AllChampion.Add(info);
                                if(AllChampion.Count== TotalChampionCount)
                                {
                                    RemainedChampions = new List<ChampionInfo>(AllChampion);
                                    Log.Stash.LogInfo("롤 챔피언 로드 완료!");
                                }
                            }
                        });
                    }
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("API 요청 실패");
                }
            }
        }
        private BitmapImage? LoadImageAsync(string imageUrl)
        {
            BitmapImage? bitmap = DownloadImageAsync(imageUrl);
            if (bitmap == null)
            {
                return null;
            }
            return bitmap;
        }

        private BitmapImage? DownloadImageAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    byte[] imageData = client.GetByteArrayAsync(url).Result;
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();  // UI 스레드에서 사용 가능하도록 Freeze 적용
                        return bitmap;
                    }
                }
                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Show($"이미지 로딩 실패: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
