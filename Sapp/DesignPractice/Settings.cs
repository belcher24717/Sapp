using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesignPractice
{
    public class Settings
    {
        private static string userID;
        private static bool onlyAllowInstalled;

        private static bool writeAccess;
        private static bool inUse;

        private static Settings thisInstance = new Settings();

        private Settings()
        {
            writeAccess = false;
            inUse = false;
            userID = "76561198027181438";
            onlyAllowInstalled = false;
        }

        public static Settings GetInstance(Window reciever)
        {
            if (inUse)
                return null;

            if(reciever.Title.Equals("SettingsScreen"))
                writeAccess = true;

            inUse = true;
            return thisInstance;
        }

        public void ReturnInstance(ref Settings theSettingsObject)
        {
            theSettingsObject = null;
            inUse = false;
            writeAccess = false;
        }

        public void SetUserID(string id)
        {
            if (writeAccess)
                userID = id;
        }

        public void SetInstalledGamesOnly(bool tf)
        {
            if (writeAccess)
                onlyAllowInstalled = tf;
        }

        public string GetUserID()
        {
            return userID;
        }

        public bool OnlyAllowInstalled()
        {
            return onlyAllowInstalled;
        }

    }
}
