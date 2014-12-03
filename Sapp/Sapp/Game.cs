﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;

namespace Sapp
{
    public class Game : IComparable<Game>
    {
        // maybe consolidate these attributes into a GameProperties object?
        private string title;
//        private List<string> genre;
        private int appID;
        private double totalPlayTime;
        private double lastTwoWeeksPlayTime;
        private bool isInstalled;
        private int lastTimePlayed;
        private bool downloadableContent;
        //these might be looked up at run time

        private List<GameUtilities.Tags> tagList;
//        private bool singlePlayer;
//        private bool multiplayer;
//        private bool cooperative;


        //start with just title and either build the rest in later, or add them here
        public Game(string title, int appid, bool installed)
        {
            this.title = title;
            this.appID = appid;
            this.isInstalled = installed;

            this.lastTwoWeeksPlayTime = 0;
            this.totalPlayTime = 0;

            tagList = new List<GameUtilities.Tags>();
 //           PopulateTags(tags);
        }

        public int GetAppID()
        {
            return appID;
        }

        public double GetHoursPlayed()
        {
            return totalPlayTime;
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

        /*
        private void PopulateTags(List<string> tags)
        {
            foreach (string tag in tags)
            {
                // seperated this logic for ease of reading and in case we add extra logic
                CreateTag(tag);
            }
        }
        */

        public void AddTag(string tag)
        {
            GameUtilities.Tags newTag = GameUtilities.CreateTag(tag);

            if (newTag != GameUtilities.Tags.NullTag)
                tagList.Add(GameUtilities.CreateTag(tag));
        }

        public void AddGameTime(string gameTime)
        {
            double timeTest = 0.0;

            try
            {
                timeTest = double.Parse(gameTime);
            }
            catch
            {

            }

            if (totalPlayTime == 0)
                totalPlayTime = timeTest;
            else
            {
                lastTwoWeeksPlayTime = totalPlayTime;
                totalPlayTime = timeTest;
            }
        }


        //This method will have 2 behaviors based on settings
        public bool IsGenres(string genreList)
        {
 //           int matching = 0;

 //           foreach (string s in genreList)
 //           if (tagList.Contains(genreList))
 //               matching++;

 //           return matching == genre.Count;
            return false;
        }

    }
}
