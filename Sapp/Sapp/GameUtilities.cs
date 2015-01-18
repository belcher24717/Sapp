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
            Stealth,

            Platformer,
            Building,
            MOBA,
            Exploration,
            Roguelike,
            Puzzle,
            Dark,
            Tactical,
            TurnBased,
            Mystery,
            Fantasy,
            Funny,
            Arcade,
            Driving,
            Retro,
            Relaxing,
            Difficult,
            Comedy,
            RTS,
            StoryRich,
            Competitive


            

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
            else if (tag.Equals("Platformer"))
                return Tags.Platformer;
            else if (tag.Equals("Building"))
                return Tags.Building;
            else if (tag.Equals("MOBA"))
                return Tags.MOBA;
            else if (tag.Equals("Exploration"))
                return Tags.Exploration;
            else if (tag.Equals("Rogue-like"))
                return Tags.Roguelike;
            else if (tag.Equals("Puzzle"))
                return Tags.Puzzle;
            else if (tag.Equals("Funny"))
                return Tags.Funny;
            else if (tag.Equals("Dark"))
                return Tags.Dark;
            else if (tag.Equals("Tactical"))
                return Tags.Tactical;
            else if (tag.Equals("Turn-Based"))
                return Tags.TurnBased;
            else if (tag.Equals("Mystery"))
                return Tags.Mystery;
            else if (tag.Equals("Fantasy"))
                return Tags.Fantasy;
            else if (tag.Equals("Arcade"))
                return Tags.Arcade;
            else if (tag.Equals("Driving"))
                return Tags.Driving;
            else if (tag.Equals("Retro"))
                return Tags.Retro;
            else if (tag.Equals("Relaxing"))
                return Tags.Relaxing;
            else if (tag.Equals("Difficult"))
                return Tags.Difficult;
            else if (tag.Equals("Comedy"))
                return Tags.Comedy;
            else if (tag.Equals("RTS"))
                return Tags.RTS;
            else if (tag.Equals("Story Rich"))
                return Tags.StoryRich;
            else if (tag.Equals("Competitive"))
                return Tags.Competitive;

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

        public static void SaveGameList(GamesList games, string fileName, string fileExtention)
        {
            Stream sw = null;
            try
            {
                sw = new FileStream(@".\" + fileName + "." + fileExtention, FileMode.Create);
                IFormatter formatter = new BinaryFormatter();

                formatter.Serialize(sw, games);
                sw.Close();
            }
            catch(Exception e)
            {
                Logger.Log("Error: Problem saving in GameUtilities.SaveGameList - " + e.ToString());
                if (sw != null)
                    sw.Close();
            }
        }

        public static GamesList LoadGameList(string fileNamePassedIn, string fileExtention, bool useFileNameAsPath)
        {
            string filename;
            if (useFileNameAsPath)
                filename = fileNamePassedIn;
            else
                filename = @".\" + fileNamePassedIn + "." + fileExtention;

            if (File.Exists(filename))
            {
                Stream sr = null;
                try
                {
                    GamesList gl;

                    sr = new FileStream(filename, FileMode.Open);

                    IFormatter formatter = new BinaryFormatter();
                    gl = (GamesList)formatter.Deserialize(sr);
                    sr.Close();

                    return gl;
                }
                catch (Exception e)
                {
                    Logger.Log("Error: Problem loading in GameUtilities.LoadGameList - " + e.ToString());
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
            
            GamesList games = LoadGameList(userID, "games", false);
            GamesList newlyAddedGames = new GamesList();
            bool addedNewGames = false;

            #region Read In Game Data

            //the username will have to be entered by the user manually the first time.
            XmlTextReader reader = new XmlTextReader("http://steamcommunity.com/profiles/" + userID + "/games?tab=all&xml=1");
            //76561198027181438 JOHNNY
            //76561198054602483 NICKS

            LoadingBar initLoadBar = new LoadingBar("Updating play time...");
            initLoadBar.Show();

            int appid = 0;
            while (reader.Read())
            {

                if (XmlNodeType.Element == reader.NodeType)
                {
                    if (reader.Name.Equals("appID"))
                    {
                        reader.Read();

                        //might throw try/catch here
                        Logger.Log("AppID: " + reader.Value);
                        appid = int.Parse(reader.Value);

                        reader.Read();
                        reader.Read();
                        reader.Read();
                        reader.Read();
                        string gameName = reader.Value;

                        Logger.Log("Game: " + gameName);

                        if (!games.ContainsId(appid))
                        {
                            Game gameToAdd = new Game(gameName, appid, GameUtilities.IsInstalled(appid));
                            games.Add(gameToAdd);
                            newlyAddedGames.Add(gameToAdd);
                            addedNewGames = true;
                        }
                        //need to reset this value so it is properly updated if no hours have been played
                        else
                        {
                            games.GetGame(appid).Last2Weeks = 0;
                            games.GetGame(appid).SetInstallState(GameUtilities.IsInstalled(appid));
                        }
                    }

                    else if (reader.Name.Equals("hoursLast2Weeks"))
                    {
                        reader.Read();
                        games.GetGame(appid).Last2Weeks = TryParseDouble(reader.Value);
                    }
                    else if (reader.Name.Equals("hoursOnRecord"))
                    {
                        reader.Read();
                        games.GetGame(appid).HoursPlayed = TryParseDouble(reader.Value);
                    }
                }
                Application.DoEvents(); 
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
                    Application.DoEvents();
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
                    Application.DoEvents();
                }

                #endregion

                loadBarTags.ForceClose();
                Logger.Log("END: Tag Loading", true);

            }

            SaveGameList(games, userID, "games");

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

                    Logger.Log("GameUtilities.AddTags - Index: " + index + " Length: " + htmlToParse.Length);

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
                    Logger.Log(theList.GetGame(appID).Title + " IS DLC");
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
