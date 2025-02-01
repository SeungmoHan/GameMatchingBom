using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Microsoft.VisualBasic.CompilerServices;

namespace Test
{

    public partial class MainWindow
    {
        private List<User> currentUsers;
        private MemberLoader memberLoader = new();
        private ObservableCollection<string> memberNickNameStrings;
        private ObservableCollection<string> nameFilter = new();

        public MainWindow()
        {
            InitializeComponent();
            memberLoader.Init();
            currentUsers = new();
            DataContext = this;
            memberNickNameStrings = new(memberLoader.MemberList.Select(u => $"{u.Name}-{u.NickName}"));
            lstSuggestions.ItemsSource = nameFilter;
            RefreshView();
        }

        private void btnAddPeople(object sender, RoutedEventArgs e)
        {
            // 있는이름확인
            if (Util.HasName(tbNameAdd.Text, currentUsers))
            {
                HandyControl.Controls.MessageBox.Show($"{tbNameAdd.Text}는 이미 등록된 유저입니다.");
                return;
            }
            // Invalid 이름 확인
            if (true == Util.InvalidName(tbNameAdd.Text))
            {
                tbNameAdd.Text = "이름 추가";
                return;
            }
            
            // 유저 딕셔너리에 없으면...
            User? newUser = Util.FindUser(tbNameAdd.Text, memberLoader.MemberList);
            if (null == newUser)
            {
                // 새로 만들어서 기본값대입 이름만 변경
                newUser = new();
                newUser.Name = tbNameAdd.Text;
                newUser.NickName = "없음";
                newUser.Tier = UserTier.Invalid;
            }
            Util.AddNew(newUser, ref currentUsers);
            tbNameAdd.Text = "이름 추가";
            txtSearch.Text = "";
            RefreshView();
        }

        private void btnRemovePeople(object sender, RoutedEventArgs e)
        {
            if (false == Util.HasName(tbNameAdd.Text, currentUsers))
                return;
            Util.RemoveName(tbNameAdd.Text, ref currentUsers);
            tbNameAdd.Text = "이름 추가";
            txtSearch.Text = "";
            RefreshView();
        }

        private void RefreshView()
        {
            dgPeople.ItemsSource = currentUsers;
            dgPeople.Items.Refresh();
            txtSearchKeyUp(null, null);
        }

        private void btnResetPeople(object sender, RoutedEventArgs e)
        {
            currentUsers.Clear();
            tbNameAdd.Text = "이름 추가";
            RefreshView();
        }

        private void tbNameGotFocus(object sender, RoutedEventArgs e)
        {
            tbNameAdd.Text = "";
        }

        private void btnShuffleClick(object sender, RoutedEventArgs e)
        {
            MatchResult matchResult = MatchMaker.GetMatches(currentUsers);

            lvTeamResult.ItemsSource = matchResult.Matches;
            lvRemainPeople.ItemsSource = matchResult.RemainUsers.nonMatchedUser;
        }

        private void txtSearchKeyUp(object sender, KeyEventArgs e)
        {
            string query = txtSearch.Text.ToLower();

            // 검색어에 맞는 항목 필터링
            nameFilter.Clear();
            var results = memberNickNameStrings.Where(item => item.ToLower().Contains(query)).ToList();

            if (results.Any())
            {
                foreach (var item in results)
                    nameFilter.Add(item);

                lstSuggestions.Visibility = Visibility.Visible;
            }
            else
            {
                lstSuggestions.Visibility = Visibility.Collapsed;
            }
        }

        private void lstSuggestionsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstSuggestions.SelectedItem is string selectedItem)
            {
                txtSearch.Text = selectedItem;
                tbNameAdd.Text = selectedItem.Split('-').First();

                btnAddPeople(null, null);
            }
        }

        private void dgPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var userInfo = dgPeople.SelectedItem as User;
            if (userInfo == null)
                return;
            tbNameAdd.Text = userInfo.Name;
            btnRemovePeople(null,null);
            RefreshView();
        }


        private void dgPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var hit = dgPeople.InputHitTest(e.GetPosition(dgPeople)) as DependencyObject;

            while (hit != null && !(hit is DataGridRow))
            {
                hit = VisualTreeHelper.GetParent(hit);
            }

            if (hit is DataGridRow row)
            {
                var selectedUser = (User)row.Item;
                tbNameAdd.Text = selectedUser.Name;
                //MessageBox.Show($"클릭된 유저: {selectedUser.Name} ({selectedUser.NickName})");
            }
        }
    }
}