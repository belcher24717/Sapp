using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for SettingsScreen.xaml
    /// </summary>
    public partial class SettingsScreen : Window
    {
        public SettingsScreen()
        {
            InitializeComponent();
            Settings reference = Settings.GetInstance(this);

            if (reference != null)
            {
                txtUserID.Text = reference.UserID;
                cbOnlyInstalled.IsChecked = reference.OnlyAllowInstalled;

                reference.ReturnInstance(ref reference);
            }
        }

        private void btnAcceptClicked(object sender, RoutedEventArgs e)
        {
            Settings reference = Settings.GetInstance(this);

            reference.OnlyAllowInstalled = (bool)cbOnlyInstalled.IsChecked;
            reference.UserID = txtUserID.Text;

            reference.Save();
            reference.ReturnInstance(ref reference);

            this.Close();
        }

        private void MouseDownOnWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetRectangleSize();
        }

        private void SetRectangleSize()
        {
            MainRectangle.Height = this.Height + 2;
            MainRectangle.Width = this.Width + 2;
        }

        private void btnCancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
