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

        public DisplayMessage(String messageHeader, String message, System.Windows.Forms.MessageBoxButtons buttons, bool showDontShow = false)
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
            if (showDontShow)
            {
                cbxDontShow.Visibility = System.Windows.Visibility.Visible;
                lblMessage.Margin = new Thickness(23, 52, 23, 81);
            }
        }

        private void btnNoClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void btnOkClicked(object sender, RoutedEventArgs e)
        {
            if (cbxDontShow.Visibility == System.Windows.Visibility.Visible)
                DialogResult = !cbxDontShow.IsChecked;
            else
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
