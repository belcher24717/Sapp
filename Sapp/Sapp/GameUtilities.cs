using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            Competitive,
            Utilities
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
            else if (tag.Equals("Survival"))
                return Tags.Survival;
            else if (tag.Equals("Horror"))
                return Tags.Horror;
            else if (tag.Equals("Massively Multiplayer"))
                return Tags.MassivelyMultiplayer;
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
            else if (tag.Equals("Puzzle"))
                return Tags.Puzzle;
            else if (tag.Equals("Funny"))
                return Tags.Funny;
            else if (tag.Equals("Dark"))
                return Tags.Dark;
            else if (tag.Equals("Tactical"))
                return Tags.Tactical;
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
            else if (tag.Equals("Co-op"))
                return Tags.CoOp;
            else if (tag.Equals("Turn-Based"))
                return Tags.TurnBased;
            else if (tag.Equals("Rogue-like"))
                return Tags.Roguelike;
            else if (tag.Equals("Sci-fi"))
                return Tags.SciFi;

            else if (tag.Equals("Utilities"))
                return Tags.Utilities;
            else if (tag.Equals("No Tags"))//Game no longer on steam (under that appid)
                return Tags.NoTags;
            else // tag is not recognized, won't be added
                return Tags.NullTag;

        } // end CreateTag

        public static bool IsInstalled(Int64 id)
        {
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App " + id);

            if (key == null)
                return false;

            return true; 
        }

        public static void VerifyInstalledGames(GamesList games)
        {
            string gamesLocation = Settings.GetInstance().SteamLocation + @"\steamapps\common";

            if (!Directory.Exists(gamesLocation))
                return;

            string[] tempGamePaths = Directory.GetDirectories(gamesLocation);
            List<string> gameTitles = new List<string>();

            foreach (string s in tempGamePaths)
                gameTitles.Add(FixGameTitle(new DirectoryInfo(s).Name));

            foreach (Game g in games)
            {
                if(g.IsInstalled == false)
                    g.IsInstalled = gameTitles.Contains(FixGameTitle(g.Title));
            }
        }

        private static string FixGameTitle(string gameName)
        {
            gameName = gameName.ToLower();
            gameName = gameName.Replace(" ", "");
            gameName = gameName.Replace("(TM)", "");
            gameName = gameName.Replace("™", "");
            gameName = gameName.Replace("(R)", "");
            gameName = gameName.Replace("®", "");
            gameName = gameName.Replace(":", "");
            gameName = gameName.Replace("(", "");
            gameName = gameName.Replace(")", "");
            gameName = gameName.Replace("_", "");
            gameName = gameName.Replace("-", "");
            gameName = gameName.Replace("!", "");
            gameName = gameName.Replace(".", "");
            gameName = gameName.Replace("'", "");
            gameName = gameName.Replace("\"", "");

            gameName = gameName.Replace("&", "and");
            gameName = gameName.Replace("gameoftheyear", "goty");
            gameName = gameName.Replace("gotyedition", "goty");

            return gameName;
        }

        public static void SaveGameList(GamesList games, string fileName, string fileExtention, bool useFileNameAsPath = false)
        {
            string filename;
            Stream sw = null;
            try
            {
                if (!Directory.Exists(Settings.FILE_LOCATION))
                    Directory.CreateDirectory(Settings.FILE_LOCATION);

                if (useFileNameAsPath)
                    filename = fileName;
                else
                    filename = Settings.FILE_LOCATION + @"\" + fileName + "." + fileExtention;

                sw = new FileStream(filename, FileMode.Create);
                IFormatter formatter = new BinaryFormatter();

                formatter.Serialize(sw, games);
                sw.Close();
            }
            catch(Exception e)
            {
                Logger.LogError("<GameUtilities.SaveGameList> Problem saving - " + e.ToString());
                if (sw != null)
                    sw.Close();
            }
        }

        public static GamesList LoadGameList(string fileNamePassedIn, string fileExtention, bool useFileNameAsPath = false)
        {
            string filename;

            if (!Directory.Exists(Settings.FILE_LOCATION))
                Directory.CreateDirectory(Settings.FILE_LOCATION);

            if (useFileNameAsPath)
                filename = fileNamePassedIn;
            else
                filename = Settings.FILE_LOCATION + @"\" + fileNamePassedIn + "." + fileExtention;

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
                    Logger.LogError("<GameUtilities.LoadGameList> Problem loading: " + e.ToString());
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
            XmlTextReader reader = GetXmlReader(userID);
            if (reader == null)
                goto Offline;

            //76561198027181438 JOHNNY
            //76561198054602483 NICKS

            LoadingBar initLoadBar = new LoadingBar("Updating play time...");
            initLoadBar.Show();
            initLoadBar.Activate();

            int appid = 0;

            try
            {
                while (reader.Read())
                {
                    
                    if (XmlNodeType.Element == reader.NodeType)
                    {
                        if (reader.Name.Equals("meta"))
                        {
                            Logger.LogWarning("Private profile, skipping game retrieval");
                            //private profile, we've been redirected
                            goto Offline;
                        }
                        if (reader.Name.Equals("appID"))
                        {
                            reader.Read();

                            //Logger.Log("AppID: " + reader.Value);
                            appid = int.Parse(reader.Value);

                            while(reader.NodeType != XmlNodeType.CDATA)
                              reader.Read();

                            string gameName = reader.Value;

                            //Logger.Log("Game: " + gameName);

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

            }
            catch (WebException we)
            {
                Logger.LogError("<GameUtilities.PopulateList> Internet connection was lost during game information acquisition: " + we.Message, true);
                goto Offline;
                //TODO: Exit execution to retry/exit window
                /*if (addedNewGames)
                    return null;
                else if (games == null || games.Count == 0)
                    return null;
                else
                    return games;*/
            }
            catch (Exception e)
            {
                Logger.LogError("<GameUtilities.PopulateList> An error occured: " + e.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                initLoadBar.ForceClose();
            }

            #endregion

            #region Checking Flagged DLC

            try
            {
                List<Int64> failedDLCs = games.ReturnFailedDlcCheckList();

                if (failedDLCs.Count != 0)
                {
                    Logger.Log("START: Check Flagged DLC", true);

                    // games is the list of potential DLC
                    Task[] tasks = new Task[failedDLCs.Count];
                    HelperThread.theList = games;

                    // used to assign a thread to its array position
                    int number = 0;

                    foreach (int appIdOfGame in failedDLCs)
                    {
                        tasks[number] = Task.Factory.StartNew(() =>
                        {
                            var sacThread = new HelperThread(appIdOfGame);
                            sacThread.WeedOutDLC(null);

                        });
                        number++;
                    }

                    //start the loading bar so they see something happening
                    int counter = failedDLCs.Count;
                    LoadingBar loadBar = new LoadingBar(counter, "Checking For Unmarked DLC...");
                    loadBar.Show();

                    List<Task> taskWatcher = new List<Task>();
                    taskWatcher.AddRange(tasks);

                    while (taskWatcher.Count > 0)
                    {
                        for (int j = 0; j < taskWatcher.Count; j++)
                        {
                            if (taskWatcher[j].Status == TaskStatus.RanToCompletion || taskWatcher[j].Status == TaskStatus.Faulted)
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
                    loadBar.ForceClose();

                    Logger.Log("END: Check Flagged DLC", true);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("While Checking Flagged DLC an error occurred: " + e.Message);
            }

            #endregion

            #region Checking Flagged Tagging

            try
            {
                List<Int64> failedTags = games.ReturnFailedTaggingList();

                if (failedTags.Count != 0)
                {
                    List<Task> taskWatcher = new List<Task>();
                    Task[] tasks = new Task[games.Count];
                    HelperThread.theList = games;

                    tasks = new Task[failedTags.Count];
                    HelperThread.theList = games;
                    int number = 0;

                    foreach (Int64 gameID in failedTags)
                    {
                        tasks[number] = Task.Factory.StartNew(() =>
                        {
                            var sacThread = new HelperThread(gameID);
                            sacThread.AddTags(null);
                        });
                        number++;
                    }

                    taskWatcher.AddRange(tasks);

                    LoadingBar loadBarTags = new LoadingBar(taskWatcher.Count, "Re-Tagging Games...");
                    loadBarTags.Show();

                    while (taskWatcher.Count > 0)
                    {
                        for (int j = 0; j < taskWatcher.Count; j++)
                        {
                            if (taskWatcher[j].Status == TaskStatus.RanToCompletion || taskWatcher[j].Status == TaskStatus.Faulted)
                            {
                                taskWatcher.RemoveAt(j);
                                j--;
                                loadBarTags.Progress();
                            }
                        }
                        Application.DoEvents();
                    }

                    foreach (Int64 gameID in failedTags)
                    {
                        if (games.GetGame(gameID).ContainsTag(Tags.Utilities))
                            games.GetGame(gameID).IsUtility = true;
                    }

                    HelperThread.theList = null;
                }
            }
            catch (Exception e)
            {
                Logger.LogError("While Checking Flagged Tags an error occurred: " + e.Message);
            }

            #endregion

            try
            {
                if (addedNewGames)
                {
                    Logger.Log("START: DLC Removal", true);

                    #region Weed Out DLC

                    // games is the list of potential DLC
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
                                sacThread.WeedOutDLC(null);

                            });
                            number++;
                        }
                    }

                    int counter = 0;
                    while (tasks[counter] != null)
                        counter++;

                    LoadingBar loadBar = new LoadingBar(counter, "Marking DLC...");
                    loadBar.Show();
                    loadBar.Activate();

                    Task[] noNullTasks = new Task[counter];

                    for (int i = 0; i < counter; i++)
                        noNullTasks[i] = tasks[i];

                    List<Task> taskWatcher = new List<Task>();
                    taskWatcher.AddRange(noNullTasks);

                    while (taskWatcher.Count > 0)
                    {
                        for (int j = 0; j < taskWatcher.Count; j++)
                        {
                            if (taskWatcher[j].Status == TaskStatus.RanToCompletion || taskWatcher[j].Status == TaskStatus.Faulted)
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

                    //- counter to count the dlc we will skip -- Because DLC does not have tags
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

                    LoadingBar loadBarTags = new LoadingBar(taskWatcher.Count, "Tagging Games...");
                    loadBarTags.Show();
                    loadBarTags.Activate();

                    while (taskWatcher.Count > 0)
                    {
                        for (int j = 0; j < taskWatcher.Count; j++)
                        {
                            if (taskWatcher[j].Status == TaskStatus.RanToCompletion || taskWatcher[j].Status == TaskStatus.Faulted)
                            {
                                taskWatcher.RemoveAt(j);
                                j--;
                                loadBarTags.Progress();
                            }
                        }
                        Application.DoEvents();
                    }

                    foreach (Game game in newlyAddedGames)
                    {
                        if (game.ContainsTag(Tags.Utilities))
                            game.IsUtility = true;
                    }

                    HelperThread.theList = null;

                    #endregion

                    loadBarTags.ForceClose();
                    Logger.Log("END: Tag Loading", true);

                    foreach (Game g in games)
                        if (g.ContainsTag(Tags.NoTags) && games.Count(x => x.Title.Equals(g.Title)) > 1)
                            g.IsDLC = true;
                }
            }
            catch (Exception e)
            {
                Logger.LogError("While creating new games an error occurred: " + e.Message);
                return null;
            }

            if (!Directory.Exists(Settings.FILE_LOCATION))
                Directory.CreateDirectory(Settings.FILE_LOCATION);

            SaveGameList(games, userID, "games");

            Offline:

            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].IsDLC || games[i].IsUtility)
                {
                    games.RemoveAt(i);
                    i--;
                }
            }

            foreach (Game g in games)
            {
                g.SetInstallState(GameUtilities.IsInstalled(g.GetAppID()));
            }
            UpdateCustomGameAppId(games);
            VerifyInstalledGames(games);

            return games;
        }

        private static XmlTextReader GetXmlReader(string userID)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = false;

            HttpClient client;
            string responseString;
            Uri redirection;
            XmlTextReader reader = null;

            try
            {
                //XmlTextReader test = new XmlTextReader("http://steamcommunity.com/profiles/" + userID + "/games?tab=all&xml=1");
                client = new HttpClient(httpClientHandler);
                client.BaseAddress = new Uri("http://steamcommunity.com/profiles/" + userID + "/games?tab=all&xml=1");
                Task<HttpResponseMessage> test = client.GetAsync("http://steamcommunity.com/profiles/" + userID + "/games?tab=all&xml=1");
                test.Wait(1000);
                if (test.Exception != null)
                    throw test.Exception;

                redirection = test.Result.Headers.Location;

                if (redirection != null)
                {
                    //check if private profile
                }

                Task<string> responseTest = test.Result.Content.ReadAsStringAsync();
                responseString = responseTest.Result;
            }
            catch
            {
                Logger.LogWarning("Offline, skipping game check");
                return null;
            }

            //check if profile is private if it is use last else, otherwise you can use the redirection if its not null

            if (responseString.Contains(userID))
            {
                TextReader tr = new StringReader(responseString);
                reader = new XmlTextReader(tr);
            }
            else if (redirection != null)
            {
                reader = new XmlTextReader(redirection.ToString() + "/games?tab=all&xml=1");
            }
            else
            {
               reader = new XmlTextReader("http://steamcommunity.com/profiles/" + userID + "/games?tab=all&xml=1");
            }
                
            return reader;
        }

        //Newly added. Custom games that are updated may change in size. Updating their appId ensures Custom games can be played using the Connect feature...
        private static void UpdateCustomGameAppId(GamesList games)
        {
            //TODO: Maybe run through list and create list of custom games...
            //      Then run through each custom game and update...
            //      Then load the file, delete these games if found and then save out these games for persistence of update...
            foreach (Game game in games)
            {
                if (game.GetAppID() < 0)
                {
                    if (File.Exists(game.FilePath))
                    {
                        game.SetAppId(-(Int64)(new System.IO.FileInfo(game.FilePath).Length));
                    }
                    else
                    {
                        game.IsInstalled = false;
                    }
                }
            }
        }

        public static GamesList IntersectLists(GamesList oldList, Int64[] gameIds)
        {
            GamesList tempList = new GamesList();

            foreach (Game g in oldList)
            {
                if (gameIds.Contains(g.GetAppID()))
                    tempList.Add(g);
            }

            return tempList;
        }
    
    }

    class HelperThread
    {
        private Int64 appID;

        public static GamesList theList;

        internal HelperThread(Int64 appid)
        {
            this.appID = appid;
        }

        internal void AddTags(object state)
        {
            try
            {
                //assume failure
                theList.GetGame(appID).TaggingFailed = true;

                string htmlToParse;
                int startIndex;
                int endIndex;
                int index;

                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://store.steampowered.com/app/" + appID + "/");
                wr.CookieContainer = new CookieContainer();
                wr.CookieContainer.Add(new Cookie("birthtime", "", "/", "store.steampowered.com"));

                WebResponse response = wr.GetResponse();

                // Obtain a 'Stream' object associated with the response object.
                Stream ReceiveStream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

                // Pipe the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(ReceiveStream, encode);

                htmlToParse = readStream.ReadToEnd();
                ReceiveStream.Close();
                readStream.Close();

                if (htmlToParse.IndexOf("http://store.steampowered.com/search/?category2=24&snr=1_5_9__423") > 0 || //Local Co-op
                   htmlToParse.IndexOf("http://store.steampowered.com/search/?category2=1&snr=1_5_9__423") > 0 || //Multi-player
                   htmlToParse.IndexOf("http://store.steampowered.com/search/?category2=9&snr=1_5_9__423") > 0) //Co-op
                {
                    theList.GetGame(appID).Multiplayer = true;
                }

                startIndex = htmlToParse.IndexOf("glance_tags popular_tags");
                endIndex = htmlToParse.IndexOf("app_tag add_button");

                htmlToParse = htmlToParse.Substring(startIndex, (endIndex - startIndex));

                // run until no tags left
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

                theList.GetGame(appID).TaggingFailed = false;
            }
            catch (WebException we)
            {
                Logger.LogError("<GameUtilities.AddTags> Lost internet connection during execution: " + we.Message, true);
            }
            catch(Exception exception)
            {
                //If it comes here the store page probably does not exist due to
                //some kind of removal from steam, Mark as untagged.
                if (exception is ArgumentOutOfRangeException || exception is IndexOutOfRangeException)
                {
                    theList.GetGame(appID).AddTag("No Tags");
                    theList.GetGame(appID).TaggingFailed = false;
                    return;
                }
                throw;
            }
        }

        internal void WeedOutDLC(object state)
        {
            bool checkWorked = WeedOutDLCCommunityCheck();

            if (!checkWorked)
                WeedOutDLCStoreCheck();
        }

        private bool WeedOutDLCStoreCheck()
        {
            bool successful = false;

            try
            {
                string htmlToParse;

                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://store.steampowered.com/app/" + appID + "/");
                wr.CookieContainer = new CookieContainer();
                wr.CookieContainer.Add(new Cookie("birthtime", "", "/", "store.steampowered.com"));

                using (WebResponse response = wr.GetResponse())
                {

                    // Obtain a 'Stream' object associated with the response object.
                    Stream ReceiveStream = response.GetResponseStream();
                    Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

                    // Pipe the stream to a higher level stream reader with the required encoding format. 
                    StreamReader readStream = new StreamReader(ReceiveStream, encode);

                    htmlToParse = readStream.ReadToEnd();
                    ReceiveStream.Close();
                    readStream.Close();
                }

                if (htmlToParse.Contains("game_area_dlc_bubble")) //|| htmlToParse.Contains("<title>Welcome to Steam</title>"))
                {
                    theList.GetGame(appID).IsDLC = true;
                    theList.GetGame(appID).DlcCheckFailed = false;
                }
                else
                    theList.GetGame(appID).DlcCheckFailed = false;

                //if we get to the end of the try it was a successful attempt
                successful = true;
            }
            catch (WebException we)
            {
                Logger.LogError("<GameUtilities.WeedOutDLC.WeedOutDLCStoreCheck> WebExeption during DLC removal. \nappID: " + appID + " \nmessage: " + we.Message, true);

                theList.GetGame(appID).DlcCheckFailed = true;
            }
            catch (Exception e) { Logger.LogError("<GameUtilities.WeedOutDLC.WeedOutDLCStoreCheck> \nappID: " + appID + " \nmessage: " + e.Message, true); }

            return successful;
        }

        private bool WeedOutDLCCommunityCheck()
        {
            bool successful = false;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://steamcommunity.com/app/" + appID);
                request.Method = "HEAD";
                request.Timeout = 10 * 1000;

                string location;
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    location = response.ResponseUri.AbsoluteUri;
                }
                if (location != "http://steamcommunity.com/app/" + appID)
                {
                    theList.GetGame(appID).IsDLC = true;
                    theList.GetGame(appID).DlcCheckFailed = false;
                }
                else
                    theList.GetGame(appID).DlcCheckFailed = false;

                successful = true;
            }
            catch (WebException we)
            {
                Logger.LogError("<GameUtilities.WeedOutDLC.WeedOutDLCCommunityCheck> WebExeption during DLC removal. \nappID: " + appID + " \nmessage: " + we.Message, true);

                theList.GetGame(appID).DlcCheckFailed = true;
            }
            catch (Exception e) { Logger.LogError("<GameUtilities.WeedOutDLC.WeedOutDLCCommunityCheck> \nappID: " + appID + " \nmessage: " + e.Message, true); }

            return successful;
        }

    }
}
