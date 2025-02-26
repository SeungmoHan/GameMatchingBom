using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using HandyControl.Tools.Extension;
using Test.Log;

namespace Test
{
    /// <summary>
    /// HomeView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomeView : UserControl
    {
        private MainWindow parent;
        private ObservableCollection<string> memberNickNameStrings;
        private ObservableCollection<string> nameFilter = new();
        public HomeView(MainWindow parent)
        {
            InitializeComponent();

            memberNickNameStrings = new(MatchingManager.Instance.MemberLoader.MemberList.Select(u => $"{u.Name}-{u.NickName}"));
            lstSuggestions.ItemsSource = nameFilter;
            cbMemberCountType.ItemsSource = Enum.GetValues(typeof(MemberCount));
            this.parent = parent;
            DataContext = this;
            cbMemberCountType.Text = "_5명";
            RefreshView();
        }


        private void btnCreateNewUser(object sender, RoutedEventArgs e)
        {
            AddMemberDialog addDialog = new(null);
            addDialog.originUser = null;
            addDialog.ShowDialog();
        }



        private void RefreshView()
        {
            tbCurrentUserCount.Text = $"매칭 대기 인원 : {MatchingManager.Instance.CurrentUsers.Count}명";
            dgPeople.ItemsSource = MatchingManager.Instance.CurrentUsers;
            dgPeople.Items.Refresh();
            memberNickNameStrings = new(MatchingManager.Instance.MemberLoader.MemberList.Select(u => $"{u.Name}-{u.NickName}"));
            txtSearchKeyUp(null, null);
        }

        private void btnReloadMember(object sender, RoutedEventArgs e)
        {
            MatchingManager.Instance.MemberLoader.Reload();
            Stash.LogInfo("맴버 리로드 성공!");
            RefreshView();
        }

        private void btnResetCurrentMember(object sender, RoutedEventArgs e)
        {
            MatchingManager.Instance.CurrentUsers.Clear();
            tbNameAdd.Text = "이름 추가";
            Stash.LogInfo("대기열 맴버 리셋");
            RefreshView();
        }

        private void tbNameGotFocus(object sender, RoutedEventArgs e)
        {
            tbNameAdd.Text = "";
        }

        private void dgPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var userInfo = dgPeople.SelectedItem as User;
            if (userInfo == null)
                return;
            tbNameAdd.Text = userInfo.Name;
            RemoveCurrentUser();
            RefreshView();
        }

        public void RemoveCurrentUser()
        {
            if (false == Util.HasName(tbNameAdd.Text, MatchingManager.Instance.CurrentUsers))
                return;
            Util.RemoveName(tbNameAdd.Text, ref MatchingManager.Instance.CurrentUsers);
            Stash.LogInfo($"대기중인 참가자 {tbNameAdd.Text} 제거");
            tbNameAdd.Text = "이름 추가";
            txtSearch.Text = "";
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
            }
        }

        private void btnShuffleClickWithoutLine(object sender, RoutedEventArgs e)
        {
            MatchingManager.Instance.UseLineInfo = false;
            var memberCount = (MemberCount)Enum.Parse(typeof(MemberCount), cbMemberCountType.Text);
            MatchingManager.Instance.MatchingMemberCount = (int)memberCount;
            var matchResultView = new MatchingResultView(parent);
            matchResultView.ShowMatchInfo();
            parent.MainContent.Content = matchResultView;
            Stash.LogInfo($"{memberCount} vs {memberCount} 매칭 (라인X)");
        }

        private void btnShuffleClickWithLine(object sender, RoutedEventArgs e)
        {
            MatchingManager.Instance.UseLineInfo = true;
            var memberCount = (MemberCount)Enum.Parse(typeof(MemberCount), cbMemberCountType.Text);
            MatchingManager.Instance.MatchingMemberCount = (int)memberCount;
            var matchResultView = new MatchingResultView(parent);
            matchResultView.ShowMatchInfo();
            parent.MainContent.Content = matchResultView;
            Stash.LogInfo($"{memberCount} vs {memberCount} 매칭 (라인O)");
        }

        private void txtSearchKeyUp(object sender, KeyEventArgs e)
        {
            string query = txtSearch.Text.ToLower();

            // 검색어에 맞는 항목 필터링
            nameFilter.Clear();
            var curUser = MatchingManager.Instance.CurrentUsers;
            var results = memberNickNameStrings.Where(item => item.ToLower().Contains(query)).ToList();

            List<string> removeItem = new();
            foreach (var result in results)
            {
                foreach (var user in curUser)
                {
                    if (result == $"{user.Name}-{user.NickName}")
                        removeItem.Add(result);
                }
            }
            foreach(var item in removeItem)
                results.Remove(item);


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

                var findUser = Util.GetUser(tbNameAdd.Text, MatchingManager.Instance.CurrentUsers);
                if (findUser != null)
                {
                    HandyControl.Controls.MessageBox.Show($"{tbNameAdd.Text}는 이미 매칭 맴버에 포함되어있습니다");
                }
                else
                {
                    var addUser = Util.GetUser(tbNameAdd.Text, MatchingManager.Instance.MemberLoader.MemberList);
                    if (addUser != null)
                    {
                        Util.AddNew(addUser, ref MatchingManager.Instance.CurrentUsers);
                        Stash.LogInfo($"대기열에 {addUser.Name} 추가!");
                    }
                }
 
            }

            tbNameAdd.Text = string.Empty;
            txtSearch.Text = string.Empty;
            RefreshView();
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var itemString = lstSuggestions.SelectedItem as string;
            if (itemString == null)
            {
                return;
            }
            var str = itemString.Split('-').First();
            var user = Util.GetUser(str, MatchingManager.Instance.MemberLoader.MemberList);
            if (null == user)
            {
                return;
            }
            MatchingManager.Instance.MemberLoader.RemoveMemberList(user);
            Util.RemoveName(user.Name, ref MatchingManager.Instance.CurrentUsers); 
            Stash.LogInfo($"맴버 리스트에서 {user.Name} 제거!\n노션에 업데이트 해주세요");
            RefreshView();
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var itemString = lstSuggestions.SelectedItem as string;
            if (itemString == null)
            {
                return;
            }
            var str = itemString.Split('-').First();
            var user = Util.GetUser(str, MatchingManager.Instance.MemberLoader.MemberList);
            if (null == user)
            {
                return;
            }

            AddMemberDialog dialog = new(user);
            dialog.ShowDialog();
            RefreshView();
        }
    }
}
