using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;


namespace Sapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static GamesList gamePool;  
        private static GamesList removedPool;

        private bool onlyPlayInstalledGames;

        private List<GameUtilities.Tags> tagsCheckedInclude;
        private List<GameUtilities.Tags> tagsCheckedExclude;
        private List<CheckBox> checkboxesChecked;
        private bool checkboxesActive;
        private bool textFilterActive;

        private DataGridHandler gamePoolHandler;
        private DataGridHandler removedPoolHandler;

        private const int MIN_WINDOW_SIZE = 500;
        private const int MAX_WINDOW_SIZE = 850;
        private const int NUM_HOUR_FILTERS = 2;
        private const string FILTER_TEXT = "Text Filter...";

        private bool populateGamesSuccessful;

        private string lastEnteredText;

        /*private Visibility DisconnectVisibility
        {
            get { return CoopUtils.HostListening || CoopUtils.JoinListening ? Visibility.Visible : Visibility.Hidden; }
        }*/

        public MainWindow()
        {

            //TODO: Move a lot of this logic into an onload event

            InitializeComponent();
            //Initially hide hidden games

            Logger.Log("\n---------------------------------------------------------------------------");
            Logger.Log("Main Application Started", true);

            bool settingsLoaded = false;
            lastEnteredText = "";
            Nullable<bool> windowAccepted = true;

            while (!settingsLoaded)
            {
                try
                {
                    Settings.Initialize();

                    checkboxesActive = false;
                    gamePool = new GamesList();
                    removedPool = new GamesList();
                    tagsCheckedInclude = new List<GameUtilities.Tags>();
                    tagsCheckedExclude = new List<GameUtilities.Tags>();
                    checkboxesChecked = new List<CheckBox>();

                    SetRectangleSize();
                    UpdateTextFilterSettings(true);

                    settingsLoaded = true;
                    textFilterActive = false;
                }
                catch (FileNotFoundException fileNotFound)
                {
                    Logger.LogWarning("<MainWindow.MainWindow> Settings File Not Found", true);
                    SettingsScreen ss = new SettingsScreen(gamePool, removedPool);
                    windowAccepted = ss.ShowDialog();
                }
                catch (SerializationException serializationFailed)
                {
                    Logger.LogWarning("<MainWindow.MainWindow> Settings File Corrupted", true);
                    SettingsScreen ss = new SettingsScreen(gamePool, removedPool);
                    windowAccepted = ss.ShowDialog();
                }

                if (windowAccepted != null && !(bool)windowAccepted)
                {
                    settingsLoaded = true;
                    this.Close();
                }
            }

            if (windowAccepted != null && (bool)windowAccepted)
            {
                gamePoolHandler = new DataGridHandler(ref dgGamePool);
                removedPoolHandler = new DataGridHandler(ref dgRemovedPool);

                Logger.Log("START: Populating Games", true);

                populateGamesSuccessful = PopulateGames();

                Logger.Log("END: Populating Games", true);

                //TODO: Make all the checkboxes, and then all references to a list here.
                //This list will make management of all checkboxes easy (seperate by type, like: tag filters, intalled only, hours played, etc)

                //hoursPlayedHandler = new HoursHandler(ref chkbxHoursPlayed, ref lblPreHoursPlayed, ref lblPostHoursPlayed, ref combobox_HoursPlayed, ref textbox_HoursPlayed);
                //hoursLast2WeeksHandler = new HoursHandler(ref chkbxHoursPlayedLast2Weeks, ref lblPreHoursPlayedLast2Weeks,
                //    ref lblPostHoursPlayedLast2Weeks, ref combobox_HoursPlayedLast2Weeks, ref textbox_HoursPlayedLast2Weeks);

                this.Width = MAX_WINDOW_SIZE;
                this.MaxWidth = MAX_WINDOW_SIZE;
                this.MinWidth = MAX_WINDOW_SIZE;

                onlyPlayInstalledGames = Settings.GetInstance().OnlyPlayInstalledGames;

                //Do a quick save at the start to save any message settings that may have occured.
                Settings.GetInstance().Save();

                BlanketUpdate(GetTagApplicationMethod());
            }
        }

        private void MouseDownOnWindow(object sender, MouseButtonEventArgs e)
        {
            if (this.ResizeMode != System.Windows.ResizeMode.NoResize)
            {
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
                this.UpdateLayout();
            }

            this.DragMove();
        }

        private void MouseUpOnWindow(object sender, MouseButtonEventArgs e)
        {
            if (this.ResizeMode == System.Windows.ResizeMode.NoResize)
            {
                // restore resize grips
                this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
                this.UpdateLayout();
            }
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

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //move to util, return the list
        //make a game manager to do a lot of this logic
        private bool PopulateGames()
        {
            Settings settings = Settings.GetInstance();
            if (settings == null)
                return false;
            
            //Populate, attach event
            string steamid64 = settings.SteamID64;

            gamePool = GameUtilities.PopulateGames(steamid64);

            if (gamePool == null)
                return false;

            gamePool.Changed += new ChangedEventHandler(gamePool_Changed);
            gamePoolHandler.Bind(gamePool);

            removedPool.Changed += new ChangedEventHandler(removedPool_Changed);
            removedPoolHandler.Bind(removedPool);

            return true;
        }

        public void RemoveDlc(int id)
        {
            lock (gamePool)
            {
                gamePool.RemoveNoNotify(gamePool.GetGame(id));
            }
        }

        public static GamesList GetAllMultiplayerGames()
        {
            if (gamePool == null || removedPool == null)
                return null;

            GamesList multiplayerGames = new GamesList();
            multiplayerGames.AddList(gamePool);
            multiplayerGames.AddList(removedPool);

            for (int i = 0; i < multiplayerGames.Count; i++)
            {
                if (!multiplayerGames[i].IsMultiplayerGame())
                {
                    multiplayerGames.RemoveAt(i);
                    i--;
                }
            }
            return multiplayerGames;
        }

        public static GamesList GetAllGames()
        {
            if (gamePool == null || removedPool == null)
                return null;

            GamesList allGames = new GamesList();
            allGames.AddList(gamePool);
            allGames.AddList(removedPool);
            return allGames;
        }

        public static GamesList GetGamePool()
        {
            GamesList allGamesInPool = new GamesList();
            allGamesInPool.AddList(gamePool);
            return allGamesInPool;
        }

        private void gamePool_Changed(object sender, EventArgs e)
        {
            gamePoolHandler.Refresh();
            //TODO: add some smarts to know how to sort (by title or by some other way) remember how user sorted last 
            gamePool.Sort();
        }

        private void removedPool_Changed(object sender, EventArgs e)
        {
            removedPoolHandler.Refresh();
            //TODO: add some smarts to know how to sort (by title or by some other way) remember how user sorted last 
            removedPool.Sort();
        }

        private void btnRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            if (CoopJoin.GetInstance().IsJoined())
                return;
            else if (CoopJoin.GetInstance().JustDisconnected())
                RelinkPools();

            Game itemToRemove = gamePoolHandler.GetSelectedItem();

            if (itemToRemove == null)
                return;

            removedPool.Add(itemToRemove);
            gamePool.Remove(itemToRemove);
            HostUpdate();
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            if (CoopJoin.GetInstance().IsJoined())
                return;
            else if (CoopJoin.GetInstance().JustDisconnected())
                RelinkPools();

            Game itemToRemove = removedPoolHandler.GetSelectedItem();

            if (itemToRemove == null || (CoopUtils.CollectivePool != null && !CoopUtils.CollectivePool.Contains(itemToRemove)))//TODO:ADDITIONAL TESTING
                return;

            gamePool.Add(itemToRemove);
            removedPool.Remove(itemToRemove);
            HostUpdate();
        }

        private void FixSelection(ListBox container)
        {
            if (container.Items.Count > 1)
            {
                if (container.SelectedIndex == container.Items.Count - 1)
                    container.SelectedIndex = container.Items.Count - 2;
                else
                    container.SelectedIndex += 1;

                container.Focus();
            }

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            // uncheck checkboxes - make copies of checkbox count and checkboxes for iteration
            int checkboxCount = checkboxesChecked.Count;
            List<CheckBox> copy = new List<CheckBox>(checkboxesChecked);

            for (int x = 0; x < checkboxCount; x++)
                copy[x].IsChecked = false;

            // turn off hours filters
            cb_HoursPlayed.IsChecked = false;
            cb_HoursPlayedLast2Weeks.IsChecked = false;
        }

        //maybe move this logic into util?
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (gamePool.Count == 0)
            {
                DisplayMessage noGames = new DisplayMessage("Play Error", "There are no games in the game pool!", System.Windows.Forms.MessageBoxButtons.OK);
                noGames.ShowDialog();
                return;
            }

            Random rand = new Random((DateTime.Now.Millisecond * DateTime.Now.Minute));
            int choiceGame = rand.Next(gamePool.Count);

            gamePool[choiceGame].Launch();
            if (CoopUtils.HostListening)
                CoopHost.GetInstance().Launch(gamePool[choiceGame].GetAppID());

            this.WindowState = WindowState.Minimized;

        }

        private void checkboxChanged(object sender, RoutedEventArgs e)
        {
            if (!checkboxesActive)
                return;

            CheckBox checkbox = (CheckBox)sender;
            GameUtilities.Tags tag = GameUtilities.CreateTag(checkbox.Content.ToString());

            if ((bool)(checkbox.IsChecked))
            {
                if (cbxIncludeExcludeTags.SelectedIndex == 0)
                {
                    tagsCheckedInclude.Add(tag);
                    tagsCheckedExclude.Remove(tag);
                }
                else
                {
                    checkbox.IsChecked = null;
                    tagsCheckedExclude.Add(tag);
                    tagsCheckedInclude.Remove(tag);
                }

                if (!checkboxesChecked.Contains(checkbox))
                    checkboxesChecked.Add(checkbox);
            }
            else
            {
                tagsCheckedInclude.Remove(tag);
                tagsCheckedExclude.Remove(tag);
                checkboxesChecked.Remove(checkbox);
            }

            BlanketUpdate(GetTagApplicationMethod());
        }

        private void cbChecked_OnlyInstalled(object sender, RoutedEventArgs e)
        {
            BlanketUpdate(GetTagApplicationMethod());
        } // end cbChecked_OnlyInstalled()

        public void BlanketUpdate()
        {
            Dispatcher.Invoke(new Action(() =>{BlanketUpdate(GetTagApplicationMethod());}) );
        }

        private void BlanketUpdate(TagApplicationMethod method)
        {
            if (tagsCheckedExclude == null || tagsCheckedInclude == null)
                return;

            if (CoopJoin.GetInstance().IsJoined())
                return;
            else if (CoopJoin.GetInstance().JustDisconnected())
            {
                RelinkPools();
            }
            bool thereAreTagsChecked = (tagsCheckedInclude.Count + tagsCheckedExclude.Count >= 1) ? true : false;
            bool onlyInstalledIsChecked = onlyPlayInstalledGames;
            bool hoursPlayedIsEnabled = (bool)cb_HoursPlayed.IsChecked;
            bool last2WeeksIsEnabled = (bool)cb_HoursPlayedLast2Weeks.IsChecked;

            // default values
            bool hoursPlayedGreaterThan = false; 
            bool last2WeeksGreaterThan = false; 
            double hoursPlayedHours = 0; 
            double last2WeeksHours = 0;
            List<Game> gamesToRemove = new List<Game>();

            // Get pools ready for update
            gamePool.AddList(removedPool);
            removedPool.Clear();

            #region hours pre-setup

            if (hoursPlayedIsEnabled)
            {
                hoursPlayedHours = Double.Parse(textbox_HoursPlayed.Text);
                hoursPlayedGreaterThan = (combobox_HoursPlayed.SelectedIndex == 0) ? true : false;
            }

            if (last2WeeksIsEnabled)
            {
                last2WeeksHours = Double.Parse(textbox_HoursPlayedLast2Weeks.Text);
                last2WeeksGreaterThan = (combobox_HoursPlayedLast2Weeks.SelectedIndex == 0) ? true : false;
            }

            #endregion

            #region Host Setup

            if (CoopUtils.HostListening)
            {
                if(CoopUtils.CollectivePool != null)
                {
                    foreach (Game game in gamePool)
                        if (!CoopUtils.CollectivePool.Contains(game) || !game.Multiplayer)
                            gamesToRemove.Add(game);

                    //remove them so they arent removed twice
                    removedPool.AddList(gamesToRemove);
                    gamePool.RemoveList(gamesToRemove);

                    gamesToRemove.Clear();
                }
            }

            #endregion

            // iterate through each game to finalize gamepool and removedpool
            foreach (Game game in gamePool)
            {
                #region Text Filter

                if (!textbox_searchfilter.Text.Equals(FILTER_TEXT))
                {
                    if (!game.Title.ToLower().Contains(textbox_searchfilter.Text.ToLower()))
                    {
                        gamesToRemove.Add(game);
                        continue;
                    }
                }

                #endregion

                #region Tags
                if (thereAreTagsChecked)
                {
                    if (!game.ContainsTag(tagsCheckedInclude, method))
                    {
                        gamesToRemove.Add(game);
                        continue; 
                    }

                    if (game.ContainsTag(tagsCheckedExclude, TagApplicationMethod.ContainsOne))
                    {
                        gamesToRemove.Add(game);
                        continue;
                    }
                }
                #endregion

                #region Only Installed
                if (onlyInstalledIsChecked)
                {
                    
                    if (game.IsInstalled == false)
                    {
                        gamesToRemove.Add(game);
                        continue;
                    }
                } // end onlyinstalled checkbox if
                #endregion

                #region Hours Played
                
                if (hoursPlayedIsEnabled)
                {
                    //TODO: Consider using method return for this?
                    if (hoursPlayedGreaterThan) //greaterThan
                    {
                        if (game.HoursPlayed < hoursPlayedHours) //hours
                        {
                            gamesToRemove.Add(game);
                            continue;
                        }
                    }
                    else // lessThan
                    {
                        if (game.HoursPlayed > hoursPlayedHours) //hours
                        {
                            gamesToRemove.Add(game);
                            continue;
                        }
                    } // end hours if/else

                } // end hoursplayed expander if
                #endregion 

                #region Hours Played Last 2 Weeks
                
                if (last2WeeksIsEnabled)
                {
                    if (last2WeeksGreaterThan)
                    {
                        if (game.Last2Weeks < last2WeeksHours)
                        {
                            gamesToRemove.Add(game);
                            continue;
                        }
                    }
                    else // lessThan
                    {
                        if (game.Last2Weeks > last2WeeksHours)
                        {
                            gamesToRemove.Add(game);
                            continue;
                        }
                    } // end hours if/else

                } // end hoursplayedLast2Weeks expander if
                #endregion

            } // end foreach

            //only 1 refresh per datagrid this way
            removedPool.AddList(gamesToRemove);
            gamePool.RemoveList(gamesToRemove);
            
            HostUpdate();
        }

        private void formLoaded(object sender, RoutedEventArgs e)
        {
            while (!populateGamesSuccessful)
            {

                if (MessageBox.Show("There was an error populating the game pool.\n\n\tWould you like to retry?", "Oops!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    populateGamesSuccessful = PopulateGames();
                }
                else
                {
                    this.Close();
                    break;
                }
            }

            checkboxesActive = true;
            this.Activate();//bring the window to the front
        }

        private void HostUpdate()
        {
            if (CoopHost.GetInstance().IsHosting())
                CoopHost.GetInstance().UpdateGamePool();
        }

        //TODO: Implement this! ESC -> Options, Enter -> Move game from list to other list, etc?
        private void onKeyDown(object sender, KeyEventArgs e)
        {
            //shouldn't do left/right arrow moving
            /*
            if (e.Key == Key.Left)
                btnAddGame_Click(sender, e);
            if (e.Key == Key.Right)
                btnRemoveGame_Click(sender, e);
            */
        }

        private void btnOpenOptions(object sender, RoutedEventArgs e)
        {
            TagApplicationMethod preMethod = GetTagApplicationMethod();
            SettingsScreen ss = new SettingsScreen(gamePool, removedPool);
            ss.ShowDialog();
            TagApplicationMethod postMethod = GetTagApplicationMethod();

            Settings testForRefresh = Settings.GetInstance();
            bool refresh = testForRefresh.ShouldRefresh();
            bool onlyRefreshGamePool = testForRefresh.ShouldRefreshGamePoolOnly();

            //do a little bit more than just test for refresh
            //-- get the new cols to show
            //--get only play installed games
            List<string> colsToShow = testForRefresh.GetColumnsToShow();
            onlyPlayInstalledGames = testForRefresh.OnlyPlayInstalledGames;

            gamePoolHandler.ClearColumns();
            removedPoolHandler.ClearColumns();

            foreach (string s in colsToShow)
            {
                gamePoolHandler.AddColumn(s);
                removedPoolHandler.AddColumn(s);
            }

            //+1 to include title column
            int width = 290 / (colsToShow.Count + 1);
            for (int i = 0; i < colsToShow.Count + 1; i++)
            {
                dgGamePool.Columns[i].Width = width;
                dgRemovedPool.Columns[i].Width = width;
            }

            if (refresh)
            {
                gamePool.Clear();
                removedPool.Clear();
                //ResetCheckboxes();
                PopulateGames();
                tagsCheckedInclude.Clear();
                tagsCheckedExclude.Clear();
            }
            else if (!refresh && preMethod != postMethod) // if they change contains method, update list occordingly.           
                BlanketUpdate(postMethod);
            else if (onlyRefreshGamePool)
                BlanketUpdate(postMethod);
            
        }

        private TagApplicationMethod GetTagApplicationMethod()
        {
            Settings grabTagAppMethod = Settings.GetInstance();
            TagApplicationMethod method = grabTagAppMethod.TagApplication;

            return method;
        }

        private void btnMinimizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CheckboxEnableChanged_Hours(object sender, RoutedEventArgs e)
        {
            BlanketUpdate(GetTagApplicationMethod());
        }

        // these may end up being removed
        #region HoursHelperEvents

        private void HoursGtLgComboBoxChanged(object sender, SelectionChangedEventArgs e)
        {
            if (removedPool != null) // this should only happen on load
                BlanketUpdate(GetTagApplicationMethod());
        }

        private void HoursTextChanged(object sender, TextChangedEventArgs e)
        {
            if (removedPool != null) // this should only happen on load
            {
                TextBox tb = (TextBox)sender;
                string textToVerify;

                //TODO:  May implement last entered text option here, and move validation here instead of in blanketupdate
                if (tb.Name.Equals("textbox_HoursPlayedLast2Weeks"))
                    textToVerify = textbox_HoursPlayedLast2Weeks.Text;
                else if (tb.Name.Equals("textbox_HoursPlayed"))
                    textToVerify = textbox_HoursPlayed.Text;
                else
                {
                    Logger.LogWarning("<MainWindow.HoursTextChanged> HoursTextChanged event fired, but not from any handled Textbox.", true);
                    return;
                }

                if (Verify(textToVerify))
                {
                    lastEnteredText = textToVerify;
                    BlanketUpdate(GetTagApplicationMethod());
                }
                else if (textToVerify.Equals(""))
                {
                    lastEnteredText = "0";
                    tb.Text = "0";
                    BlanketUpdate(GetTagApplicationMethod());
                }
                else // text failed check, revert to old text...
                    tb.Text = lastEnteredText;

                if (tb.Focusable)
                    tb.Focus();
            }
        }

        private bool Verify(string text)
        {
            return VerificationClass.VerifyHours(text);
        }

        #endregion

        private void btnClickRemoveAll(object sender, RoutedEventArgs e)
        {
            if (CoopJoin.GetInstance().IsJoined())
                return;
            else if (CoopJoin.GetInstance().JustDisconnected())
                RelinkPools();

            foreach (Game game in gamePool)
                removedPool.Add(game);

            gamePool.Clear();
            HostUpdate();
        }

        private void btnClickAddAll(object sender, RoutedEventArgs e)
        {
            if (CoopJoin.GetInstance().IsJoined())
                return;
            else if (CoopJoin.GetInstance().JustDisconnected())
                RelinkPools();

            if (CoopUtils.CollectivePool == null)
            {
                foreach (Game game in removedPool)
                {
                    gamePool.Add(game);
                }
                removedPool.Clear();
            }

            else
                for(int i = 0; i < removedPool.Count; i++)//each (Game game in removedPool)
                {
                    if (CoopUtils.CollectivePool.Contains(removedPool[i]))
                    {
                        gamePool.Add(removedPool[i]);
                        removedPool.Remove(removedPool[i]);
                        i--;
                    }
                }

            HostUpdate();
        }

        private void btnSaveGamePool(object sender, RoutedEventArgs e)
        {
            String filename = RequestFilename(false);

            if (filename == null)
                return;

            if (filename != null)
            {
                GamesList tempListToSave = new GamesList();
                tempListToSave.AddList(gamePool.ToList<Game>());

                if(!Directory.Exists(@".\saves"))
                    Directory.CreateDirectory(@".\saves");

                GameUtilities.SaveGameList(tempListToSave, filename, "gp", true);
            }
        }

        private void btnLoadGamePool(object sender, RoutedEventArgs e)
        {
            //open file browser to search for .gp files
            String filename = RequestFilename(true);

            if (filename == null)
                return;

            GamesList tempGamePool = GameUtilities.LoadGameList(filename, "gp", true);

            btnClickRemoveAll(null, new RoutedEventArgs());

            foreach(Game game in tempGamePool)
            {
                gamePool.Add(game);
                removedPool.Remove(game);
            }
        }

        private String RequestFilename(bool load)
        {
            FileDialog dlg = null;

            String filename = null;
            String initialDirectory = Directory.Exists(Directory.GetCurrentDirectory() + "\\saves") ? Directory.GetCurrentDirectory() + "\\saves" : "DEFAULT";
            
            if(load)
                dlg = new Microsoft.Win32.OpenFileDialog();
            else
                dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".gp";
            dlg.Filter = "GAMEPOOL Files (*.gp)|*.gp";

            if (!initialDirectory.Equals("DEFAULT"))
                dlg.InitialDirectory = initialDirectory;

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
                filename = dlg.FileName;
                
            return filename;
        }

        private void btnOpenHiddenGames_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width < MAX_WINDOW_SIZE)
            {
                btnOpenHiddenGamesArrow.Content = "<";

                //BEFORE POP: Show the add/remove buttons while the window is not expanded
                btnAddAll.Visibility = Visibility.Visible;
                btnAddGame.Visibility = Visibility.Visible;
                btnRemoveAll.Visibility = Visibility.Visible;
                btnRemoveGame.Visibility = Visibility.Visible;

                this.MaxWidth = MAX_WINDOW_SIZE;
                this.MinWidth = MAX_WINDOW_SIZE;

                btnOpenHiddenGamesArrow.ToolTip = "Hide Removed";
            }
            else
            {
                btnOpenHiddenGamesArrow.Content = ">";

                this.MinWidth = MIN_WINDOW_SIZE;
                this.MaxWidth = MIN_WINDOW_SIZE;

                //AFTER POP: Hide the add/remove buttons while the window is not expanded
                btnAddAll.Visibility = Visibility.Hidden;
                btnAddGame.Visibility = Visibility.Hidden;
                btnRemoveAll.Visibility = Visibility.Hidden;
                btnRemoveGame.Visibility = Visibility.Hidden;

                btnOpenHiddenGamesArrow.ToolTip = "Show Removed";
            }
        }

        private void pool_datagrid_KeyDown(object sender, KeyEventArgs e)
        {
            Game selected;
            DataGrid gridSending;
            string keyPressed = e.Key.ToString();

            try
            {
                gridSending = (DataGrid)sender;
            }
            catch
            {
                return;
            }

            if (gridSending.Name.Equals("dgGamePool"))
            {
                gridSending.SelectedItem = selected = gamePool.Find(x => x.Title.StartsWith(keyPressed));
            }
            else
            {
                gridSending.SelectedItem = selected = removedPool.Find(x => x.Title.StartsWith(keyPressed));
            }

            if(selected != null)
                gridSending.ScrollIntoView(selected);
        }

        private Key GetKeybind()
        {
            Key keybind;
            Settings temp = Settings.GetInstance();

            if (temp == null)
                return Key.None;

            keybind = temp.GamePoolRemoveKeyBinding;

            return keybind;
        }

        private void event_closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CoopHost.GetInstance().IsHosting())
            {
                CoopHost.GetInstance().StopHost();
            }
            else if (CoopJoin.GetInstance().IsJoined())
            {
                CoopJoin.GetInstance().Disconnect();
            }
        }

        private void btnHostClick(object sender, RoutedEventArgs e)
        {
            if (CoopHost.GetInstance().IsHosting() || CoopJoin.GetInstance().IsJoined())
                return;

            FriendsList.GetInstance().SetList(ref tbFriendsConnected, ref lblNumFriends);
            CoopHost.GetInstance().SetBlanketUpdateMethod(BlanketUpdate);

            CoopHostWindow chw = new CoopHostWindow();
            chw.ShowDialog();
        }

        private void btnJoinClick(object sender, RoutedEventArgs e)
        {
            if (CoopJoin.GetInstance().IsJoined() || CoopHost.GetInstance().IsHosting())
                return;

            FriendsList.GetInstance().SetList(ref tbFriendsConnected, ref lblNumFriends);

            GamesList temp = new GamesList();
            temp.AddList(gamePool);
            temp.AddList(removedPool);

            CoopJoinWindow cjw = new CoopJoinWindow(temp);
            cjw.ShowDialog();

            CoopJoin.GetInstance().SetGamePools(ref dgGamePool, ref dgRemovedPool);
        }

        private void btnDisconnectClick(object sender, RoutedEventArgs e)
        {
            if (CoopUtils.HostListening)
                CoopHost.GetInstance().StopHost();
            else if (CoopUtils.JoinListening)
            {
                CoopJoin.GetInstance().Disconnect();
                RelinkPools();
            }
        }

        private void RelinkPools()
        {
            dgGamePool.ItemsSource = gamePool;
            dgRemovedPool.ItemsSource = removedPool;
            CoopJoin.GetInstance().GamesRelinked();
        }

        private void txtSearchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            textFilterActive = true;

            BlanketUpdate(GetTagApplicationMethod());

            if (textbox_searchfilter.Focusable)
                textbox_searchfilter.Focus();
        }

        private void txtFilterSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            UpdateTextFilterSettings(false);
        }

        private void txtFilterSearch_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(textbox_searchfilter.Text))
            {
                UpdateTextFilterSettings(true);
            }
        }

        private void UpdateTextFilterSettings(bool lostFocus)
        {
            if (lostFocus)
            {
                //disable TextChanged event
                textbox_searchfilter.TextChanged -= txtSearchFilter_TextChanged;

                textbox_searchfilter.Text = FILTER_TEXT;
                textbox_searchfilter.Foreground = Brushes.Gray;
                textFilterActive = false;
            }
            else
            {
                if (textbox_searchfilter.Text.Equals(FILTER_TEXT) && !textFilterActive)
                {
                    textbox_searchfilter.Text = "";
                    textbox_searchfilter.Foreground = Brushes.White;

                    //enable TextChanged event
                    textbox_searchfilter.TextChanged += txtSearchFilter_TextChanged;
                }

            }

        }

        private void btnAddEditGames_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not Yet Implemented");
        }
    }

}