using NewMatchingBom.ViewModels;
using System.Windows;

namespace NewMatchingBom.Views
{
    public partial class MemberDetailWindow : HandyControl.Controls.Window
    {
        public MemberDetailWindow()
        {
            InitializeComponent();
        }

        public MemberDetailWindow(MemberDetailViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
            base.OnKeyDown(e);
        }
    }
}