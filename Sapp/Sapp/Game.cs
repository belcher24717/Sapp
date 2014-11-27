using System;
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
        private string title;
        private List<string> genre;
        private int appID;
        private double totalPlayTime;
        private double lastTwoWeeksPlayTime;
        private bool isInstalled;
        //these might be looked up at run time

        private int lastTimePlayed;
        private bool downloadableContent;
        private bool singlePlayer;
        private bool multiplayer;
        private bool cooperative;


        //start with just title and either build the rest in later, or add them here
        public Game(string title, int appid, bool installed)
        {
            this.title = title;
            this.appID = appid;
            this.isInstalled = installed;

            this.lastTwoWeeksPlayTime = 0;
            this.totalPlayTime = 0;

            genre = new List<string>();
            PopulateGenres();
        }

        public override string ToString()
        {
            return this.title;
        }

        public int CompareTo(Game other)
        {
            return this.ToString().CompareTo(other.ToString());
        }

        public void Launch()
        {
            Process.Start("steam://run/" + appID);
        }

        //do we need more than just the title for this?
        private void PopulateGenres()
        {
            /*
            Thread.Sleep(10);
            Random rng = new Random(DateTime.Now.Millisecond);
            int x = rng.Next();
            int i = x%3;

            if(i == 0)
                genre.Add("MMO");
            if (i == 1)
                genre.Add("RPG");
            if (i == 2)
                genre.Add("FPS");
             * */
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
            int matching = 0;

            //foreach (string s in genreList)
            if (genre.Contains(genreList))
                matching++;

            return matching == genre.Count;
        }
    }
}
