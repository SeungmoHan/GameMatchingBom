using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

            dgTodaysMember.ItemsSource = TodaysMemberManager.Instance.GetSortedRecordData();
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

        private void ExportToCsvFile(string content)
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string directoryPath = System.IO.Path.Combine(userProfile, "Document", "GameMatchingBom");
            string fileName = System.IO.Path.Combine(directoryPath, "Export_PlayRecord.txt");

            if (false == Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            content = $"PlayerName\tGameType\tPlayedDate\n{content}";

            File.WriteAllText(fileName, content);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{fileName}\"", // 파일 경로 인자로 전달
                UseShellExecute = false
            };
            Process.Start(psi);
        }
    }
}
