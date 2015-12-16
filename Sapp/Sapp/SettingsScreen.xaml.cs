using Microsoft.Win32;
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
        private const string FILTER_TEXT = "Text Filter...";

        public SettingsScreen(GamesList gamePool, GamesList removedPool)
        {
            InitializeComponent();
            Settings settings = Settings.GetInstance();

            if (settings != null)
            {
                txtUserID.Text = settings.UserID;
                txtSteamPath.Text = settings.SteamLocation;
                cbxTagMethod.SelectedIndex = (int)settings.TagApplication;

                //going to have to add this for each new checkbox...
                cbHoursPlayed.IsChecked = settings.GetColumnsToShow().Contains(cbHoursPlayed.Content.ToString());
                cbHoursPlayedLast2Weeks.IsChecked = settings.GetColumnsToShow().Contains(cbHoursPlayedLast2Weeks.Content.ToString());
                cbIsInstalled.IsChecked = settings.GetColumnsToShow().Contains(cbIsInstalled.Content.ToString());

                //only installed
                cbOnlyInstalled.IsChecked = settings.OnlyPlayInstalledGames;

                //only try and fill it with something if the settings file is not there, or corrupted
                if (settings.UserID == null)
                {
                    tabColumns.IsEnabled = false;
                    tabFilter.IsEnabled = false;

                    RegistryKey regKey = Registry.CurrentUser;
                    regKey = regKey.OpenSubKey(@"Software\Valve\Steam");
                    if(regKey != null)
                        txtSteamPath.Text = regKey.GetValue("SteamPath").ToString();

                    //Default to user information tab when we don't have that information already (First run)
                    tcSettingsTab.SelectedIndex = 2;
                }

                tagApplication_closed(null, null);
            }
        }

        private void tcSettingsTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = e.OriginalSource as TabControl;
            TabItem tabSelected = tabControl == null ? null : tabControl.SelectedItem as TabItem;
            if (tabSelected != null && tabSelected.Focusable)
                tabSelected.Focus();
        }

        private void btnAcceptClicked(object sender, RoutedEventArgs e)
        {
            Settings settings = Settings.GetInstance();
            if (settings == null)
            {
                Logger.LogError("<SettingsScreen.btnAcceptClicked> Settings reference is null");
                return;
            }

            //Save where steam is
            if (File.Exists(txtSteamPath.Text + @"\config\loginusers.vdf"))
                settings.SteamLocation = txtSteamPath.Text;
            else
            {
                MessageBox.Show("Steam path is invalid.");
                goto InvalidInformation;
            }

            //save the user ID and username
            settings.UserID = txtUserID.Text;
            if (settings.UserID == null)
                goto InvalidInformation;
            
            //save the tag application method
            settings.TagApplication = (TagApplicationMethod)cbxTagMethod.SelectedIndex;

            //save Only Play Installed Games
            settings.OnlyPlayInstalledGames = (bool)cbOnlyInstalled.IsChecked;

            //Add or remove columns
            #region Update Columns

            //Hours Played
            if ((bool)cbHoursPlayed.IsChecked)
                settings.AddColumn(cbHoursPlayed.Content.ToString());
            else
                settings.RemoveColumn(cbHoursPlayed.Content.ToString());

            //Hours Last 2 Weeks
            if ((bool)cbHoursPlayedLast2Weeks.IsChecked)
                settings.AddColumn(cbHoursPlayedLast2Weeks.Content.ToString());
            else
                settings.RemoveColumn(cbHoursPlayedLast2Weeks.Content.ToString());

            //Is Installed
            if ((bool)cbIsInstalled.IsChecked)
                settings.AddColumn(cbIsInstalled.Content.ToString());
            else
                settings.RemoveColumn(cbIsInstalled.Content.ToString());

            #endregion

            Logger.Log("Saving Settings");
            settings.Save();

            DialogResult = true;
            this.Close();

            //TODO: Add Error Window logic here
            InvalidInformation:
                return;
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
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            System.Windows.Forms.DialogResult dr = dialog.ShowDialog();

            if (dr != System.Windows.Forms.DialogResult.Cancel)
                txtSteamPath.Text = dialog.SelectedPath;

            if (!txtSteamPath.Text.Equals("") && !File.Exists(txtSteamPath.Text + @"\config\loginusers.vdf"))
                MessageBox.Show("Invalid Steam path selected.");

        }

        private void tagApplication_closed(object sender, EventArgs e)
        {
            if (cbxTagMethod.SelectedIndex == 0)
                lblFilters.Content = "Filters";
            else
                lblFilters.Content = "Filter";
        }
    
    }// end SettingsScreen

}
