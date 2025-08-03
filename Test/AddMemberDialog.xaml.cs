using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Test.Log;
using Window = System.Windows.Window;

namespace Test
{
    /// <summary>
    /// AddMemberDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddMemberDialog : Window
    {
        public User? originUser { get; set; }
        public AddMemberDialog(User? originUser)
        {
            InitializeComponent();

            cbUserMainLine.ItemsSource = Enum.GetValues(typeof(MainLine));
            cbUserTierType.ItemsSource = Enum.GetValues(typeof(UserTier));

            this.originUser = originUser;
            if (originUser != null)
            {
                tbAddBoxBamName.Text = originUser.Name;
                tbAddBoxLolName.Text = originUser.NickName;
                tbNameTag.Text = originUser.Tag;
                cbUserTierType.Text = originUser.Tier.ToString();
                cbUserMainLine.Text = originUser.MainLine.ToString();
            }

        }

        private void btnAddMemberCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddMemberClick(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(tbAddBoxBamName.Text) ||
               string.IsNullOrEmpty(tbAddBoxLolName.Text) ||
               string.IsNullOrEmpty(cbUserTierType.Text) ||
               string.IsNullOrEmpty(cbUserMainLine.Text))
            {
                HandyControl.Controls.MessageBox.Show("입력안된게 있어요", "체크체크");
                return;
            }


            bool addNewUser = originUser == null;
            string oldName = string.Empty;
            string oldNick = string.Empty;
            User? newUser = null;
            if (true == addNewUser)
            {
                newUser = new User();
            }
            else
            {
                oldName = originUser.Name;
                oldNick = originUser.NickName;
                newUser = originUser;
            }

            newUser.Name = tbAddBoxBamName.Text;
            newUser.NickName = tbAddBoxLolName.Text;
            newUser.Tier = (UserTier)Enum.Parse(typeof(UserTier), cbUserTierType.Text);
            newUser.MainLine = (MainLine)Enum.Parse(typeof(MainLine), cbUserMainLine.Text);
            newUser.Tag = tbNameTag.Text;

            if (addNewUser)
            {
                MatchingManager.Instance.InsertUser(newUser);
                Stash.LogInfo($"{newUser.Name} 추가!\n노션에 업데이트 해주세요");
            }
            else
            {
                MatchingManager.Instance.Save();
                Stash.LogInfo($"닉네임수정\n{oldName}->{newUser.Name}\n{oldNick}->{newUser.NickName}\n노션에 업데이트 해주세요");
            }

            StringBuilder sb = new();
            sb.AppendLine($"Name\tNickName\tTier\tTag\tMainLine");
            sb.AppendLine($"{newUser.Name}\t{newUser.NickName}\t{(int)newUser.Tier}\t{newUser.Tag}\t{newUser.MainLine}");

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string directoryPath = System.IO.Path.Combine(userProfile, "Document", "GameMatchingBom");
            string fileName = System.IO.Path.Combine(directoryPath, "Export_UserInfo.txt");

            if (false == Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            File.WriteAllText(fileName, sb.ToString());

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{fileName}\"", // 파일 경로 인자로 전달
                UseShellExecute = false
            };
            Process.Start(psi);

            this.Close();
        }
    }
}
