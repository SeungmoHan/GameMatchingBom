using System;
using System.Collections.Generic;
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
    /// <summary>
    /// MatchingResultView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MatchingResultView : UserControl
    {
        private MainWindow parent;
        public MatchingResultView(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;

            // 여기서는 view에 뿌려주기만 하면끝
            var matchResult = MatchingManager.Instance.GetLastMatchResult();
            if (matchResult != null && matchResult.Matches.Count > 0)
            {
                tbMatchingResultEmpty.Visibility = Visibility.Hidden;
                lvTeamResult.ItemsSource = matchResult.Matches;
                lvRemainPeople.ItemsSource = matchResult.RemainUsers.nonMatchedUser;
            }
            else
            {
                tbMatchingResultEmpty.Visibility = Visibility.Visible;
                lvRemainPeople.ItemsSource = new List<string>();
            }
        }

        public void ShowMatchInfo()
        {
            // 여기서는 view에 뿌려주기만 하면끝
            var matchResult = MatchingManager.Instance.CreateMatchResult();
            if (matchResult.Matches.Count == 0)
            {
                tbMatchingResultEmpty.Visibility = Visibility.Visible;
            }
            else
            {
                lvTeamResult.ItemsSource = matchResult.Matches;
                tbMatchingResultEmpty.Visibility = Visibility.Hidden;
            }
            lvRemainPeople.ItemsSource = matchResult.RemainUsers.nonMatchedUser;
        }

        private void BtnRematchingClicked(object sender, RoutedEventArgs e)
        {
            // 여기서는 view에 뿌려주기만 하면끝
            var matchResult = MatchingManager.Instance.CreateMatchResult();
            if (matchResult.Matches.Count == 0)
            {
                tbMatchingResultEmpty.Visibility = Visibility.Visible;
            }
            else
            {
                lvTeamResult.ItemsSource = matchResult.Matches;
                tbMatchingResultEmpty.Visibility = Visibility.Hidden;
            }
            lvRemainPeople.ItemsSource = matchResult.RemainUsers.nonMatchedUser;
        }
    }
}
