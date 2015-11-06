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

namespace Sapp
{
    /// <summary>
    /// Interaction logic for CoopJoinWindow.xaml
    /// </summary>
    public partial class CoopJoinWindow : Window
    {
        private GamesList gameList;

        public CoopJoinWindow(GamesList list)
        {
            InitializeComponent();
            gameList = list;

            Settings settings = Settings.GetInstance();
            txtIpAddress.Text = settings.JoinIpAddress;
            txtNickname.Text = settings.JoinNickname.Equals("") ? settings.UserID : settings.JoinNickname;
            txtPort.Text = "" + settings.JoinPort;
        }

        private void btnAcceptClicked(object sender, RoutedEventArgs e)
        {
            int port;
            try
            {
                port = int.Parse(txtPort.Text);
            }
            catch
            {
                return;
            }

            Settings settings = Settings.GetInstance();

            settings.JoinNickname = txtNickname.Text;
            settings.JoinPort = port;
            settings.JoinIpAddress = txtIpAddress.Text;
            settings.Save();

            CoopJoin tryJoin = CoopJoin.GetInstance();

            tryJoin.SetName(txtNickname.Text);
            tryJoin.SetIpJoining(txtIpAddress.Text);
            tryJoin.SetPassword(txtPassword.Text);
            tryJoin.SetPort(port);
            tryJoin.SetGamesList(gameList);

            tryJoin.Join();

            this.Close();
        }

        private void btnCancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MouseDownOnWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            txtIpAddress.Focus();
        }
    }
}
