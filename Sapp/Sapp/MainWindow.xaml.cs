using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        private GamesList removedPool;

        private List<GameUtilities.Tags> tagsChecked;
        private bool checkboxesActive;

        private DataGridHandler gamePoolHandler;
        private DataGridHandler removedPoolHandler;

        private HoursHandler hoursPlayedHandler;
        private HoursHandler hoursLast2WeeksHandler;

        private const int MIN_WINDOW_SIZE = 500;
        private const int MAX_WINDOW_SIZE = 850;
        private const int NUM_HOUR_FILTERS = 2;

        public MainWindow()
        {
            InitializeComponent();
            //Initially hide hidden games
            

            Logger.Log("Main Application Started", true);

            bool settingsLoaded = false;
            Nullable<bool> windowAccepted = true;

            while (!settingsLoaded)
            {

                try
                {
                    Settings.Initialize();

                    checkboxesActive = false;
                    gamePool = new GamesList();
                    removedPool = new GamesList();
                    tagsChecked = new List<GameUtilities.Tags>();

                    SetRectangleSize();

                    settingsLoaded = true;
                }
                catch (FileNotFoundException fileNotFound)
                {
                    Logger.Log("Settings File Not Found", true);
                    SettingsScreen ss = new SettingsScreen();
                    windowAccepted = ss.ShowDialog();
                }
                catch (SerializationException serializationFailed)
                {
                    Logger.Log("Settings File Corrupted", true);
                    SettingsScreen ss = new SettingsScreen();
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

                PopulateGames();

                Logger.Log("END: Populating Games", true);

                //TODO: Make all the checkboxes, and then all references to a list here.
                //This list will make management of all checkboxes easy (seperate by type, like: tag filters, intalled only, hours played, etc)

                //hoursPlayedHandler = new HoursHandler(ref chkbxHoursPlayed, ref lblPreHoursPlayed, ref lblPostHoursPlayed, ref combobox_HoursPlayed, ref textbox_HoursPlayed);
                //hoursLast2WeeksHandler = new HoursHandler(ref chkbxHoursPlayedLast2Weeks, ref lblPreHoursPlayedLast2Weeks,
                //    ref lblPostHoursPlayedLast2Weeks, ref combobox_HoursPlayedLast2Weeks, ref textbox_HoursPlayedLast2Weeks);

                this.Width = MIN_WINDOW_SIZE;
                this.MaxWidth = MIN_WINDOW_SIZE;
                this.MinWidth = MIN_WINDOW_SIZE;
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
        private void PopulateGames()
        {
            Settings settings = Settings.GetInstance(this);
            if (settings == null)
                return;
            
            //Populate, attach event
            string steamid64 = settings.SteamID64;
            settings.ReturnInstance(ref settings);

            gamePool = GameUtilities.PopulateGames(steamid64);
            gamePool.Changed += new ChangedEventHandler(gamePool_Changed);
            gamePoolHandler.Bind(gamePool);

            removedPool.Changed += new ChangedEventHandler(removedPool_Changed);
            removedPoolHandler.Bind(removedPool);

        }

        public static void RemoveDlc(int id)
        {
            lock (gamePool)
            {
                gamePool.RemoveNoNotify(gamePool.GetGame(id));
            }
        }

        private void gamePool_Changed(object sender, EventArgs e)
        {
            gamePoolHandler.Refresh();
        }

        private void removedPool_Changed(object sender, EventArgs e)
        {
            removedPoolHandler.Refresh();
        }

        private void btnRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            Game itemToRemove = gamePoolHandler.GetSelectedItem();

            if (itemToRemove == null)
                return;

            removedPool.Add(itemToRemove);
            gamePool.Remove(itemToRemove);
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            Game itemToRemove = removedPoolHandler.GetSelectedItem();

            if (itemToRemove == null)
                return;

            gamePool.Add(itemToRemove);
            removedPool.Remove(itemToRemove);
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

        //maybe move this logic into util?
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (gamePool.Count == 0)
            {
                MessageBox.Show("No games in game pool!");
                return;
            }

            Random rand = new Random((DateTime.Now.Millisecond * DateTime.Now.Minute));
            int choiceGame = rand.Next(gamePool.Count);

            gamePool[choiceGame].Launch();
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
                if(cbxIncludeExcludeTags.SelectedIndex == 0)
                    tagsChecked.Add(tag);
                else
                {
                    checkbox.IsChecked = null;
                    //add to exclude tags here
                }
            }
            else
            {
                tagsChecked.Remove(tag);
                //remove from exclude
            }

            BlanketUpdate(GetTagApplicationMethod());
        }

        private void cbChecked_OnlyInstalled(object sender, RoutedEventArgs e)
        {
            BlanketUpdate(GetTagApplicationMethod());
        } // end cbChecked_OnlyInstalled()

        private void BlanketUpdate(TagApplicationMethod method)
        {
            bool thereAreTagsChecked = (tagsChecked.Count >= 1) ? true : false;
            bool onlyInstalledIsChecked = (bool)chkbxOnlyInstalled.IsChecked;
            bool hoursPlayedIsExpanded = (bool)hoursPlayedExpander.IsExpanded;
            bool last2WeeksIsExpanded = (bool)last2WeeksExpander.IsExpanded;

            // default values
            bool hoursPlayedGreaterThan = false; 
            bool last2WeeksGreaterThan = false; 
            double hoursPlayedHours = 0; 
            double last2WeeksHours = 0;

            if (hoursPlayedIsExpanded)
            {
                if (!VerificationClass.VerifyHours(textbox_HoursPlayed, ref hoursPlayedHours))
                    return;
                hoursPlayedGreaterThan = (combobox_HoursPlayed.SelectedIndex == 0) ? true : false;
            }

            if (last2WeeksIsExpanded)
            {
                if (!VerificationClass.VerifyHours(textbox_HoursPlayedLast2Weeks, ref last2WeeksHours))
                    return;
                last2WeeksGreaterThan = (combobox_HoursPlayedLast2Weeks.SelectedIndex == 0) ? true : false;
            }

            // Get pools ready for update
            gamePool.AddList(removedPool);
            removedPool.Clear();
            List<Game> gamesToRemove = new List<Game>();

            // iterate through each game to finalize gamepool and removedpool
            foreach (Game game in gamePool)
            {

                #region Tags
                if (thereAreTagsChecked)
                {
                    if (!game.ContainsTag(tagsChecked, method))
                    {
                        gamesToRemove.Add(game);
                        continue; 
                    }
                }
                #endregion

                #region Only Installed
                if (onlyInstalledIsChecked)
                {
                    if (!game.IsInstalled())
                    {
                        gamesToRemove.Add(game);
                        continue;
                    }
                } // end onlyinstalled checkbox if
                #endregion

                #region Hours Played
                
                if (hoursPlayedIsExpanded)
                {
                    //TODO: Considuer using method return for this?
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
                
                if (last2WeeksIsExpanded)
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
        }

        private void CheckBoxChanged(ref GamesList pool, string checkboxChanged)
        {
            List<Game> tempForRemoval = new List<Game>();

            Settings grabTagAppMethod = Settings.GetInstance(this);
            TagApplicationMethod tam = grabTagAppMethod.TagApplication;
            grabTagAppMethod.ReturnInstance(ref grabTagAppMethod);

            List<GameUtilities.Tags> checkedTags = GetCheckboxInTags();


            //TODO: Must also go through removed list and re-add games to list if necesarry.

            foreach (Game g in pool)//-> this method will explicity visit game pool and removed pool instead.
                if (!g.ContainsTag(checkedTags, tam))
                    tempForRemoval.Add(g);

            //this is so we dont edit the list while looking through it
            foreach (Game g in tempForRemoval)
            {
                removedPool.Add(g);
                gamePool.Remove(g);
            }
        }

        // TODO: Do we need this anymore?
        private List<GameUtilities.Tags> GetCheckboxInTags()
        {
            List<GameUtilities.Tags> tagsOn = new List<GameUtilities.Tags>();

            //TODO: This will handle a list of checkboxes and just go through a foreach
            if ((bool)chkbxFPS.IsChecked)
            {
                tagsOn.Add(GameUtilities.CreateTag((string)chkbxFPS.Content));
            }
            if ((bool)chkbxMMO.IsChecked)
            {
                tagsOn.Add(GameUtilities.CreateTag((string)chkbxMMO.Content));
            }
            if ((bool)chkbxRPG.IsChecked)
            {
                tagsOn.Add(GameUtilities.CreateTag((string)chkbxRPG.Content));
            }

            return tagsOn;
        }

        private void formLoaded(object sender, RoutedEventArgs e)
        {
            checkboxesActive = true;
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.Key == Key.Left)
                btnAddGame_Click(sender, e);
            if (e.Key == Key.Right)
                btnRemoveGame_Click(sender, e);
            */
        }

        private void btnOpenSettings(object sender, RoutedEventArgs e)
        {
            TagApplicationMethod preMethod = GetTagApplicationMethod();
            SettingsScreen ss = new SettingsScreen();
            ss.ShowDialog();
            TagApplicationMethod postMethod = GetTagApplicationMethod();

            Settings testForRefresh = Settings.GetInstance(this);
            bool refresh = testForRefresh.ShouldRefresh();
            List<string> colsToShow = testForRefresh.GetColumnsToShow();
            testForRefresh.ReturnInstance(ref testForRefresh);

            gamePoolHandler.ClearColumns();
            removedPoolHandler.ClearColumns();

            foreach (string s in colsToShow)
            {
                gamePoolHandler.AddColumn(s);
                removedPoolHandler.AddColumn(s);
            }

            if (refresh)
            {
                gamePool.Clear();
                removedPool.Clear();
                ResetCheckboxes();
                PopulateGames();
                tagsChecked.Clear();
            }

            else if (!refresh && preMethod != postMethod) // if they change contains method, update list occordingly. 
            {
                BlanketUpdate(postMethod);
            }
        }

        // helper method for btnOpenSettings -> if (refresh)
        private void ResetCheckboxes()
        {
            chkbxFPS.IsChecked = false;
            chkbxMMO.IsChecked = false;
            chkbxMulti.IsChecked = false;
            chkbxOnlyInstalled.IsChecked = false;
            chkbxRPG.IsChecked = false;
            chkbxSingle.IsChecked = false;
            chkbxSurvival.IsChecked = false;
        }

        private TagApplicationMethod GetTagApplicationMethod()
        {
            Settings grabTagAppMethod = Settings.GetInstance(this);
            TagApplicationMethod method = grabTagAppMethod.TagApplication;
            grabTagAppMethod.ReturnInstance(ref grabTagAppMethod);

            return method;
        }

        private void btnMinimizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ExpanderChanged_Hours(object sender, RoutedEventArgs e)
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

                //TODO:  May implement last entered text option here, and move validation here instead of in blanketupdate
                if (tb.Name.Equals("textbox_HoursPlayedLast2Weeks"))
                {
                    //hoursLast2WeeksHandler.Verify();                    
                }
                else if (tb.Name.Equals("textbox_HoursPlayed"))
                {
                    //hoursPlayedHandler.Verify();
                }

                BlanketUpdate(GetTagApplicationMethod());

                if (tb.Focusable)
                    tb.Focus();
            }
        }

        #endregion

        private void btnClickRemoveAll(object sender, RoutedEventArgs e)
        {
            foreach (Game game in gamePool)
                removedPool.Add(game);

            gamePool.Clear();
        }

        private void btnClickAddAll(object sender, RoutedEventArgs e)
        {
            foreach (Game game in removedPool)
                gamePool.Add(game);

            removedPool.Clear();
        }

        private void btnSaveGamePool(object sender, RoutedEventArgs e)
        {
            FilenameTypein typein = new FilenameTypein();
            bool saveClicked = (bool)typein.ShowDialog();
            string fileName = typein.FileName;

            if (saveClicked && fileName != null)
            {
                GamesList tempListToSave = new GamesList();
                tempListToSave.AddList(gamePool.ToList<Game>());

                if(!Directory.Exists(@".\saves"))
                    Directory.CreateDirectory(@".\saves");

                GameUtilities.SaveGameList(tempListToSave, @".\saves\" + fileName, "gp");
            }
            
        }

        private void btnLoadGamePool(object sender, RoutedEventArgs e)
        {
            //open file browser to search for .gp files
            string filename;
            OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".gp";
            dlg.Filter = "GAMEPOOL Files (*.gp)|*.gp";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
                filename = dlg.FileName;
            else
                return;

            GamesList tempGamePool = GameUtilities.LoadGameList(filename, "gp", true);

            btnClickRemoveAll(null, new RoutedEventArgs());

            foreach(Game game in tempGamePool)
            {
                gamePool.Add(game);
                removedPool.Remove(game);
            }
        }

        private void btnOpenHiddenGames_Click(object sender, RoutedEventArgs e)
        {
            //Current not great implementation
            if (this.Width < MAX_WINDOW_SIZE)
            {
                btnOpenHiddenGamesArrow.Content = "<";

                this.MaxWidth = MAX_WINDOW_SIZE;
                while (this.Width < MAX_WINDOW_SIZE)
                {
                    this.Width += 5;
                    System.Windows.Forms.Application.DoEvents();
                }
                this.MinWidth = MAX_WINDOW_SIZE;
            }
            else
            {
                btnOpenHiddenGamesArrow.Content = ">";
                
                this.MinWidth = MIN_WINDOW_SIZE;
                while (this.Width > MIN_WINDOW_SIZE)
                {
                    this.Width -= 5;
                    System.Windows.Forms.Application.DoEvents();
                }
                this.MaxWidth = MIN_WINDOW_SIZE;
            }
        }

        private void gamepool_datagrid_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == GetKeybind())
                btnRemoveGame_Click(sender, e);
        }

        private Key GetKeybind()
        {
            Key keybind;
            Settings temp = Settings.GetInstance(this);

            if (temp == null)
                return Key.None;

            keybind = temp.GamePoolRemoveKeyBinding;
            temp.ReturnInstance(ref temp);

            return keybind;
        }

    }
}