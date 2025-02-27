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


    public partial class PeerlessView : UserControl
    {
        public MainWindow mainWindow;
        
        public PeerlessView(MainWindow parent)
        {
            mainWindow = parent;
            InitializeComponent();
            cbPeopleCountBox.ItemsSource = new int[]{ 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20 };
            RefreshView();
        }

        private bool RandomPickUIFlag
        {
            set
            {
                btnRandomChoice.IsEnabled = value;
                cbPeopleCountBox.IsEnabled = value;
            }
        }

        private void RefreshView()
        {
            int currentCount = PeerlessManager.Instance.CurrentChampionCount;
            if (PeerlessManager.Instance.TotalChampionCount == currentCount)
            {
                var remainedChampion = PeerlessManager.Instance.RemainedChampions.OrderBy(o => o.ChampionNameKor).ToList();
                var inThisGame = PeerlessManager.Instance.SelectedChampionInThisGame.OrderBy(o => o.ChampionNameKor).ToList();
                var usedChampion = PeerlessManager.Instance.UsedChampions.OrderBy(o => o.ChampionNameKor).ToList();

                var remainCount = remainedChampion.Count;
                var inThisGameCount = inThisGame.Count;
                var usedCount = usedChampion.Count;

                if (false == string.IsNullOrEmpty(tbChampionSearchBox.Text))
                {
                    remainedChampion = remainedChampion.Where(item => item.ChampionNameKor.Contains(tbChampionSearchBox.Text)).ToList();
                    inThisGame = inThisGame.Where(item => item.ChampionNameKor.Contains(tbChampionSearchBox.Text)).ToList();
                    usedChampion = usedChampion.Where(item => item.ChampionNameKor.Contains(tbChampionSearchBox.Text)).ToList();
                }

                AllChampionListView.ItemsSource = remainedChampion;
                ChampionListViewInThisGame.ItemsSource = inThisGame;
                UsedChampionsListView.ItemsSource = usedChampion;

                tbNotSelect.Text = $"Not Selected({remainCount})";
                tbInThisGame.Text = $"In This Game({inThisGameCount})";
                tbAlreadyUsed.Text = $"Used({usedCount})";
            }
            tbCurrentCount.Text = currentCount.ToString();
            cbPeopleCountBox.Text = PeerlessManager.Instance.SelectedCount.ToString();

            RandomPickUIFlag = PeerlessManager.Instance.IsEnalbeSelect();
        }


        private void AddUsedChampionInThisGame_dblClicked(object sender, MouseButtonEventArgs e)
        {
            if (AllChampionListView.SelectedItem is ChampionInfo selectedItem)
            {
                if(PeerlessManager.Instance.SelectedCount <= PeerlessManager.Instance.SelectedChampionInThisGame.Count)
                {
                    HandyControl.Controls.MessageBox.Show($"더이상 추가 불가능!!", $"최대 {PeerlessManager.Instance.SelectedCount}명");
                    return;
                }
                PeerlessManager.Instance.AddSelectedChampion(selectedItem);
                tbChampionSearchBox.Text = string.Empty;
                RefreshView();
            }
        }

        private void RemoveUsedChampionInThisGame_dblClicked(object sender, MouseButtonEventArgs e)
        {
            if (ChampionListViewInThisGame.SelectedItem is ChampionInfo selectedItem)
            {
                int choiceChampionCount = int.Parse(cbPeopleCountBox.Text);
                PeerlessManager.Instance.RemoveSelectedChampion(selectedItem);
                tbChampionSearchBox.Text = string.Empty;
                RefreshView();
            }
        }

        private void BtnCommitCharacters(object sender, RoutedEventArgs e)
        {
            RandomPickUIFlag = true;
            PeerlessManager.Instance.CommitSelectedChampion();
            tbChampionSearchBox.Text = string.Empty;
            RefreshView();
        }

        private void BtnRevertCharacters(object sender, RoutedEventArgs e)
        {
            RandomPickUIFlag = true;
            PeerlessManager.Instance.RevertSelectedChampion();
            tbChampionSearchBox.Text = string.Empty;
            RefreshView();
        }

        private void BtnRandomChoiceChampion(object sender, RoutedEventArgs e)
        {
            RandomPickUIFlag = false;
            int pickCount = int.Parse(cbPeopleCountBox.Text);
            int currentPickedCount = PeerlessManager.Instance.SelectedChampionInThisGame.Count;
            PeerlessManager.Instance.PickRandomChampion(pickCount - currentPickedCount);
            tbChampionSearchBox.Text = string.Empty;
            RefreshView();
        }

        private void OnParticipationCountChanged(object sender, EventArgs e)
        {
            int pickCount = int.Parse(cbPeopleCountBox.Text);
            if (PeerlessManager.Instance.SelectedChampionInThisGame.Count > pickCount)
            {
                HandyControl.Controls.MessageBox.Show($"최대 인원({PeerlessManager.Instance.SelectedChampionInThisGame.Count})보다 작은 숫자는 불가능해염");
                pickCount = PeerlessManager.Instance.SelectedChampionInThisGame.Count;
            }
            PeerlessManager.Instance.SelectedCount = pickCount;
            RefreshView();
        }

        private void ChampionSearchChanged(object sender, TextChangedEventArgs e)
        {
            RefreshView();
        }

        private void BtnRevertUsedCharacter(object sender, RoutedEventArgs e)
        {
            var result = HandyControl.Controls.MessageBox.Show($"진짜 삭제하는거 맞지?", "경고!", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                result = HandyControl.Controls.MessageBox.Show($"다시한번 묻는다?", "경고!", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    HandyControl.Controls.MessageBox.Show($"네 삭제하겠습니다", "낙장불입");
                    PeerlessManager.Instance.RevertUsedItems();
                }
            }

            RefreshView();
        }

        private void BtnRemoveUsedCharacter(object sender, RoutedEventArgs e)
        {
            var selectedItems = UsedChampionsListView.SelectedItems;
            List<ChampionInfo> items = new();
            foreach (var item in selectedItems)
            {
                ChampionInfo? champ = item as ChampionInfo;
                if (null == champ)
                    continue;
                items.Add(champ);
            }

            if (items.Count > 0)
            {
                var result = HandyControl.Controls.MessageBox.Show($"진짜 삭제하는거 맞지?", "경고!", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    result = HandyControl.Controls.MessageBox.Show($"다시한번 묻는다?", "경고!", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        HandyControl.Controls.MessageBox.Show($"네 삭제하겠습니다", "낙장불입");
                        PeerlessManager.Instance.RemoveUsedItem(items);
                    }
                }
            }
            RefreshView();
        }
    }
}
