using Microsoft.Extensions.DependencyInjection;
using NewMatchingBom.ViewModels;
using System;
using System.Windows;

namespace NewMatchingBom.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // App.Current를 통해 서비스 프로바이더 접근
            if (Application.Current is App app && app.ServiceProvider != null)
            {
                var viewModel = app.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                DataContext = viewModel;
            }
        }
    }
}