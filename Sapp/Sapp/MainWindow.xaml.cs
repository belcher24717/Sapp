using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
        private GamesList gamePool;
        private GamesList removedPool;
        private bool checkboxesActive;
        private bool sortSwitch;

        public MainWindow()
        {
            InitializeComponent();

            Settings.Initialize();

            checkboxesActive = false;
            sortSwitch = false;
            gamePool = new GamesList();
            removedPool = new GamesList();

            SetRectangleSize();
            
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
            gamePool = GameUtilities.PopulateGames(settings.SteamID64);
            gamePool.Changed += new ChangedEventHandler(gamePool_Changed);

            settings.ReturnInstance(ref settings);

            lstbxGamePool.ItemsSource = gamePool;


            removedPool.Changed += new ChangedEventHandler(removedPool_Changed);
            lstbxNotInGamePool.ItemsSource = removedPool;
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

            // ------------------------------------------ THIS WILL BE REMOVED ---------------------------------------------------

        }

        //maybe move this logic into util?
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random((DateTime.Now.Millisecond * DateTime.Now.Minute));
            int choiceGame = rand.Next(gamePool.Count);

            gamePool[choiceGame].Launch();

        }

        private void checkboxChecked(object sender, RoutedEventArgs e)
        {
            if (!checkboxesActive)
                return;

            CheckBoxChanged(ref removedPool, ((CheckBox)sender).Content.ToString());
        }

        private void checkboxUnchecked(object sender, RoutedEventArgs e)
        {
            if (!checkboxesActive)
                return;

            CheckBoxChanged(ref gamePool, ((CheckBox)sender).Content.ToString());
        }

        private void CheckBoxChanged(ref GamesList pool, string checkboxChanged)
        {
            List<Game> tempForRemoval = new List<Game>();

            foreach (Game g in pool)
                if (g.IsGenres(checkboxChanged))
                    tempForRemoval.Add(g);

            //this is so we dont edit the list while looking through it
            foreach (Game g in tempForRemoval)
                PutGameIntoOtherPool(g);
        }

        private void formLoaded(object sender, RoutedEventArgs e)
        {
            PopulateGames();

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
                PopulateGames();

            
        }

        /* button no longer exists currently
        private void btnRefreshClick(object sender, RoutedEventArgs e)
        {
            PopulateGames();
        }
        */

    }
}
