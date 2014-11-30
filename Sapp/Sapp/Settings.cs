using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;

namespace Sapp
{
    [Serializable()]
    public class Settings
    {

        #region Properties

        private string userID;
        public string UserID
        {
            get { return userID; }
            set
            {
                if (writeAccess && GetSteamID64(value))
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
                if (writeAccess && Directory.Exists(value))
                    steamLocation = value;
            }
        }

        #endregion

        private static string lastUsersSteamID64;

        private static bool writeAccess;
        private static bool inUse;

        private static Settings thisInstance = new Settings();

        private Settings()
        {
            writeAccess = false;
            inUse = false;
            lastUsersSteamID64 = "";
        }

        public static Settings GetInstance(Window reciever)
        {
            if (inUse)
                return null;

            if (reciever.Title.Equals("SettingsScreen"))
                writeAccess = true;

            inUse = true;
            return thisInstance;
        }

        #region Load Settings

        public static void Initialize()
        {
            Stream sr;
            try
            {
                sr = new FileStream(@".\settings.bin", FileMode.Open);
            }
            catch (FileNotFoundException fnfe)
            {
                //file not found, open settings screen (first launch or if it isnt there)
                SettingsScreen ss = new SettingsScreen();
                ss.ShowDialog();

                Initialize();

                return;
            }

            try
            {
                IFormatter formatter = new BinaryFormatter();
                thisInstance = (Settings)formatter.Deserialize(sr);
            }
            catch (Exception ioe)
            {
                sr.Close();

                //file was corrupted, open settings screen
                SettingsScreen ss = new SettingsScreen();
                ss.ShowDialog();

                Initialize();

                return;

            }

            sr.Close();
        }

        #endregion

        public void Save()
        {
            try
            {
                Stream sw = new FileStream(@".\settings.bin", FileMode.Create);
                IFormatter formatter = new BinaryFormatter();

                formatter.Serialize(sw, thisInstance);
                sw.Close();
            }
            catch
            {
                MessageBox.Show("Settings not saved, an error occured");
            }
        }

        public void ReturnInstance(ref Settings theSettingsObject)
        {
            theSettingsObject = null;
            inUse = false;
            writeAccess = false;
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
                MessageBox.Show("User Name could not be found. Be sure to login to Steam.");
                return false;
            }

            bool foundName = false;

            while (!foundName && !steamConfig.EndOfStream)
            {
                if (!walker.Contains("{"))
                {
                    steamID64 = walker;
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
                MessageBox.Show("User Name could not be found. Be sure to login to Steam.");
                return false;
            }

            steamID64 = steamID64.Trim('\"', ' ', '\t');

            return true;
        }

        public bool ShouldRefresh()
        {
            if (steamID64.Equals(lastUsersSteamID64))
                return false;
            
            else
                lastUsersSteamID64 = steamID64;

            return true;
            
        }

    }
}
