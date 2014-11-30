using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Sapp
{
    public abstract class GameUtilities
    {
        public static bool IsInstalled(int id)
        {

            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App " + id);

            if (key == null)
                return false;
            return true;
        
        }

        //very inefficient. Find another way to do this
        public static void CheckIfDLC(int appid)
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


            if (appid == 214933)
            {
            }
            //ThreadPool.SetMaxThreads(9, 9);
            ThreadPool.QueueUserWorkItem(ThreadTesting, appid);
            /*
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://steamcommunity.com/app/" + appid);

                request.Method = "HEAD";
                request.AllowAutoRedirect = true;
                request.Timeout = 15000;

                HttpWebResponse response = request.GetResponse() as HttpWebResponse; //request.


                return response.ResponseUri.AbsolutePath.Equals("http://steamcommunity.com/app/" + appid);
            }
            catch
            {
                return false;
            }*/
        }

        
        private static void ThreadTesting(object appID)
        {
            int appid = (int)appID;
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://steamcommunity.com/app/" + appid);

                request.Method = "HEAD";
                request.AllowAutoRedirect = true;
                request.Timeout = 15000;

                HttpWebResponse response = request.GetResponse() as HttpWebResponse; //request.

                //this dlc never comes through?
                if (appid == 214933)
                {
                }

                //return response.ResponseUri.AbsolutePath.Equals("http://steamcommunity.com/app/" + appid);
                if (!response.ResponseUri.Equals("http://steamcommunity.com/app/" + appid))
                {

                }

            }
            catch
            {
                
            }
        }

        public static GamesList PopulateGames(string userID)
        {
            GamesList games = new GamesList();

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

                        //might need to use game added if DLC can EVER have hours tied to it.
                        //if (GameUtilities.CheckIfDLC(appid))
                        //{
                            games.Add(new Game(gameName, appid, GameUtilities.IsInstalled(appid)));
                            GameUtilities.CheckIfDLC(appid);
                        //}
                    }

                    else if (reader.Name.Contains("hours"))
                    {
                        reader.Read();
                        games[games.Count - 1].AddGameTime(reader.Value);
                    }
                }

            }

            return games;
        }

    }
}
