using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sapp
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    [Serializable()]
    public class GamesList : List<Game>
    {
        public event ChangedEventHandler Changed;
        //TODO: Consider only doing a pull once per day
        //Issue: if they use the application and then buy a game, and want it in the pool
        

        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        public void Add(Game value)
        {
            base.Add(value);
            OnChanged(EventArgs.Empty);
        }

        public void Remove(Game value)
        {
            base.Remove(value);
            OnChanged(EventArgs.Empty);
        }

        public void RemoveAt(int index)
        {
            base.RemoveAt(index);
            OnChanged(EventArgs.Empty);
        }

        public void Clear()
        {
            base.Clear();
            OnChanged(EventArgs.Empty);
        }

        public void Sort()
        {
            base.Sort();
            //OnChanged(EventArgs.Empty);
        }

        public void Reverse()
        {
            base.Reverse();
            OnChanged(EventArgs.Empty);
        }

        public new Game this[int index]
        {
            get
            {
                return (Game)base[index];
            }
            set
            {
                base[index] = value;
                OnChanged(EventArgs.Empty);
            }
        }

        public Game GetGame(Int64 appID)
        {
            foreach (Game g in this)
            {
                if (g.GetAppID() == appID)
                    return g;
            }
            return null;
        }

        public void RemoveNoNotify(Game value)
        {
            base.Remove(value);
        }

        public bool ContainsId(int appID)
        {
            foreach (Game g in this)
            {
                if (g.GetAppID() == appID)
                    return true;
            }
            return false;
        }

        public void RemoveList(List<Game> games)
        {
            foreach (Game game in games)
                base.Remove(game);

            OnChanged(EventArgs.Empty);
        }

        public void AddList(List<Game> games)
        {
            foreach (Game game in games)
            {
                if(!base.Contains(game))
                    base.Add(game);
            }

            OnChanged(EventArgs.Empty);
        }

        public void SetList(List<Game> games)
        {
            base.Clear();
            foreach(Game g in games)
                base.Add(g);

            OnChanged(EventArgs.Empty);
        }

        public List<Int64> ReturnFailedDlcCheckList()
        {
            List<Int64> failedDLCs = new List<Int64>();

            foreach (Game g in this)
            {
                if (g.DlcCheckFailed)
                    failedDLCs.Add(g.GetAppID());
            }

            return failedDLCs;
        }

        public List<Int64> ReturnFailedTaggingList()
        {
            List<Int64> failedTags = new List<Int64>();

            foreach (Game g in this)
            {
                if (g.TaggingFailed)
                    failedTags.Add(g.GetAppID());
            }

            return failedTags;
        } 

    }

}
