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
                txtSteamPath.Text = reference.SteamLocation;
                cbxTagMethod.SelectedIndex = (int)reference.TagApplication;

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

            //only save it if its valid --SOMETHING IS HOLDING SETTINGS AND THIS IS BLOWING UP
            if (File.Exists(txtSteamPath.Text + @"\config\loginusers.vdf"))
                reference.SteamLocation = txtSteamPath.Text;

            reference.UserID = txtUserID.Text;
            reference.TagApplication = (TagApplicationMethod)cbxTagMethod.SelectedIndex;

            //save in mainwindow after we check if it should be refreshed
            reference.Save();
            reference.ReturnInstance(ref reference);

            DialogResult = true;
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
            DialogResult = false;
            this.Close(); 
        }

        private void btnBrowseClicked(object sender, RoutedEventArgs e)
        {
            string testPath = "";

            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                testPath = dialog.SelectedPath;
            

            txtSteamPath.Text = testPath;
            if (!File.Exists(testPath + @"\config\loginusers.vdf"))
                MessageBox.Show("Invalid Steam path selected.");

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
