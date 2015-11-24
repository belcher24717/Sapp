using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Sapp
{

    [Serializable()]
    public enum TagApplicationMethod
    {
        ContainsAll,
        ContainsOne
    }

    [Serializable()]
    public class Settings
    {

        public static String FILE_LOCATION = @".\bin";


        #region Properties

        private bool onlyPlayInstalledGames;
        public bool OnlyPlayInstalledGames
        {
            get { return onlyPlayInstalledGames; }
            set
            {
                    onlyInstalledWasChanged = true;
                    onlyPlayInstalledGames = value;
            }
        }

        private Key gamePoolRemoveKeyBinding;
        public Key GamePoolRemoveKeyBinding
        {
            get { return gamePoolRemoveKeyBinding; }
            set
            {
                 gamePoolRemoveKeyBinding = value;
            }
        }

        private string userID;
        public string UserID
        {
            get { return userID; }
            set
            {
                if (GetSteamID64(value))
                {
                    userID = value;
                }
            }
        }

        private string steamID64;
        public string SteamID64
        {
            get { return steamID64; }
        }

        private string steamLocation;
        public string SteamLocation
        {
            get { return steamLocation; }
            set
            {
                if (Directory.Exists(value))
                    steamLocation = value;
            }
        }

        private TagApplicationMethod tagApplication;
        public TagApplicationMethod TagApplication
        {
            get { return tagApplication; }
            set
            {
                tagApplication = value;
            }
        }

        //Joining
        private string joinIpAddress;
        public string JoinIpAddress
        {
            get { return joinIpAddress; }
            set
            {
                joinIpAddress = value;
            }
        }

        private string joinNickname;
        public string JoinNickname
        {
            get { return joinNickname; }
            set
            {
                joinNickname = value;
            }
        }

        private int joinPort;
        public int JoinPort
        {
            get { return joinPort; }
            set
            {
                joinPort = value;
            }
        }

        //Hosting
        private string hostNickname;
        public string HostNickname
        {
            get { return hostNickname; }
            set
            {
                hostNickname = value;
            }
        }

        private int hostPort;
        public int HostPort
        {
            get { return hostPort; }
            set
            {
                hostPort = value;
            }
        }

        #endregion

        private static bool userWasChanged;
        private static bool onlyInstalledWasChanged;
        private static List<string> columnsToShow;

        private static Settings thisInstance = new Settings();

        private Settings()
        {
            userWasChanged = false;
            onlyInstalledWasChanged = false;
            tagApplication = TagApplicationMethod.ContainsAll;
            gamePoolRemoveKeyBinding = Key.D;
            onlyPlayInstalledGames = false;
            hostPort = joinPort = 7780;
            hostNickname = joinNickname = "";
            joinIpAddress = "";
        }

        public static Settings GetInstance()
        {
            return thisInstance;
        }

        #region Load Settings

        public static void Initialize()
        {
            if (!Directory.Exists(Settings.FILE_LOCATION))
                Directory.CreateDirectory(Settings.FILE_LOCATION);

            Stream sr = new FileStream(Settings.FILE_LOCATION + @"\settings.bin", FileMode.Open);

            try
            {
                IFormatter formatter = new BinaryFormatter();
                thisInstance = (Settings)formatter.Deserialize(sr);
            }
            catch (SerializationException se)
            {
                sr.Close();
                Logger.Log("In Settings.Initialize: " + se.ToString());
                throw new SerializationException();
            }
            sr.Close();
        }

        #endregion

        public void Save()
        {
            try
            {
                if (!Directory.Exists(Settings.FILE_LOCATION))
                    Directory.CreateDirectory(Settings.FILE_LOCATION);

                Stream sw = new FileStream(Settings.FILE_LOCATION + @"\settings.bin", FileMode.Create);
                IFormatter formatter = new BinaryFormatter();

                formatter.Serialize(sw, thisInstance);
                sw.Close();
                Logger.Log("Successful Save");
            }
            catch
            {
                Logger.Log("Settings not saved, an error occured");
            }
        }

        //make run only when needed (only when username changes)
        private bool GetSteamID64(string uid)
        {
            string walker = "";
            StreamReader steamConfig;

            try
            {
                steamConfig = new StreamReader(steamLocation + @"\config\loginusers.vdf");
            }
            catch
            {
                DisplayMessage msg = new DisplayMessage("User file not found", "Please verify Steam path.", System.Windows.Forms.MessageBoxButtons.OK);
                msg.ShowDialog();
                return false;
            }

            bool foundName = false;

            string temp = "";

            while (!foundName && !steamConfig.EndOfStream)
            {
                if (!walker.Contains("{"))
                {
                    temp = walker;
                    walker = steamConfig.ReadLine();
                }
                else
                {
                    walker = steamConfig.ReadLine();
                }

                //if the user name is contained, try and see if the actual name is the one given
                if (string.Compare(walker, "\t\t\"accountname\"\t\t\"" + uid + "\"", true) == 0)
                    foundName = true;
                
            }
            steamConfig.Close();

            if (!foundName)
            {
                DisplayMessage msg = new DisplayMessage("User Name not found", "Must login to Steam with this account.", System.Windows.Forms.MessageBoxButtons.OK);
                msg.ShowDialog();
                return false;
            }

            temp = temp.Trim('\"', ' ', '\t');

            if(steamID64 == null || !steamID64.Equals(temp))
                userWasChanged = true;

            steamID64 = temp;

            return true;
        }

        public bool ShouldRefresh()
        {
            if (userWasChanged)
            {
                userWasChanged = false;
                return true;
            }

            return false;
            
        }

        public bool ShouldRefreshGamePoolOnly()
        {
            if (onlyInstalledWasChanged)
            {
                onlyInstalledWasChanged = false;
                return true;
            }
            return false;
        }

        public void AddColumn(string col)
        {
            //if the property doesnt exist, dont add the column
            try
            {
                Type gameType = typeof(Game);
                PropertyInfo myPropInfo = gameType.GetProperty(col);
            }
            catch{return;}

            if (columnsToShow == null)
                columnsToShow = new List<string>();

            if(!columnsToShow.Contains(col))
                columnsToShow.Add(col);
        }

        public void RemoveColumn(string col)
        {
            columnsToShow.Remove(col);
        }

        public List<string> GetColumnsToShow()
        {
            if (columnsToShow == null)
                columnsToShow = new List<string>();
            return columnsToShow;
        }

    }
}
