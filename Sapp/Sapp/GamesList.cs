using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sapp
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    public class GamesList : List<Game>
    {
        public event ChangedEventHandler Changed;

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
            OnChanged(EventArgs.Empty);
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

    }

}
