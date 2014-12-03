using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Sapp
{
    public abstract class GameUtilities
    {

        public enum Tags
        {
            NullTag,
            Action,
            Indie,
            Adventure,
            Strategy,
            RPG,
            Simulation,
            Casual,
            FreeToPlay,
            Singleplayer,
            Multiplayer,
            MMO,
            Racing,
            Sports,
            Shooter,
            FPS,
            SciFi
        };

        public static GameUtilities.Tags CreateTag(string tag) // beautiful if block!
        {
            if (tag.Equals("Action"))
                return GameUtilities.Tags.Action;
            else if (tag.Equals("Indie"))
                return GameUtilities.Tags.Indie;
            else if (tag.Equals("Adventure"))
                return GameUtilities.Tags.Adventure;
            else if (tag.Equals("Strategy"))
                return GameUtilities.Tags.Strategy;
            else if (tag.Equals("RPG"))
                return GameUtilities.Tags.RPG;
            else if (tag.Equals("Simulation"))
                return GameUtilities.Tags.Simulation;
            else if (tag.Equals("Casual"))
                return GameUtilities.Tags.Casual;
            else if (tag.Equals("Free to Play"))
                return GameUtilities.Tags.FreeToPlay;
            else if (tag.Equals("Singleplayer"))
                return GameUtilities.Tags.Singleplayer;
            else if (tag.Equals("Massively Multiplayer"))
                return GameUtilities.Tags.MMO;
            else if (tag.Equals("Multiplayer"))
                return GameUtilities.Tags.Multiplayer;
            else if (tag.Equals("Racing"))
                return GameUtilities.Tags.Racing;
            else if (tag.Equals("Sports"))
                return GameUtilities.Tags.Sports;
            else if (tag.Equals("Shooter"))
                return GameUtilities.Tags.Shooter;
            else if (tag.Equals("FPS"))
                return GameUtilities.Tags.FPS;
            else if (tag.Equals("Sci-Fi"))
                return GameUtilities.Tags.SciFi;
            else // tag is not recognized, won't be added
                return GameUtilities.Tags.NullTag;

        } // end CreateTag

        public static bool IsInstalled(int id)
        {

            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App " + id);

            if (key == null)
                return false;
            return true;
        
        }

        public static GamesList PopulateGames(string userID)
        {
            GamesList games = new GamesList();

            #region Read In Game Data

            //the username will have to be entered by the user manually the first time.
            XmlTextReader reader = new XmlTextReader("http://steamcommunity.com/profiles/" + userID + "/games?tab=all&xml=1");
            //76561198027181438 JOHNNY
            //76561198054602483 NICKS



            while (reader.Read())
            {

                if (XmlNodeType.Element == reader.NodeType)
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

                        games.Add(new Game(gameName, appid, GameUtilities.IsInstalled(appid)));
                        
                    }

                    else if (reader.Name.Contains("hours"))
                    {
                        reader.Read();
                        games[games.Count - 1].AddGameTime(reader.Value);
                    }
                }
            }

            #endregion

            #region Weed Out DLC

            Task[] tasks = new Task[games.Count];
            HelperThread.theList = games;

            //gets rid of dlc, using multiple threads (tasks)
            int number = 0;
            foreach (Game g in games)
            {

                if (g.GetHoursPlayed() == 0)
                {
                    tasks[number] = Task.Factory.StartNew(() =>
                    {
                        var sacThread = new HelperThread(g.GetAppID());
                        //ThreadPool.QueueUserWorkItem(sacThread.ThreadStart);'
                        sacThread.WeedOutDLC(null);
                        
                    });
                    number++;
                    
                }
            }

            int counter = 0;
            while (tasks[counter] != null)
                counter++;

            LoadingBar loadBar = new LoadingBar(counter, "Removing DLC From Games List...");
            loadBar.Show();

            Task[] noNullTasks = new Task[counter];

            for (int i = 0; i < counter; i++)
                noNullTasks[i] = tasks[i];

            List<Task> taskWatcher = new List<Task>();
            taskWatcher.AddRange(noNullTasks);

            while (taskWatcher.Count > 0)
            {
                for (int j = 0; j < taskWatcher.Count; j++)
                {
                    if (taskWatcher[j].Status == TaskStatus.RanToCompletion)
                    {
                        taskWatcher.RemoveAt(j);
                        j--;
                        loadBar.Progress();
                    }
                }
            }

            taskWatcher.Clear();
            HelperThread.theList = null;

            #endregion

            #region Load Tags

            tasks = new Task[games.Count];
            HelperThread.theList = games;
            number = 0;

            foreach (Game game in games)
            {
                tasks[number] = Task.Factory.StartNew(() =>
                {
                    var sacThread = new HelperThread(game.GetAppID());
                    sacThread.AddTags(null);
                });
                number++;
            }

            taskWatcher.AddRange(tasks);

            LoadingBar loadBarTags = new LoadingBar(taskWatcher.Count, "Adding Tags To Games...");
            loadBar.Show();

            while (taskWatcher.Count > 0)
            {
                for (int j = 0; j < taskWatcher.Count; j++)
                {
                    if (taskWatcher[j].Status == TaskStatus.RanToCompletion)
                    {
                        taskWatcher.RemoveAt(j);
                        j--;
                        loadBarTags.Progress();
                    }
                }
            }

            #endregion

            return games;
        }



    }

    class HelperThread
    {
        private int appID;

        public static GamesList theList;


        internal HelperThread(int appid)
        {
            this.appID = appid;
        }

        internal void AddTags(object state)
        {
            try
            {
                //WebBrowser wb = new WebBrowser();
                //wb.Navigate("http://store.steampowered.com/app/" + appID + "/");

                //HtmlDocument doc = wb.Document;

                //HtmlElementCollection collection = doc.GetElementsByTagName("glance_tags popular_tags\\");

                //return;
                //WebClient client = new WebClient();
                
                //string xmlString = client.DownloadString("http://store.steampowered.com/app/" + appID + "/");

                //client.Dispose();
            }
            catch
            {

            }

        }

        

        internal void WeedOutDLC(object state)
        {
            WebResponse response = null;
            try
            {
                WebRequest request = HttpWebRequest.Create("http://steamcommunity.com/app/" + appID);

                request.Method = "HEAD";

                response = request.GetResponse() as HttpWebResponse; //request.
                

                if (response != null && !response.ResponseUri.Equals("http://steamcommunity.com/app/" + appID))
                {
                    lock (theList)
                    {
                        theList.Remove(theList.GetGame(appID));
                    }
                }

                response.Close();

            }
            catch
            {
                if(response != null)
                    response.Close();
            }

        }

    }
}
