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
        private bool sortSwitch;

        private DataGridHandler gamePoolDataGrid;
        private DataGridHandler removedPoolDataGrid;

        private static MainWindow thisInstance;

        public MainWindow()
        {
            InitializeComponent();

            Logger.Log("Main Application Started", true);

            thisInstance = this;

            bool settingsLoaded = false;
            Nullable<bool> windowAccepted = true;

            while (!settingsLoaded)
            {

                try
                {
                    Settings.Initialize();

                    checkboxesActive = false;
                    sortSwitch = false;
                    gamePool = new GamesList();
                    removedPool = new GamesList();
                    tagsChecked = new List<GameUtilities.Tags>();

                    SetRectangleSize();

                    settingsLoaded = true;
                }
                catch (FileNotFoundException fileNotFound)
                {
                    SettingsScreen ss = new SettingsScreen();
                    windowAccepted = ss.ShowDialog();
                }
                catch (SerializationException serializationFailed)
                {
                    SettingsScreen ss = new SettingsScreen();
                    windowAccepted = ss.ShowDialog();
                }

                if (windowAccepted != null && !(bool)windowAccepted)
                {
                    settingsLoaded = true;
                    this.Close();
                }
            }

            gamePoolDataGrid = new DataGridHandler(ref dgGamePool);
            removedPoolDataGrid = new DataGridHandler(ref dgRemovedPool);

            Logger.Log("START: Populating Games", true);

            PopulateGames();

            Logger.Log("END: Populating Games", true);

            //TODO: Make all the checkboxes, and then all references to a list here.
            //This list will make management of all checkboxes easy (seperate by type, like: tag filters, intalled only, hours played, etc)
            
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
            gamePoolDataGrid.Bind(gamePool);
            gamePoolDataGrid.AddColumn("Title");

            removedPool.Changed += new ChangedEventHandler(removedPool_Changed);
            removedPoolDataGrid.Bind(removedPool);
            removedPoolDataGrid.AddColumn("Title");

            //TODO: REMOVE THIS, IT WILL BE DYNAMICALLY SET IN THE SETTINGS SCREEN
            gamePoolDataGrid.AddColumn("HoursPlayed");
            removedPoolDataGrid.AddColumn("HoursPlayed");

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
            gamePoolDataGrid.Refresh();
        }

        private void removedPool_Changed(object sender, EventArgs e)
        {
            removedPoolDataGrid.Refresh();
        }

        private void btnRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            Game itemToRemove = gamePoolDataGrid.GetSelectedItem();

            if (itemToRemove == null)
                return;

            removedPool.Add(itemToRemove);
            gamePool.Remove(itemToRemove);
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            Game itemToRemove = removedPoolDataGrid.GetSelectedItem();

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

        private void btnSortLists_Click(object sender, RoutedEventArgs e)
        {
            // ------------------------------------------ THIS WILL BE REMOVED ---------------------------------------------------
            gamePool.Sort();
            //Optimization: potentially add logic to only sort IF removedPool is visible
            removedPool.Sort();

            if (!sortSwitch)
            {
                gamePool.Reverse();
                //Optimization: potentially add logic to only sort IF removedPool is visible
                removedPool.Reverse();
                sortSwitch = true;
            }
            else
                sortSwitch = false;

            //lstbxGamePool.Items.Refresh();

            // ------------------------------------------ THIS WILL BE REMOVED ---------------------------------------------------

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
                tagsChecked.Add(tag);
            else
                tagsChecked.Remove(tag);

            BlanketUpdate(GetTagApplicationMethod());
        }

        private void cbChecked_OnlyInstalled(object sender, RoutedEventArgs e)
        {
            BlanketUpdate(GetTagApplicationMethod());
        } // end cbChecked_OnlyInstalled()

        #region OLD CODE
        
        private void checkboxChecked(object sender, RoutedEventArgs e)
        {
            if (!checkboxesActive)
                return;

            GameUtilities.Tags tag = GameUtilities.CreateTag(((CheckBox)sender).Content.ToString());

            TagApplicationMethod method = GetTagApplicationMethod();

            //CheckBoxChanged(ref gamePool, ((CheckBox)sender).Content.ToString()); 
            tagsChecked.Add(tag);
            //RemoveTaggedGames(method);
            BlanketUpdate(method);
        }
         
        private void checkboxUnchecked(object sender, RoutedEventArgs e)
        {
            if (!checkboxesActive)
                return;

            GameUtilities.Tags tag = GameUtilities.CreateTag(((CheckBox)sender).Content.ToString());

            TagApplicationMethod method = GetTagApplicationMethod();

            //CheckBoxChanged(ref removedPool, ((CheckBox)sender).Content.ToString());
            tagsChecked.Remove(tag);
            //AddTaggedGames(method);
            BlanketUpdate(method);
        }

        private void RemoveTaggedGames(TagApplicationMethod method) // tag checked, remove appropriate games.
        {
            List<Game> gameToRemove = new List<Game>();

            foreach (Game game in gamePool)
            {
                if (!game.ContainsTag(tagsChecked, method))
                    gameToRemove.Add(game);
                else if ((bool)chkbxOnlyInstalled.IsChecked)
                    if (!game.IsInstalled())
                        gameToRemove.Add(game);

            }

            // could do this with games instead of indexes
            foreach (Game game in gameToRemove)
            {
                removedPool.Add(game);
                gamePool.Remove(game);
            }

            // TEMPORARY
            removedPool.Sort();

        }

        private void AddTaggedGames(TagApplicationMethod method) // tag unchecked, re-add appropriate games.
        {
            List<Game> tempToBeAdded = new List<Game>();
            bool addGame = true;

            foreach (Game game in removedPool)
            {
                //TODO: Need to add logic for filters other than tags, such as isInstalled.

                // potentially have filter list that we check here with && like below...
                if (!game.ContainsTag(tagsChecked, method))
                    addGame = false;
                else if ((bool)chkbxOnlyInstalled.IsChecked)
                    if (!game.IsInstalled())
                        addGame = false;
               
                if (addGame)
                    tempToBeAdded.Add(game);
                else
                    addGame = true;
            }

            foreach (Game game in tempToBeAdded)
            {
                gamePool.Add(game);
                removedPool.Remove(game);
            }

            // TEMPORARY
            gamePool.Sort();
        }

        #endregion

        private void BlanketUpdate(TagApplicationMethod method)
        {
            // ensures a game cannot be added multiple times if it fails multiple checks (tags, is installed, etc...)
            bool gameRemoved = false;

            gamePool.AddList(removedPool);

            removedPool.Clear();

            List<Game> gamesToRemove = new List<Game>();

            foreach (Game game in gamePool)
            {
                if (tagsChecked.Count >= 1)
                {
                    if (!game.ContainsTag(tagsChecked, method))
                    {
                        gamesToRemove.Add(game);
                        gameRemoved = true;
                        continue; //This makes it go to the next iteration of the loop
                    }
                }

                if ((bool)chkbxOnlyInstalled.IsChecked)
                {
                    if (!game.IsInstalled())
                    {
                        gamesToRemove.Add(game);
                        gameRemoved = true; // in case we add more logic that requires this after
                        continue;
                    }
                }

                //if (!games hours < OR > chosen hours), etc...

                gameRemoved = false;
            }

            //only 1 refresh per datagrid this way
            removedPool.AddList(gamesToRemove);
            gamePool.RemoveList(gamesToRemove);


            // TEMPORARY - No longer needed because the datagrid and gamepool will never be in the same order
            //gamePool.Sort();
            //removedPool.Sort();
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

            if (e.Key == Key.Left)
                btnAddGame_Click(sender, e);
            if (e.Key == Key.Right)
                btnRemoveGame_Click(sender, e);

        }

        private void btnOpenSettings(object sender, RoutedEventArgs e)
        {
            TagApplicationMethod preMethod = GetTagApplicationMethod();
            SettingsScreen ss = new SettingsScreen();
            ss.ShowDialog();
            TagApplicationMethod postMethod = GetTagApplicationMethod();

            Settings testForRefresh = Settings.GetInstance(this);
            bool refresh = testForRefresh.ShouldRefresh();
            testForRefresh.ReturnInstance(ref testForRefresh);

            if (refresh)
            {
                gamePool.Clear();
                removedPool.Clear();
                ResetCheckboxes();
                PopulateGames();
                tagsChecked.Clear();
            }

            else if (preMethod != postMethod) // if they change contains method, update list occordingly. 
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

    }
}
