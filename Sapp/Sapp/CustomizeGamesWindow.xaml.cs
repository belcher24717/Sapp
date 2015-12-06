using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sapp
{
    /// <summary>
    /// Interaction logic for AddEditGamesWindow.xaml
    /// </summary>
    public partial class CustomizeGamesWindow : Window
    {
        private const string FILTER_TEXT = "Text Filter...";
        bool customTextFilterActive;
        bool editTextFilterActive;

        private List<Game> games;
        private List<Game> _customGames = new List<Game>();
        private List<Game> _editGames = new List<Game>();

        public CustomizeGamesWindow()
        {
            InitializeComponent();
            games = MainWindow.GetAllGames();
            PopulateCustomGamesList();
            PopulateEditGamesList();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_addcustomgame_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_editcustomgame_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_removecustomgame_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_editgame_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MouseDownOnWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void txtFilterSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            UpdateTextFilterSettings(sender, false);
        }

        private void txtFilterSearch_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (String.IsNullOrEmpty(tb.Text))
            {
                UpdateTextFilterSettings(sender, true);
            }
        }

        private void UpdateTextFilterSettings(object sender, bool lostFocus)
        {
            TextBox tb = (TextBox)sender;
            if (lostFocus)
            {
                //disable TextChanged event
                tb.TextChanged -= txtSearchFilter_TextChanged;

                tb.Text = FILTER_TEXT;
                tb.Foreground = Brushes.Gray;
                if (tb.Name.ToLower().Contains("custom"))
                {
                    customTextFilterActive = false;
                }
                else if (tb.Name.ToLower().Contains("edit"))
                {
                    editTextFilterActive = false;
                }
            }
            else
            {
                if (tb.Text.Equals(FILTER_TEXT))
                {
                    if ((tb.Name.ToLower().Contains("custom") && !customTextFilterActive) || (tb.Name.ToLower().Contains("edit") && !editTextFilterActive))
                    {
                        tb.Text = "";
                        tb.Foreground = Brushes.White;

                        //enable TextChanged event
                        tb.TextChanged += txtSearchFilter_TextChanged;
                    }
                }
            }

        }

        private void txtSearchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Name.ToLower().Contains("custom"))
            {
                customTextFilterActive = true;
            }
            else if (tb.Name.ToLower().Contains("edit"))
            {
                editTextFilterActive = true;
            }

            UpdateList(tb);

            if (tb.Focusable)
                tb.Focus();
        }

        private void UpdateList(TextBox tb)
        {

            if (tb.Name.ToLower().Contains("custom") && !tb.Text.Equals(FILTER_TEXT))
            {

                IterateListItems(tb, new List<Game>(_customGames), listbox_customgames);
            }
            else if (tb.Name.ToLower().Contains("edit") && !tb.Text.Equals(FILTER_TEXT))
            {
                IterateListItems(tb, new List<Game>(_editGames), listbox_editgames);
            }
        }

        private void IterateListItems(TextBox tb, List<Game> list, ListBox listView)
        {
            List<Game> trimmedList = new List<Game>(list);
            foreach (Game game in list)
            {
                if (!game.Title.ToLower().Contains(tb.Text.ToLower()))
                {
                    trimmedList.Remove(game);
                }
            }

            listView.Items.Clear();
            foreach (Game game in trimmedList)
            {
                listView.Items.Add(game.ToString());
            }
        }

        private void PopulateEditGamesList()
        {

            if (games == null)
                return;

            _editGames = games;

            // if there ARE games...
            if (_editGames.Capacity > 0)
            {
                foreach (Game game in games)
                {
                    if (!listbox_editgames.Items.Contains(game.ToString()))
                    {
                        listbox_editgames.Items.Add(game.ToString());
                    }
                }
            }
        }

        private void PopulateCustomGamesList()
        {

            if (games == null)
                return;

            foreach (Game game in games)
            {
                if (game.GetAppID() < 0)
                {
                    if (!_customGames.Contains(game))
                    {
                        _customGames.Add(game);
                    }
                }
            }

            // if there ARE custom games...
            if (_customGames.Capacity > 0)
            {
                foreach (Game game in _customGames)
                {
                    if (!listbox_customgames.Items.Contains(game.ToString()))
                    {
                        listbox_customgames.Items.Add(game.ToString());
                    }
                }
            }

        }

        public void AddCustomGame(Game game)
        {
            if (!_customGames.Contains(game))
            {
                _customGames.Add(game);
                listbox_customgames.Items.Add(game.ToString());
            }
        }

        public void RemoveCustomGame(Game game)
        {
            if (_customGames.Contains(game))
            {
                _customGames.Remove(game);
                listbox_customgames.Items.Remove(game.ToString());
            }
        }
    }
}
