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
    /// Interaction logic for DisplayMessage.xaml
    /// </summary>
    public partial class DisplayMessage : Window
    {

        public DisplayMessage(String messageHeader, String message, System.Windows.Forms.MessageBoxButtons buttons)
        {
            InitializeComponent();

            lblMessage.Content = message;
            lblMessageHeader.Content = messageHeader;

            if (buttons == System.Windows.Forms.MessageBoxButtons.YesNo)
            {
                btnOkay.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (buttons == System.Windows.Forms.MessageBoxButtons.OK)
            {
                btnNo.Visibility = System.Windows.Visibility.Hidden;
                btnYes.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void btnNoOkayClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void btnYesClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void MouseDownOnWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

    }
}
