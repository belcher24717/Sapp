using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DesignPractice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Game> gamePool;
        private List<Game> removedPool;
        private bool checkboxesActive;

        public MainWindow()
        {
            InitializeComponent();

            checkboxesActive = false;
            gamePool = new List<Game>();
            removedPool = new List<Game>();

            SetRectangleSize();
            PopulateGames();
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

        private void PopulateGames()
        {
            //the username will have to be entered by the user manually the first time.
            XmlTextReader reader = new XmlTextReader("http://steamcommunity.com/id/belcher24717/games?tab=all&xml=1");

            //the first text node will be a large int, do not want
            bool firstTextEaten = false;

            while (reader.Read())
            {
                //eat up that first text node
                if (!firstTextEaten && XmlNodeType.Text == reader.NodeType)
                {
                    firstTextEaten = true;
                }

                if (XmlNodeType.Text == reader.NodeType && TestIfAppID(reader.Value))
                {
                    int appid = int.Parse(reader.Value);
                    reader.Read();
                    reader.Read();
                    reader.Read();
                    reader.Read();
                    string gameName = reader.Value;

                    gamePool.Add(new Game(gameName, appid));
                }
            }

            //get everything into the list
            gamePool.Sort();
            foreach(Game g in gamePool)
                lstbxGamePool.Items.Add(g);
        }

        //there are other text nodes that are all doubles, dont know what they are for
        //but we want to skip them
        private bool TestIfAppID(string value)
        {
            try
            {
                int test = int.Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void btnRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            if(lstbxGamePool.SelectedIndex == -1)
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
                lstbxNotInGamePool.Items.Add(gameToSwap);

                //remove it from the playable list
                gamePool.RemoveAt(index);
                lstbxGamePool.Items.RemoveAt(index);
            }

            else if (lstbxNotInGamePool.Items.Contains(gameToSwap))
            {
                int index = lstbxNotInGamePool.Items.IndexOf(gameToSwap);

                //add it to removed list
                gamePool.Add(gameToSwap);
                lstbxGamePool.Items.Add(gameToSwap);

                //remove it from the playable list
                removedPool.RemoveAt(index);
                lstbxNotInGamePool.Items.RemoveAt(index);
            }
        }

        private void btnSortLists_Click(object sender, RoutedEventArgs e)
        {
            gamePool.Sort();
            removedPool.Sort();

            while (lstbxNotInGamePool.Items.Count != 0)
                lstbxNotInGamePool.Items.RemoveAt(0);

            while (lstbxGamePool.Items.Count != 0)
                lstbxGamePool.Items.RemoveAt(0);

            foreach (Game s in gamePool)
                lstbxGamePool.Items.Add(s);

            foreach (Game s in removedPool)
                lstbxNotInGamePool.Items.Add(s);
                
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random((DateTime.Now.Millisecond * DateTime.Now.Minute));
            int choiceGame = rand.Next(gamePool.Count);

            //launch game from choice game
            //MessageBox.Show(gamePool[choiceGame].ToString());

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

        private void CheckBoxChanged(ref List<Game> pool, string checkboxChanged)
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
            checkboxesActive = true;
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Left)
                btnAddGame_Click(sender, e);
            if (e.Key == Key.Right)
                btnRemoveGame_Click(sender, e);

        }

        
    }
}
