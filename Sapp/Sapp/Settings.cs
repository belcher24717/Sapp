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
                    userID = value;
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
            catch (InvalidOperationException ioe)
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

    }
}
