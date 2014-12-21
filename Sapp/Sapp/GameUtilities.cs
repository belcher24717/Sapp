using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
            NullTag,//used for nullification
            NoTags,//used for games that have no tags (game no longer on steam)

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
            SciFi,
            Survival,
            Horror,
            MassivelyMultiplayer,
            CoOp,
            Sandbox,
            OpenWorld,
            Stealth

            

        };

        //TODO: Add more tags
        public static GameUtilities.Tags CreateTag(string tag) // beautiful if block!
        {
            if (tag.Equals("Action"))
                return Tags.Action;
            else if (tag.Equals("Indie"))
                return Tags.Indie;
            else if (tag.Equals("Adventure"))
                return Tags.Adventure;
            else if (tag.Equals("Strategy"))
                return Tags.Strategy;
            else if (tag.Equals("RPG"))
                return Tags.RPG;
            else if (tag.Equals("Simulation"))
                return Tags.Simulation;
            else if (tag.Equals("Casual"))
                return Tags.Casual;
            else if (tag.Equals("Free to Play"))
                return Tags.FreeToPlay;
            else if (tag.Equals("Singleplayer"))
                return Tags.Singleplayer;
            else if (tag.Equals("MMO"))
                return Tags.MMO;
            else if (tag.Equals("Multiplayer"))
                return Tags.Multiplayer;
            else if (tag.Equals("Racing"))
                return Tags.Racing;
            else if (tag.Equals("Sports"))
                return Tags.Sports;
            else if (tag.Equals("Shooter"))
                return Tags.Shooter;
            else if (tag.Equals("FPS"))
                return Tags.FPS;
            else if (tag.Equals("Sci-fi"))
                return Tags.SciFi;
            else if (tag.Equals("Survival"))
                return Tags.Survival;
            else if (tag.Equals("Horror"))
                return Tags.Horror;
            else if (tag.Equals("Massively Multiplayer"))
                return Tags.MassivelyMultiplayer;
            else if (tag.Equals("Co-op"))
                return Tags.CoOp;
            else if (tag.Equals("Open World"))
                return Tags.OpenWorld;
            else if (tag.Equals("Sandbox"))
                return Tags.Sandbox;
            else if (tag.Equals("Stealth"))
                return Tags.Stealth;


            else if (tag.Equals("No Tags"))//Game no longer on steam (under that appid)
                return Tags.NoTags;
            else // tag is not recognized, won't be added
                return Tags.NullTag;

        } // end CreateTag

        public static bool IsInstalled(int id)
        {

            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App " + id);

            if (key == null)
                return false;
            return true;
        
        }

        private static void SaveGameList(GamesList games, string userID)
        {
            try
            {
                Stream sw = new FileStream(@".\" + userID + ".games", FileMode.Create);
                IFormatter formatter = new BinaryFormatter();

                formatter.Serialize(sw, games);
                sw.Close();
            }
            catch
            {
                //MessageBox.Show("Settings not saved, an error occured");
            }
        }

        private static GamesList LoadGameList(string userID)
        {

            if (File.Exists(@".\" + userID + ".games"))
            {
                Stream sr = null;
                try
                {
                    GamesList gl;

                    sr = new FileStream(@".\" + userID + ".games", FileMode.Open);

                    IFormatter formatter = new BinaryFormatter();
                    gl = (GamesList)formatter.Deserialize(sr);
                    sr.Close();

                    return gl;
                }
                catch
                {
                    if (sr != null)
                        sr.Close();
                }
            }
            
            return new GamesList();
        }

        private static double TryParseDouble(string s)
        {
            try
            {
                double ds = double.Parse(s);
                return ds;
            }
            catch
            {
                return 0.0;
            }
        }

        public static GamesList PopulateGames(string userID)
        {
            
            GamesList games = LoadGameList(userID);
            GamesList newlyAddedGames = new GamesList();

            #region Read In Game Data

            //the username will have to be entered by the user manually the first time.
            XmlTextReader reader = new XmlTextReader("http://steamcommunity.com/profiles/" + userID + "/games?tab=all&xml=1");
            //76561198027181438 JOHNNY
            //76561198054602483 NICKS

            LoadingBar initLoadBar = new LoadingBar("Updating play time...");
            initLoadBar.Show();

            int appid = 0;
            bool addedNewGames = false;
            while (reader.Read())
            {

                if (XmlNodeType.Element == reader.NodeType)
                {
                    if (reader.Name.Equals("appID"))
                    {
                        reader.Read();

                        //might throw try/catch here
                        appid = int.Parse(reader.Value);

                        reader.Read();
                        reader.Read();
                        reader.Read();
                        reader.Read();
                        string gameName = reader.Value;

                        if (!games.ContainsId(appid))
                        {
                            Game gameToAdd = new Game(gameName, appid, GameUtilities.IsInstalled(appid));
                            games.Add(gameToAdd);
                            newlyAddedGames.Add(gameToAdd);
                            addedNewGames = true;
                        }
                    }

                    else if (reader.Name.Equals("hoursLast2Weeks"))
                    {
                        reader.Read();
                        games.GetGame(appid).HoursLastTwoWeeks = TryParseDouble(reader.Value);
                    }
                    else if (reader.Name.Equals("hoursOnRecord"))
                    {
                        reader.Read();
                        games.GetGame(appid).HoursPlayed = TryParseDouble(reader.Value);
                    }
                }
            }

            initLoadBar.ForceClose();

            #endregion

            if (addedNewGames)
            {
                Logger.Log("START: DLC Removal", true);

                #region Weed Out DLC

                Task[] tasks = new Task[games.Count];
                HelperThread.theList = games;

                //gets rid of dlc, using multiple threads (tasks)
                int number = 0;

                foreach (Game g in newlyAddedGames)
                {

                    if (g.HoursPlayed == 0)
                    {
                        tasks[number] = Task.Factory.StartNew(() =>
                        {
                            var sacThread = new HelperThread(g.GetAppID());
                            //ThreadPool.QueueUserWorkItem(sacThread.ThreadStart);
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

                loadBar.ForceClose();
                Logger.Log("END: DLC Removal", true);

                Logger.Log("START: Tag Loading", true);

                #region Load Tags

                int numDLC = 0;

                foreach (Game g in newlyAddedGames)
                    if (g.IsDLC)
                        numDLC++;

                //- counter to count the dlc we will skip
                tasks = new Task[newlyAddedGames.Count - numDLC];
                HelperThread.theList = games;
                number = 0;

                foreach (Game game in newlyAddedGames)
                {
                    if (!game.IsDLC)
                    {
                        tasks[number] = Task.Factory.StartNew(() =>
                        {
                            var sacThread = new HelperThread(game.GetAppID());
                            sacThread.AddTags(null);
                        });
                        number++;
                    }
                }

                taskWatcher.AddRange(tasks);

                LoadingBar loadBarTags = new LoadingBar(taskWatcher.Count, "Adding Tags To Games...");
                loadBarTags.Show();

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

                loadBarTags.ForceClose();
                Logger.Log("END: Tag Loading", true);

            }

            SaveGameList(games, userID);

            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].IsDLC)
                {
                    games.RemoveAt(i);
                    i--;
                }
            }

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
            //Done for debugging purposes
            string htmlToParse;
            int startIndex;
            int endIndex;
            int index;
            try
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://store.steampowered.com/app/" + appID + "/");
                wr.CookieContainer = new CookieContainer();
                wr.CookieContainer.Add(new Cookie("birthtime", "", "/", "store.steampowered.com"));

                WebResponse response = wr.GetResponse();

                // Obtain a 'Stream' object associated with the response object.
	            Stream ReceiveStream = response.GetResponseStream();
	            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

                // Pipe the stream to a higher level stream reader with the required encoding format. 
	            StreamReader readStream = new StreamReader( ReceiveStream, encode );

                htmlToParse = readStream.ReadToEnd();
                ReceiveStream.Close();
                readStream.Close();

                startIndex = htmlToParse.IndexOf("glance_tags popular_tags");
                endIndex = htmlToParse.IndexOf("app_tag add_button");

                htmlToParse = htmlToParse.Substring(startIndex, (endIndex - startIndex));

                while (true)
                {
                    index = htmlToParse.IndexOf("http://store.steampowered.com/tag");

                    //no tags left
                    if (index == -1)
                        break;

                    htmlToParse = htmlToParse.Substring(index);
                    index = htmlToParse.IndexOf('>');
                    htmlToParse = htmlToParse.Substring(index);

                    index = htmlToParse.IndexOf('<');

                    string tagToAdd = htmlToParse.Substring(1, index - 1);

                    htmlToParse = htmlToParse.Substring(index);

                    theList.GetGame(appID).AddTag(tagToAdd.Trim());
                }

            }
            catch
            {
                //If it comes here the store page probably does not exist due to
                //some kind of removal from steam, Mark as untagged.
                theList.GetGame(appID).AddTag("No Tags");
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
                        theList.GetGame(appID).IsDLC = true;
                    }
                }

                response.Close();

            }
            catch(Exception e)
            {
                Logger.Log("In HelperThread.WeedOutDLC: " + e.ToString(), true);

                if(response != null)
                    response.Close();
            }

        }

    }
}
