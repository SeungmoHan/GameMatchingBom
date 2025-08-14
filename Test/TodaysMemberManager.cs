using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using Test.SpreadSheet;

namespace Test
{
    public class PlayRecord
    {
        public string PlayedHistory { get; set; } = string.Empty;
        public string GameType { get; set; } = string.Empty;
    }

    public class RecordViewData
    {
        public uint Ranking { get; set; } = 0;
        public string PlayerName { get; set; } = string.Empty;
        public uint CurrentPoint { get; set; } = 0;
    }

    public class TodaysMemberManager
    {
        public static TodaysMemberManager Instance { get; set; } = new();

        public Dictionary<string, List<PlayRecord>> RecordDict { get; set; } = new Dictionary<string, List<PlayRecord>>();
        public void Init()
        {
            LoadMembersRecord();
        }

        public List<RecordViewData> GetSortedRecordData(List<User> members)
        {
            List<RecordViewData> ret = new List<RecordViewData>();
            foreach (var member in members)
            {
                bool valid = true;
                foreach (var already in ret)
                {
                    if (already.PlayerName == member.Name)
                    {
                        HandyControl.Controls.MessageBox.Show($"중복된 이름이 있습니다: {member.Name}", "오류");
                        valid = false;
                        break;
                    }
                }

                if(valid == false)
                    continue;

                RecordViewData view = new RecordViewData();
                view.PlayerName = member.Name;
                view.CurrentPoint = 0;
                ret.Add(view);
            }


            foreach (var record in RecordDict)
            {
                string playerName = record.Key;
                var history = record.Value;

                foreach(var item in ret)
                {
                    if (item.PlayerName == playerName)
                    {
                        item.CurrentPoint = GetRecordPoint(history);
                        break;
                    }
                }
            }

            ret = ret.OrderByDescending(item => item.CurrentPoint).ToList();
            uint index = 0;
            uint rank = 0;
            uint lastPoint = 978671234; // invalid한 임의의 값
            ret.ForEach((item) =>
            {
                if (item.CurrentPoint == lastPoint)
                {
                    ++index;
                }
                else
                {
                    rank = ++index;
                }
                lastPoint = item.CurrentPoint;
                item.Ranking = rank;
            });

            return ret;
        }

        private uint GetRecordPoint(List<PlayRecord> matchRecord)
        {
            Dictionary<string,uint> playedDateSet = new();

            foreach (var record in matchRecord)
            {
                if (false == playedDateSet.ContainsKey(record.PlayedHistory))
                    playedDateSet.Add(record.PlayedHistory, 0);
                playedDateSet[record.PlayedHistory]= Math.Max(GetPointByGameType(record.GameType), playedDateSet[record.PlayedHistory]);
            }

            uint ret = 0;
            foreach(var datePoint in playedDateSet)
            {
                ret += datePoint.Value;
            }
            return ret;
        }

        private uint GetPointByGameType(string gameType)
        {
            switch (gameType)
            {
                case "자랭":
                    return 1;
                case "칼바람":
                    return 1;
                case "내전":
                    return 3;
            }
            return 0;
        }

        public bool LoadMembersRecord()
        {
            string SheetId = "1twtASQQqxml0G8jZ20fKQaaJ89Ih0IFFuuhFJBzrg1w";
            string SheetGid = "0";
            List<List<string>> rows = SpreadSheetLoader.LoadSheetData(SheetId, SheetGid);
            bool readComment = false;
            bool readColumnName = false;
            foreach (var item in rows)
            {
                if (readComment == false)
                {
                    readComment = true;
                    continue;
                }

                if (readColumnName == false)
                {
                    readColumnName = true;
                    continue;
                }

                string playerName = item[0];
                string gameType = item[1];
                string playedDate = item[2];
                if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(gameType) ||
                    string.IsNullOrEmpty(playedDate))
                    continue;
                if (false == RecordDict.ContainsKey(playerName))
                {
                    RecordDict[playerName] = new List<PlayRecord>();
                }
                RecordDict[playerName].Add(new PlayRecord()
                {
                    PlayedHistory = playedDate,
                    GameType = gameType
                });
            }
            return true;
        }

        public bool SaveMembersRecord()
        {

            return true;
        }

    }
}
