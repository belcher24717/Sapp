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

        private static MainWindow thisInstance;

        public MainWindow()
        {
            InitializeComponent();

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

            PopulateGames();

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

            lstbxGamePool.ItemsSource = gamePool;


            removedPool.Changed += new ChangedEventHandler(removedPool_Changed);
            lstbxNotInGamePool.ItemsSource = removedPool;

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
            lstbxGamePool.Items.Refresh();
        }

        private void removedPool_Changed(object sender, EventArgs e)
        {
            lstbxNotInGamePool.Items.Refresh();
        }

        private void btnRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            if (lstbxGamePool.SelectedIndex == -1)
                return;

            Game itemToRemove = gamePool[lstbxGamePool.SelectedIndex];
            PutGameIntoOtherPool(itemToRemove);
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            if (lstbxNotInGamePool.SelectedIndex == -1)
                return;

            Game itemToAdd = removedPool[lstbxNotInGamePool.SelectedIndex];
            PutGameIntoOtherPool(itemToAdd);
        }

        private void PutGameIntoOtherPool(Game gameToSwap)
        {
            if (lstbxGamePool.Items.Contains(gameToSwap))
            {
                int index = lstbxGamePool.Items.IndexOf(gameToSwap);

                //add it to removed list
                removedPool.Add(gameToSwap);

                FixSelection(lstbxGamePool);

                //remove it from the playable list
                gamePool.RemoveAt(index);
            }

            else if (lstbxNotInGamePool.Items.Contains(gameToSwap))
            {
                int index = lstbxNotInGamePool.Items.IndexOf(gameToSwap);

                //add it to removed list
                gamePool.Add(gameToSwap);

                FixSelection(lstbxNotInGamePool);

                //remove it from the playable list
                removedPool.RemoveAt(index);
            }
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

        private void checkboxChecked(object sender, RoutedEventArgs e)
        {
            if (!checkboxesActive)
                return;

            GameUtilities.Tags tag = GameUtilities.CreateTag(((CheckBox)sender).Content.ToString());

            TagApplicationMethod method = GetTagApplicationMethod();

            //CheckBoxChanged(ref gamePool, ((CheckBox)sender).Content.ToString()); 
            tagsChecked.Add(tag);
            RemoveTaggedGames(method);
        }

        private void checkboxUnchecked(object sender, RoutedEventArgs e)
        {
            if (!checkboxesActive)
                return;

            GameUtilities.Tags tag = GameUtilities.CreateTag(((CheckBox)sender).Content.ToString());

            TagApplicationMethod method = GetTagApplicationMethod();

            //CheckBoxChanged(ref removedPool, ((CheckBox)sender).Content.ToString());
            tagsChecked.Remove(tag);
            AddTaggedGames(method);
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
                if (!game.ContainsTag(tagsChecked, method) /* && !game.ContainsFilter(filter list) */ )
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
                PutGameIntoOtherPool(g);
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
            SettingsScreen ss = new SettingsScreen();
            ss.ShowDialog();

            Settings testForRefresh = Settings.GetInstance(this);
            bool refresh = testForRefresh.ShouldRefresh();
            testForRefresh.ReturnInstance(ref testForRefresh);

            if (refresh)
            {
                gamePool.Clear();
                removedPool.Clear();
                PopulateGames();
                tagsChecked.Clear();

                ResetCheckboxes();
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

        private void cbChecked_OnlyInstalled(object sender, RoutedEventArgs e)
        {
            TagApplicationMethod method = GetTagApplicationMethod();

            if ((bool)chkbxOnlyInstalled.IsChecked)
                RemoveTaggedGames(method);
            else
                AddTaggedGames(method);

            /*
            //once more filters are added we will need a method that will sort based off all of them
            if ((bool)chkbxOnlyInstalled.IsChecked)
            {
                for (int i = 0; i < gamePool.Count; i++)
                {
                    if (!gamePool[i].IsInstalled())
                    {
                        PutGameIntoOtherPool(gamePool[i]);
                        i--;
                    }

                }

            }
            else
            {
                while (removedPool.Count > 0)
                {
                    PutGameIntoOtherPool(removedPool[0]);
                }
            }
             */
        } // end cbChecked_OnlyInstalled()

        private TagApplicationMethod GetTagApplicationMethod()
        {
            Settings grabTagAppMethod = Settings.GetInstance(this);
            TagApplicationMethod method = grabTagAppMethod.TagApplication;
            grabTagAppMethod.ReturnInstance(ref grabTagAppMethod);

            return method;
        }

    }
}
