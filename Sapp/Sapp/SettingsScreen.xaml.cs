using System;
using System.Collections.Generic;
using System.IO;
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
                txtSteamPath.Text = reference.SteamLocation;

                //only try and fill it with something if the settings file is not there, or corrupted
                if (reference.UserID == null)
                {
                    string testPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Steam";
                    if(File.Exists(testPath + @"\config\loginusers.vdf"))
                        txtSteamPath.Text = testPath;
                }

                reference.ReturnInstance(ref reference);
            }
        }

        private void btnAcceptClicked(object sender, RoutedEventArgs e)
        {
            Settings reference = Settings.GetInstance(this);

            //get rid of this when we move this CB
            reference.OnlyAllowInstalled = (bool)cbOnlyInstalled.IsChecked;

            //only save it if its valid
            if (File.Exists(txtSteamPath.Text + @"\config\loginusers.vdf"))
                reference.SteamLocation = txtSteamPath.Text;

            reference.UserID = txtUserID.Text;

            reference.Save();
            reference.ReturnInstance(ref reference);

           // PopulateGames();

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

        private void btnBrowseClicked(object sender, RoutedEventArgs e)
        {
            string testPath = "";

            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                testPath = dialog.SelectedPath;
            }

            txtSteamPath.Text = testPath;
            if (!File.Exists(testPath + @"\config\loginusers.vdf"))
                MessageBox.Show("Invalid Steam path selected.");

        }
    }
}
