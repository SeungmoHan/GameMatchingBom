using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using System.Windows.Threading;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;
using Test.Log;

namespace Test
{

    public partial class MainWindow
    {
        private ObservableCollection<string> logViews { get; set; }
        private DispatcherTimer timer { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            MatchingManager.Instance.Init();
            Task.Run(PeerlessManager.Instance.InitChampions);
            Stash.LogInfo("MemberLoad Clear");
            MainContent.Content = new HomeView(this);  // 홈 화면
            Closing += (a, b) => { Log.Stash.Flush(); };
            RefreshView();
            timer = new();
            timer.Interval = TimeSpan.FromSeconds(0.3);
            timer.Tick += ((sender, args) =>
            {
                RefreshView();
            });
            timer.Start();
        }

        private void RefreshView()
        {
            logViews = new(Stash.GetStashLogString());
            lvStashLog.ItemsSource = logViews;
            DataContext = this;
        }

        private void BtnHomeViewClicked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new HomeView(this); // 홈 화면
            MatchingManager.Instance.Reset();
        }

        private void BtnMatchingViewClicked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new MatchingResultView(this);// 결과창
        }

        private void BtnLogResetClicked(object sender, RoutedEventArgs e)
        {
            Stash.Clear();
            RefreshView();
        }

        private void BtnPeerlessViewClicked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new PeerlessView(this);
        }
    }
}