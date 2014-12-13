using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;

namespace Sapp
{
    [Serializable()]
    public class Game : IComparable<Game>
    {
        // maybe consolidate these attributes into a GameProperties object?
        
//        private List<string> genre;
        private int appID;
        
        private bool isInstalled;
        private int lastTimePlayed;
        private bool downloadableContent;
        
        //these might be looked up at run time

        private List<GameUtilities.Tags> tagList;
//        private bool singlePlayer;
//        private bool multiplayer;
//        private bool cooperative;
        public bool IsDLC
        {
            get;
            set;
        }

        public double HoursLastTwoWeeks
        {
            get;
            set;
        }

        public double HoursPlayed
        {
            get;
            set;
        }

        private string title;
        public string Title
        {
            get { return title; }
        }

        //start with just title and either build the rest in later, or add them here
        public Game(string title, int appid, bool installed)
        {
            this.title = title;
            this.appID = appid;
            this.isInstalled = installed;

            this.HoursLastTwoWeeks = 0;
            this.HoursPlayed = 0;
            IsDLC = false;

            tagList = new List<GameUtilities.Tags>();
 //           PopulateTags(tags);
        }

        public int GetAppID()
        {
            return appID;
        }

        public override string ToString()
        {
            return this.title;
        }

        public bool IsInstalled()
        {
            return isInstalled;
        }

        public int CompareTo(Game other)
        {
            return this.ToString().CompareTo(other.ToString());
        }

        public void Launch()
        {
            Process.Start("steam://run/" + appID);
        }

        public void AddTag(string tag)
        {
            GameUtilities.Tags newTag = GameUtilities.CreateTag(tag);

            if (newTag != GameUtilities.Tags.NullTag)
                tagList.Add(newTag);
        }

        public List<GameUtilities.Tags> GetTags()
        {
            return tagList;
        }

        //This method will have 2 behaviors based on settings
        public bool ContainsTag(List<GameUtilities.Tags> tagsApplied, TagApplicationMethod method)
        {
            if (method == TagApplicationMethod.ContainsAll)
            {
                foreach (GameUtilities.Tags tag in tagsApplied)
                {
                    if (!tagList.Contains(tag))
                        return false;
                }
                return true;
            }

            else if (method == TagApplicationMethod.ContainsOne)
            {
                foreach (GameUtilities.Tags tag in tagsApplied)
                    if (tagList.Contains(tag))
                        return true;
            }

            return false;
        }

        public bool ContainsTag(GameUtilities.Tags tag) // overload
        {
            if (tagList.Contains(tag))
                return true;

            return false;
        }



    }
}
