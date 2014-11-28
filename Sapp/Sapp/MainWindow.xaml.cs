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
        private List<Game> gamePool;
        private List<Game> removedPool;
        private bool checkboxesActive;
        private bool sortSwitch;

        public MainWindow()
        {
            InitializeComponent();

            Settings.Initialize();

            checkboxesActive = false;
            sortSwitch = false;
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

        //make a game manager to do a lot of this logic
        private void PopulateGames()
        {
            Settings settings = Settings.GetInstance(this);

            if (settings == null)
                return;

            lstbxNotInGamePool.Items.Clear();
            lstbxGamePool.Items.Clear();
            gamePool.Clear();
            removedPool.Clear();

            //the username will have to be entered by the user manually the first time.
            XmlTextReader reader = new XmlTextReader("http://steamcommunity.com/profiles/" + settings.UserID + "/games?tab=all&xml=1");
            //76561198027181438 JOHNNY
            //76561198054602483 NICKS

            bool gameAdded = false;
            bool hadStats = false;
            //bool firstPass = true; // this is needed so that the first pass will work for hadStats

            while (reader.Read())
            {

                if(XmlNodeType.Element == reader.NodeType)
                {
                    if (reader.Name.Equals("appID"))
                    {

                        reader.Read();

                        //might throw try/catch here
                        int appid = int.Parse(reader.Value);

                        reader.Read();
                        reader.Read();
                        reader.Read();
                        reader.Read();
                        string gameName = reader.Value;

                        //might need to use game added if DLC can EVER have hours tied to it.
                        if (CheckIfDLC(appid))
                        {
                            gamePool.Add(new Game(gameName, appid, IsInstalled(appid)));
                        }
                    }

                    else if (reader.Name.Contains("hours"))
                    {
                        reader.Read();
                        gamePool[gamePool.Count - 1].AddGameTime(reader.Value);
                    }
                }

            }

            //I'm finished, return the instance
            settings.ReturnInstance(ref settings);


            //get everything into the list
            //gamePool.Sort();
            foreach (Game g in gamePool)
                lstbxGamePool.Items.Add(g);
        }

        //very inefficient. Find another way to do this
        private bool CheckIfDLC(int appid)
        {
            return true;
            /*
            WebClient wc = new WebClient();
            string data = wc.DownloadString("http://steamcommunity.com/app/" + appid);

            int i = data.IndexOf("http://steamcommunity.com/app/") + 30;

            int j = i;
            while (!data[j].Equals('\"'))
                j++;
            j = j - i;

            string temp = data.Substring(i, j);

            return appid.ToString().Equals(temp);
             * */
            //if (appid == 98421)
                //appid = appid;

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://steamcommunity.com/app/" + appid);

                request.Method = "HEAD";
                request.AllowAutoRedirect = true;
                //request.Timeout = 5000;

                HttpWebResponse response = request.GetResponse() as HttpWebResponse; //request.
                
                return response.ResponseUri.AbsolutePath.Equals("http://steamcommunity.com/app/" + appid);
            }
            catch
            {
                return false;
            }
        }

        private bool IsInstalled(int id)
        {
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App " + id);

            if (key == null)
                return false;
            return true;
        }

        //there are other text nodes that are all doubles, dont know what they are for
        //but we want to skip them
        private bool TestIfAppID(XmlReader reader, bool gameAdded)
        {
            try
            {

                int test = int.Parse(reader.Value);

                return true;
            }
            catch
            {
                if (gameAdded)
                {
                    gamePool[gamePool.Count - 1].AddGameTime(reader.Value);
                }
                return false;
            }
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
                lstbxNotInGamePool.Items.Add(gameToSwap);

                FixSelection(lstbxGamePool);

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

                FixSelection(lstbxNotInGamePool);

                //remove it from the playable list
                removedPool.RemoveAt(index);
                lstbxNotInGamePool.Items.RemoveAt(index);
            }
        }

        private void FixSelection(ListBox container)
        {
            if (container.Items.Count > 1)
                if (container.SelectedIndex == container.Items.Count - 1)
                    container.SelectedIndex = container.Items.Count - 2;
                else
                    container.SelectedIndex += 1;
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

        private void btnRefreshClick(object sender, RoutedEventArgs e)
        {
            PopulateGames();
        }


    }
}
