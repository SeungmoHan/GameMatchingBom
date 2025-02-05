using HandyControl.Controls;
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
using System.Windows.Shapes;
using Window = System.Windows.Window;

namespace Test
{
    /// <summary>
    /// AddMemberDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddMemberDialog : Window
    {
        public User originUser { get; set; }
        public AddMemberDialog()
        {
            InitializeComponent();

            cbUserMainLine.ItemsSource = Enum.GetValues(typeof(MainLine));
            cbUserTierType.ItemsSource = Enum.GetValues(typeof(UserTier));
        }

        void SetUser(User origin)
        {
            originUser = origin;
        }


        private void btnAddMemberCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddMemberClick(object sender, RoutedEventArgs e)
        {
            bool addNewUser = originUser == null;

            User? newUser = null;
            if (true == addNewUser)
                newUser = new User();
            else
                newUser = originUser;

            newUser.Name = tbAddBoxBamName.Text;
            newUser.NickName = tbAddBoxLolName.Text;
            newUser.Tier = (UserTier)Enum.Parse(typeof(UserTier), cbUserTierType.Text);
            newUser.MainLine = (MainLine)Enum.Parse(typeof(MainLine), cbUserMainLine.Text);

            if (addNewUser)
                MemberLoader.Instance.AddMemberList(newUser);
            else
                MemberLoader.Instance.Save();

            this.Close();
        }
    }
}
