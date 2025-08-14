using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test
{
    public partial class UpdateTodayRecordView : UserControl
    {
        public MainWindow mainWindow;

        public UpdateTodayRecordView(MainWindow parent)
        {
            mainWindow = parent;
            InitializeComponent();
            RefreshView();
        }


        public void RefreshView()
        {
            cbGameTypeChoice.ItemsSource = new List<string>
            {
                "칼바람",
                "자랭",
                "내전",
                "선택하세요",
            };
            cbGameTypeChoice.Text = "선택하세요";
            TodaysMemberManager.Instance.SaveMembersRecord();
            dgSelectedMember.ItemsSource = MatchingManager.Instance.CurrentUsers;
            
            dgTodaysMember.ItemsSource = TodaysMemberManager.Instance.GetSortedRecordData(MatchingManager.Instance.MemberLoader.MemberList);
        }

        private void btnUpdateQueryCreate(object sender, RoutedEventArgs e)
        {
            var gameType = cbGameTypeChoice.Text;
            if (string.IsNullOrEmpty(gameType) || gameType == "선택하세요")
            {
                HandyControl.Controls.MessageBox.Show("게임 타입이 선택되지 않음.", "오류", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var updateUserList = MatchingManager.Instance.CurrentUsers;
            StringBuilder sb = new();
            DateTime now = DateTime.Now;
            string nowString = now.ToString("yyyy.MM.dd");
            foreach (var user in updateUserList)
            {
                sb.AppendLine($"{user.Name}\t{gameType}\t{nowString}");
            }

            ExportToCsvFile(sb.ToString());
        }

        private void btnReloadData(object sender, RoutedEventArgs e)
        {
            TodaysMemberManager.Instance.Init();
            RefreshView();
        }

        public async void ExportToExcelCloud(string content)
        {
            await Task.Run(() => { });
            //try
            //{
            //    // 사용자 인증 (OAuth2 팝업 자동 표시)
            //    UserCredential credential;
            //    using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            //    {
            //        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            //            GoogleClientSecrets.Load(stream).Secrets,
            //            new[] { SheetsService.Scope.Spreadsheets },
            //            "user",
            //            CancellationToken.None,
            //            new FileDataStore("token.json", true)
            //        );
            //    }

            //    // SheetsService 생성
            //    var service = new SheetsService(new BaseClientService.Initializer()
            //    {
            //        HttpClientInitializer = credential,
            //        ApplicationName = "WPF Google Sheets Demo",
            //    });

            //    // 작성할 데이터
            //    var values = new List<IList<object>> { new List<object> { "Hello", "from", "WPF" } };
            //    var valueRange = new ValueRange { Values = values };

            //    // 요청 생성
            //    var updateRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, SheetRange);
            //    updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            //    // 실행
            //    var result = await updateRequest.ExecuteAsync();
            //    MessageBox.Show("✅ 스프레드시트에 작성 성공!");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"❌ 오류 발생: {ex.Message}");
            //}
        }

        private void ExportToCsvFile(string content)
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string directoryPath = System.IO.Path.Combine(userProfile, "Document", "GameMatchingBom");
            string fileName = System.IO.Path.Combine(directoryPath, "Export_PlayRecord.txt");

            if (false == Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            content = $"export datetime {DateTime.Now}\nPlayerName\tGameType\tPlayedDate\n{content}";

            File.WriteAllText(fileName, content);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{fileName}\"", // 파일 경로 인자로 전달
                UseShellExecute = false
            };
            Process.Start(psi);
        }

        private void dgRankInfoViewDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var records = TodaysMemberManager.Instance.RecordDict;

            if (dgTodaysMember.SelectedItem == null)
            {
                HandyControl.Controls.MessageBox.Show("선택된 항목이 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var selectedRecord = dgTodaysMember.SelectedItem as RecordViewData;
            if (selectedRecord == null)
            {
                HandyControl.Controls.MessageBox.Show("선택된 항목이 올바르지 않습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (false == records.TryGetValue(selectedRecord.PlayerName, out var recordList))
            {
                HandyControl.Controls.MessageBox.Show($"선택된 플레이어의 기록이 없습니다: {selectedRecord.PlayerName}", "게임 안하는사람", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"플레이어: {selectedRecord.PlayerName}");
            if (recordList.Count == 0)
            {
                sb.AppendLine("기록이 없습니다...");
            }
            else
            {
                Dictionary<string, List<string>> datePlayRecord = new();
                foreach (var item in recordList)
                {
                    if (false == datePlayRecord.ContainsKey(item.PlayedHistory))
                    {
                        datePlayRecord[item.PlayedHistory] = new List<string>();
                    }
                    datePlayRecord[item.PlayedHistory].Add(item.GameType);
                }

                foreach(var dateRecord in datePlayRecord)
                {
                    StringBuilder temp = new();
                    temp.Append($"{dateRecord.Key} {string.Join(",", dateRecord.Value)}");
                    uint point = 0;
                    foreach (var gameType in dateRecord.Value)
                        point = Math.Max(point, TodaysMemberManager.GetPointByGameType(gameType));
                    temp.Append($" +{point}");

                    sb.AppendLine(temp.ToString());
                }
            }
            HandyControl.Controls.MessageBox.Show(sb.ToString(), "플레이 기록", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
