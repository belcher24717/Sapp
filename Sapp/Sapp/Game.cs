using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Sapp
{
    [Serializable()]
    public class Game : IComparable<Game>, IEquatable<Game>
    {
        // maybe consolidate these attributes into a GameProperties object?
        private Int64 appID;
        private int lastTimePlayed;
        private List<GameUtilities.Tags> _tagList;

        public bool IsDLC
        {
            get;
            set;
        }

        // newly added... adds the ability to set isInstalled of the game, specifically if it's a custom game who's path can't be found
        public bool IsInstalled
        {
            get;
            private set;
        }

        public bool IsInstalledManually
        {
            get;
            private set;
        }

        public bool IsUtility
        {
            get;
            set;
        }

        public double Last2Weeks
        {
            get;
            set;
        }

        public double HoursPlayed
        {
            get;
            set;
        }

        public bool DlcCheckFailed
        {
            get;
            set;
        }

        public bool TaggingFailed
        {
            get;
            set;
        }

        private string title;
        public string Title
        {
            get { return title; }
        }

        public string FilePath
        {
            set;
            get;
        }

        public bool Multiplayer
        {
            get;
            set;
        }

        //start with just title and either build the rest in later, or add them here
        public Game(string title, Int64 appid, bool installed)
        {
            this.title = title;
            this.appID = appid;
            this.IsInstalled = installed;

            this.Last2Weeks = 0;
            this.HoursPlayed = 0;
            IsDLC = false;
            DlcCheckFailed = false;

            _tagList = new List<GameUtilities.Tags>();
 //           PopulateTags(tags);
        }

        public Int64 GetAppID()
        {
            return appID;
        }

        // Newly added. Implemented because custom games may need to be re-set on Snowflake load...
        public void SetAppId(Int64 appId)
        {
            this.appID = appId;
        }

        public override string ToString()
        {
            return this.title;
        }

        public void ChangeTitle(string newTitle)
        {
            title = newTitle;
        }

        public int CompareTo(Game other)
        {
            return this.ToString().CompareTo(other.ToString());
        }

        public void Launch()
        {
            if (appID >= 0)
            {
                try
                { Process.Start("steam://run/" + appID); }
                catch (Exception e)
                {
                    Logger.LogError("The launch of " + this.Title + " failed. Reason: " + e.Message, true);
                }

            }
            else
            {
                if (File.Exists(FilePath))
                    Process.Start(FilePath);
                else
                {
                    this.IsInstalled = false;
                    DisplayMessage dm = new DisplayMessage("Executable Not Found", "The executable may have been moved or deleted.", System.Windows.Forms.MessageBoxButtons.OK);
                    dm.ShowDialog();
                }
            }
        }

        public void AddTag(string tag)
        {
            GameUtilities.Tags newTag = GameUtilities.CreateTag(tag);

            if (newTag != GameUtilities.Tags.NullTag)
                _tagList.Add(newTag);
            else
            {
                // maybe do something if it's a null tag!
            }
        }

        public void AddTag(GameUtilities.Tags tag)
        {
            if (tag != GameUtilities.Tags.NullTag)
                _tagList.Add(tag);
            else
            {
                // maybe do something if it's a null tag!
            }
        }

        public void RemoveTag(string tag)
        {
            _tagList.Remove(GameUtilities.CreateTag(tag));
        }

        //overload
        public void RemoveTag(GameUtilities.Tags tag)
        {
            _tagList.Remove(tag);
        }

        public void ClearTags()
        {
            _tagList.Clear();
        }

        public List<GameUtilities.Tags> GetTags()
        {
            return _tagList;
        }

        //This method will have 2 behaviors based on settings
        public bool ContainsTag(List<GameUtilities.Tags> tagsApplied, TagApplicationMethod method)
        {
            if (method == TagApplicationMethod.ContainsAll)
            {
                foreach (GameUtilities.Tags tag in tagsApplied)
                {
                    if (!_tagList.Contains(tag))
                        return false;
                }
                return true;
            }

            else if (method == TagApplicationMethod.ContainsOne)
            {
                foreach (GameUtilities.Tags tag in tagsApplied)
                    if (_tagList.Contains(tag))
                        return true;
            }

            return false;
        }

        public bool ContainsTag(GameUtilities.Tags tag) // overload
        {
            if (_tagList.Contains(tag))
                return true;

            return false;
        }

        public bool Equals(Game other)
        {
            return this.appID == other.GetAppID();
        }

        public void SetIsInstalled(bool installed, bool manual = false)
        {
            if (installed && !manual)
            {
                IsInstalled = installed;
                IsInstalledManually = false;
            }

            else if (!IsInstalledManually || manual)
                IsInstalled = installed;

            if (manual)
                IsInstalledManually = !IsInstalledManually;
        }

        public bool IsMultiplayerGame()
        {
            if (appID > 0)
                return Multiplayer;
            
            bool mp = _tagList.Contains(GameUtilities.Tags.Multiplayer) || 
                      _tagList.Contains(GameUtilities.Tags.CoOp) || 
                      _tagList.Contains(GameUtilities.Tags.MMO) || 
                      _tagList.Contains(GameUtilities.Tags.MassivelyMultiplayer) || 
                      _tagList.Contains(GameUtilities.Tags.NoTags);

            return mp;
        }
    }
}
