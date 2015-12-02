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
    /// Interaction logic for CoopHostWindow.xaml
    /// </summary>
    public partial class CoopHostWindow : Window
    {
        public CoopHostWindow()
        {
            InitializeComponent();

            Settings settings = Settings.GetInstance();

            txtNickname.Text = (settings.HostNickname == null || settings.HostNickname.Equals("")) ? settings.UserID : settings.HostNickname;
            txtPort.Text = "" + settings.HostPort;
        }

        private void btnHostClicked(object sender, RoutedEventArgs e)
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

            settings.HostNickname = txtNickname.Text;
            settings.HostPort = port;
            settings.Save();

            CoopHost host = CoopHost.GetInstance();

            host.SetPassword(txtPassword.Text);
            host.SetPort(port);
            host.SetName(txtNickname.Text);
            
            host.StartHost();

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
    }
}
