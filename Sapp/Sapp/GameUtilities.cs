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

            bool UpdateInformationOnly = games.Count != 0;

            #region Read In Game Data

            //the username will have to be entered by the user manually the first time.
            XmlTextReader reader = new XmlTextReader("http://steamcommunity.com/profiles/" + userID + "/games?tab=all&xml=1");
            //76561198027181438 JOHNNY
            //76561198054602483 NICKS


            int appid = 0;
            bool addedNewGames = false;
            bool addedLastGame = false;
            while (reader.Read())
            {

                if (XmlNodeType.Element == reader.NodeType)
                {
                    if (reader.Name.Equals("appID"))
                    {
                        addedLastGame = false;
                        reader.Read();

                        //might throw try/catch here
                        appid = int.Parse(reader.Value);

                        reader.Read();
                        reader.Read();
                        reader.Read();
                        reader.Read();
                        string gameName = reader.Value;

                        if (!UpdateInformationOnly || !games.ContainsId(appid))
                        {
                            games.Add(new Game(gameName, appid, GameUtilities.IsInstalled(appid)));
                            addedLastGame = true;
                            addedNewGames = true;
                        }
                    }

                    else if (reader.Name.Equals("hoursLast2Weeks"))
                    {
                        reader.Read();

                        if (!UpdateInformationOnly || addedLastGame)
                            games[games.Count - 1].HoursLastTwoWeeks = TryParseDouble(reader.Value);
                        else
                            games.GetGame(appid).HoursLastTwoWeeks = TryParseDouble(reader.Value);
                        
                    }
                    else if (reader.Name.Equals("hoursOnRecord"))
                    {
                        reader.Read();

                        if (!UpdateInformationOnly || addedLastGame)
                            games[games.Count - 1].HoursOnRecord = TryParseDouble(reader.Value);
                        else
                            games.GetGame(appid).HoursOnRecord = TryParseDouble(reader.Value);
                       
                    }
                }
            }

            #endregion

            //TODO: OPTIMIZE, Keep track of new games added (if the list loads correctly) and then only update those
            if (addedNewGames)
            {

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

                int numDLC = 0;

                foreach (Game g in games)
                    if (g.IsDLC)
                        numDLC++;

                //- counter to count the dlc we will skip
                tasks = new Task[games.Count - numDLC];
                HelperThread.theList = games;
                number = 0;

                foreach (Game game in games)
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


                string htmlToParse = readStream.ReadToEnd();
                ReceiveStream.Close();
                readStream.Close();

                int startIndex = htmlToParse.IndexOf("glance_tags popular_tags");
                int endIndex = htmlToParse.IndexOf("app_tag add_button");

                htmlToParse = htmlToParse.Substring(startIndex, (endIndex - startIndex));

                //TODO: PARSE HERE!!!!! Then add the tags to the game that corresponds to this appid
                //gameslist.getgame(appid).AddTag(TheTagToAdd)

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
                        theList.GetGame(appID).IsDLC = true;
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
