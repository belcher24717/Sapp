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
                if (writeAccess)
                {
                    userID = value;
                    GetSteamID64();
                }
            }
        }

        private bool onlyAllowInstalled;
        public bool OnlyAllowInstalled
        {
            get { return onlyAllowInstalled; }
            set
            {
                if (writeAccess)
                    onlyAllowInstalled = value;
            }
        }

        private string steamID64;
        public string SteamID64
        {
            get { return steamID64; }
            //set
            //{
             //   GetSteamID64();
                //if (writeAccess)
                    //steamID64 = value;
            //}
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

        private static bool writeAccess;
        private static bool inUse;

        
        

        private static Settings thisInstance = new Settings();

        private Settings()
        {
            writeAccess = false;
            inUse = false;
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
            Stream sw = new FileStream(@".\settings.bin", FileMode.Create);
            IFormatter formatter = new BinaryFormatter();

            formatter.Serialize(sw, thisInstance);
            sw.Close();
        }

        public void ReturnInstance(ref Settings theSettingsObject)
        {
            theSettingsObject = null;
            inUse = false;
            writeAccess = false;
        }

        //make run only when needed (only when username changes)
        private void GetSteamID64()
        {
            string walker = "";
            StreamReader steamConfig;

            try
            {
                steamConfig = new StreamReader(steamLocation + @"\config\loginusers.vdf");
            }
            catch
            {
                return;
            }

            while (!walker.Contains(userID))
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
            }
            steamConfig.Close();

            steamID64 = steamID64.Substring(2, steamID64.Length - 2);

            int i = steamID64.IndexOf('\"');

            //76561198027181438
            steamID64 = steamID64.Substring(0, i);
        }

    }
}
