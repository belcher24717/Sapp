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
            Settings settings = Settings.GetInstance(this);

            if (settings == null)
                return;

            //the username will have to be entered by the user manually the first time.
            XmlTextReader reader = new XmlTextReader("http://steamcommunity.com/profiles/" + settings.GetUserID() + "/games?tab=all&xml=1");
            //76561198027181438 JOHNNY
            //76561198054602483 NICKS

            //the first text node will be a large int, do not want
            bool firstTextEaten = false;
            bool gameAdded = false;

            while (reader.Read())
            {
                //eat up that first text node
                if (!firstTextEaten && XmlNodeType.Text == reader.NodeType)
                {
                    firstTextEaten = true;
                }

                else if (XmlNodeType.Text == reader.NodeType && TestIfAppID(reader.Value, gameAdded))
                {
                    
                    int appid = int.Parse(reader.Value);
                    reader.Read();
                    reader.Read();
                    reader.Read();
                    reader.Read();
                    string gameName = reader.Value;


                    if (CheckIfDLC(appid) && (IsInstalled(appid) || !settings.OnlyAllowInstalled()))
                    {
                        gamePool.Add(new Game(gameName, appid));
                        gameAdded = true;
                    }
                    else
                        gameAdded = false;
                }
            }

            //I'm finished, return the instance
            settings.ReturnInstance(ref settings);


            //get everything into the list
            gamePool.Sort();
            foreach(Game g in gamePool)
                lstbxGamePool.Items.Add(g);
        }

        //very inefficient. Find another way to do this
        private bool CheckIfDLC(int appid)
        {
            return true;

            WebClient wc = new WebClient();
            string data = wc.DownloadString("http://steamcommunity.com/app/" + appid);

            int i = data.IndexOf("http://steamcommunity.com/app/") + 30;

            int j = i;
            while (!data[j].Equals('\"'))
                j++;
            j = j - i;

            string temp = data.Substring(i, j);

            return appid.ToString().Equals(temp);
        }

        private bool IsInstalled(int id)
        {
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App " + id);

            if(key == null)
                return false;
            return true;
        }

        //there are other text nodes that are all doubles, dont know what they are for
        //but we want to skip them
        private bool TestIfAppID(string value, bool gameAdded)
        {
            try
            {
                int test = int.Parse(value);
                return true;
            }
            catch
            {
                if (gameAdded)
                {
                    gamePool[gamePool.Count - 1].AddGameTime(value);
                }
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

        private void btnOpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsScreen ss = new SettingsScreen();
            ss.ShowDialog();
        }

        
    }
}
