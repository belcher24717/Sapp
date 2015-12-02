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
        private List<Game> _customGames = new List<Game>();
        //private List<Game> _shallowCustomGames = new List<Game>();
        private List<Game> _editGames = new List<Game>();
        //private List<Game> _shallowEditGames = new List<Game>();

        private static List<Game> games = new List<Game>();
        private GamesList _gamePool;
        private GamesList _removedPool;
        private bool customTextFilterActive;
        private bool editTextFilterActive;
        private const string FILTER_TEXT = "Text Filter...";

        public SettingsScreen(GamesList gamePool, GamesList removedPool)
        {
            InitializeComponent();
            Settings settings = Settings.GetInstance();
            games = MainWindow.GetAllGames();

            _gamePool = gamePool;
            _removedPool = removedPool;

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
                    tabCustom.IsEnabled = false;
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

            //event handlers...
            textbox_customsearchfilter.TextChanged += new TextChangedEventHandler(txtSearchFilter_TextChanged);
            textbox_editsearchfilter.TextChanged += new TextChangedEventHandler(txtSearchFilter_TextChanged);

            populateCustomGamesList();
            populateEditGamesList();
            CenterWindowOnScreen();
        }

        //CREDIT: http://stackoverflow.com/questions/4019831/how-do-you-center-your-main-window-in-wpf
        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void populateEditGamesList()
        {

            if (games == null)
                return;

            _editGames = games;

            // if there ARE games...
            if (_editGames.Capacity > 0)
            {
                foreach (Game game in games)
                {
                    if (!listbox_editgames.Items.Contains(game.ToString()))
                    {
                        listbox_editgames.Items.Add(game.ToString());
                    }
                }
            }
        }

        private void populateCustomGamesList()
        {

            if (games == null)
                return;

            foreach (Game game in games)
            {
                if (game.GetAppID() < 0)
                {
                    if (!_customGames.Contains(game))
                    {
                        _customGames.Add(game);
                    }
                }
            }

            // if there ARE custom games...
            if (_customGames.Capacity > 0)
            {
                foreach (Game game in _customGames)
                {
                    if (!listbox_customgames.Items.Contains(game.ToString()))
                    {
                        listbox_customgames.Items.Add(game.ToString());
                    }
                }
            }
            
        }

        public void addCustomGame(Game game)
        {
            if (!_customGames.Contains(game))
            {
                _customGames.Add(game);
                listbox_customgames.Items.Add(game.ToString());
            }
        }

        public void removeCustomGame(Game game)
        {
            if (_customGames.Contains(game))
            {
                _customGames.Remove(game);
                listbox_customgames.Items.Remove(game.ToString());
            }
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
            string testPath = "";

            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                testPath = dialog.SelectedPath;
            

            txtSteamPath.Text = testPath;
            if (!File.Exists(testPath + @"\config\loginusers.vdf"))
                MessageBox.Show("Invalid Steam path selected.");

        }

        private void button_addcustomgame_Click(object sender, RoutedEventArgs e)
        {
            CustomGameWindowManager manager = new CustomGameWindowManager(Settings.Wizard.Custom, this, _gamePool, _removedPool);
            manager._wizard.SetManager(manager);
            manager._wizard.ShowDialog();
        }

        private void tagApplication_closed(object sender, EventArgs e)
        {
            if (cbxTagMethod.SelectedIndex == 0)
                lblFilters.Content = "Filters";
            else
                lblFilters.Content = "Filter";
        }

        private void button_editcustomgame_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_customgames.SelectedIndex < 0)
            {
                DisplayMessage dm = new DisplayMessage("No Game Selected...", "You must select a Custom Game first to edit.", System.Windows.Forms.MessageBoxButtons.OK);
                dm.ShowDialog();
            }
            else
            {
                int index = listbox_customgames.SelectedIndex;
                CustomGameWindowManager manager = new CustomGameWindowManager(Settings.Wizard.Custom, this, _gamePool, _removedPool, _customGames[index]);
                manager._wizard.SetManager(manager);
                manager._wizard.ShowDialog();
            }
        }

        private void button_removecustomgame_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_customgames.SelectedIndex < 0)
            {
                DisplayMessage dm = new DisplayMessage("No Game Selected...", "You must select a Custom Game first to remove.", System.Windows.Forms.MessageBoxButtons.OK);
                dm.ShowDialog();
            }
            else
            {
                DisplayMessage dm = new DisplayMessage("Remove Custom Game...", 
                                                       "Are you sure you want to remove " + listbox_customgames.SelectedItem.ToString() + "?", 
                                                       System.Windows.Forms.MessageBoxButtons.YesNo);
                if ((bool)(dm.ShowDialog()))
                {
                    int index = listbox_customgames.SelectedIndex;
                    listbox_customgames.Items.RemoveAt(index);
                    _gamePool.Remove(_customGames[index]);
                    _removedPool.Remove(_customGames[index]);

                    //Remove game from persistence...
                    string myId = Settings.GetInstance().SteamID64.ToString();
                    GamesList games = GameUtilities.LoadGameList(myId, "games");
                    games.Remove(_customGames[index]);
                    _customGames.RemoveAt(index);
                    GameUtilities.SaveGameList(games, myId, "games");
                }
            }
        }

        private void button_editgame_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_editgames.SelectedIndex < 0)
            {
                DisplayMessage dm = new DisplayMessage("No Game Selected...", "You must select a game first to edit.", System.Windows.Forms.MessageBoxButtons.OK);
                dm.ShowDialog();
            }
            else
            {
                int index = listbox_editgames.SelectedIndex;
                CustomGameWindowManager manager = new CustomGameWindowManager(Settings.Wizard.Edit, this, _gamePool, _removedPool, games[index]);
                manager._wizard.SetManager(manager);
                manager._wizard.ShowDialog();
            }
        }

        private void updateList(TextBox tb)
        {

            if (tb.Name.ToLower().Contains("custom") && !tb.Text.Equals(FILTER_TEXT))
            {

                iterateListItems(tb, new List<Game>(_customGames), listbox_customgames);
            }
            else if (tb.Name.ToLower().Contains("edit") && !tb.Text.Equals(FILTER_TEXT))
            {
                iterateListItems(tb, new List<Game>(_editGames), listbox_editgames);
            }
        }

        private void iterateListItems(TextBox tb, List<Game> list, ListBox listView)
        {
            List<Game> trimmedList = new List<Game>(list);
            foreach (Game game in list)
            {
                if (!game.Title.ToLower().Contains(tb.Text.ToLower()))
                {
                    trimmedList.Remove(game);
                }
            }

            listView.Items.Clear();
            foreach(Game game in trimmedList)
            {
                listView.Items.Add(game.ToString());
            }
        }

        private void txtSearchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Name.ToLower().Contains("custom"))
            {
                customTextFilterActive = true;
            }
            else if (tb.Name.ToLower().Contains("edit"))
            {
                editTextFilterActive = true;
            }

            updateList(tb);

            if (tb.Focusable)
                tb.Focus();
        }

        private void txtFilterSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            UpdateTextFilterSettings(sender, false);
        }

        private void txtFilterSearch_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (String.IsNullOrEmpty(tb.Text))
            {
                UpdateTextFilterSettings(sender, true);
            }
        }

        private void UpdateTextFilterSettings(object sender, bool lostFocus)
        {
            TextBox tb = (TextBox)sender;
            if (lostFocus)
            {
                //disable TextChanged event
                tb.TextChanged -= txtSearchFilter_TextChanged;

                tb.Text = FILTER_TEXT;
                tb.Foreground = Brushes.Gray;
                if (tb.Name.ToLower().Contains("custom"))
                {
                    customTextFilterActive = false;
                }
                else if (tb.Name.ToLower().Contains("edit"))
                {
                    editTextFilterActive = false;
                }
            }
            else
            {
                if (tb.Text.Equals(FILTER_TEXT))
                {
                    if ((tb.Name.ToLower().Contains("custom") && !customTextFilterActive) || (tb.Name.ToLower().Contains("edit") && !editTextFilterActive))
                    {
                        tb.Text = "";
                        tb.Foreground = Brushes.White;

                        //enable TextChanged event
                        tb.TextChanged += txtSearchFilter_TextChanged;
                    }
                }
            }

        }

    // end SettingsScreen
    }

}
