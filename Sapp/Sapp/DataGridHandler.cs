using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Sapp
{
    public class DataGridHandler
    {
        private DataGrid theGrid;

        public DataGridHandler(ref DataGrid binding)
        {
            theGrid = binding;
            
        }

        public void Bind(GamesList games)
        {
            theGrid.ItemsSource = games;
        }

        public void Refresh()
        {
            int temp = theGrid.SelectedIndex;
            int oldCount = theGrid.Items.Count;

            if (theGrid.Items.Count != ((GamesList)theGrid.Items.SourceCollection).Count)
                ForceRefresh();

            theGrid.Items.Refresh();
            
            FixSelection(temp, oldCount);
        }

        public void ForceRefresh()
        {
            GamesList correctList = (GamesList)theGrid.Items.SourceCollection;
            theGrid.ItemsSource = null;
            theGrid.ItemsSource = correctList;
        }

        public void AddColumn(string colName)
        {
            DataGridTextColumn c1 = new DataGridTextColumn();
            c1.Header = AddSpaces(colName);
            c1.Binding = new Binding(colName.Replace(" ", String.Empty));
            theGrid.Columns.Add(c1);
        }

        public void RemoveColumn(string colName)
        {
            foreach (DataGridColumn dgc in theGrid.Columns)
            {
                if (dgc.Header.Equals(AddSpaces(colName)))
                {
                    theGrid.Columns.Remove(dgc);
                    break;
                }
            }
        }

        public Game GetSelectedItem()
        {
            return (Game)theGrid.SelectedItem;
        }

        private void FixSelection(int prevIndex, int oldCount)
        {
            //no more items
            if (theGrid.Items.Count <= 0)
                return;

                //TODO: Test this with adding/removing filters
            else if (prevIndex >= theGrid.Items.Count)
                theGrid.SelectedIndex = theGrid.Items.Count - 1;
            else
                theGrid.SelectedIndex = prevIndex;

            //focus if it was the grid removed from
            //if (theGrid.Items.Count < oldCount)
                theGrid.Focus();
        }

        private string AddSpaces(string s)
        {
            string toWorkOn = s.Replace(" ", String.Empty); ;

            for (int i = 1; i < toWorkOn.Length; i++)
            {
                char c = toWorkOn[i];

                if (c < 91)
                {
                    toWorkOn = toWorkOn.Substring(0, i) + " " + toWorkOn.Substring(i, toWorkOn.Length - i);
                    i++;
                }
            }
            return toWorkOn;
        }

        public void ClearColumns()
        {
            for(int i = 0; i < theGrid.Columns.Count; i++)
            {
                if (!theGrid.Columns[i].Header.Equals("Title"))
                {
                    theGrid.Columns.Remove(theGrid.Columns[i]);
                    i--;
                }
            }
        }
    }
}
