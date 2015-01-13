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

                //going to have to add this for each new checkbox...
                cbHoursPlayed.IsChecked = reference.GetColumnsToShow().Contains(cbHoursPlayed.Content.ToString());


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
            if (reference == null)
                Logger.Log("Settings reference is null");

            //Save where steam is
            if (File.Exists(txtSteamPath.Text + @"\config\loginusers.vdf"))
                reference.SteamLocation = txtSteamPath.Text;
            else
            {
                MessageBox.Show("Steam path is invalid.");
                goto InvalidInformation;
            }

            //save the user ID and username
            reference.UserID = txtUserID.Text;
            if (reference.UserID == null)
                goto InvalidInformation;
            
            //save the tag application method
            reference.TagApplication = (TagApplicationMethod)cbxTagMethod.SelectedIndex;

            Logger.Log("Saving Settings");
            reference.Save();
            reference.ReturnInstance(ref reference);

            DialogResult = true;
            this.Close();

            return;

            InvalidInformation:
                reference.ReturnInstance(ref reference);
            

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

        private void cbChanged(object sender, RoutedEventArgs e)
        {
            Settings settings = Settings.GetInstance(this);
            if (settings == null)
                return;

            CheckBox tempCB = (CheckBox)sender;
            
            if ((bool)tempCB.IsChecked)
                settings.AddColumn(tempCB.Content.ToString());
            else
                settings.RemoveColumn(tempCB.Content.ToString());

            settings.ReturnInstance(ref settings);
                
        }

    }
}
