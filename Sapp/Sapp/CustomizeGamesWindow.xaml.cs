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
        bool textFilterActive;

        private GamesList _gamePool;
        private GamesList _removedPool;

        private List<Game> games;
        private List<Game> _customGames = new List<Game>();
        private List<Game> _editGames = new List<Game>();

        public CustomizeGamesWindow(GamesList gamePool, GamesList removedPool)
        {
            InitializeComponent();
            games = MainWindow.GetAllGames();
            PopulateCustomGamesList();
            PopulateEditGamesList();

            SetDefaultFilterText(textbox_customsearchfilter);
            SetDefaultFilterText(textbox_editsearchfilter);

            _gamePool = gamePool;
            _removedPool = removedPool;
        }

        private void tcSettingsTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tits = e.OriginalSource as TabControl;
            TabItem tabSelected = tits == null ? null : tits.SelectedItem as TabItem;
            if(tabSelected != null && tabSelected.Focusable)
                tabSelected.Focus();
        }

        private void SetDefaultFilterText(TextBox tb)
        {
            tb.Text = FILTER_TEXT;
            tb.Foreground = Brushes.Gray;
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_addcustomgame_Click(object sender, RoutedEventArgs e)
        {
            CustomGameWindowManager manager = new CustomGameWindowManager(Settings.Wizard.Add, this, _gamePool, _removedPool);
            manager._wizard.SetManager(manager);
            manager._wizard.ShowDialog();
        }

        private void button_editcustomgame_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_customgames.SelectedIndex < 0)
            {
                DisplayMessage dm = new DisplayMessage("No Game Selected...", "You must select a Custom Game first to edit.", System.Windows.Forms.MessageBoxButtons.OK);
                dm.ShowDialog();
            }
            else
            {
                int index = listbox_customgames.SelectedIndex;
                CustomGameWindowManager manager = new CustomGameWindowManager(Settings.Wizard.Edit, this, _gamePool, _removedPool, _customGames[index]);
                manager._wizard.SetManager(manager);
                manager._wizard.ShowDialog();
            }
        }

        private void button_removecustomgame_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_customgames.SelectedIndex < 0)
            {
                DisplayMessage dm = new DisplayMessage("No Game Selected...", "You must select a Custom Game first to remove.", System.Windows.Forms.MessageBoxButtons.OK);
                dm.ShowDialog();
            }
            else
            {
                DisplayMessage dm = new DisplayMessage("Remove Custom Game...",
                                                       "Are you sure you want to remove " + listbox_customgames.SelectedItem.ToString() + "?",
                                                       System.Windows.Forms.MessageBoxButtons.YesNo);
                if ((bool)(dm.ShowDialog()))
                {
                    int index = listbox_customgames.SelectedIndex;
                    Game gameToRemove = _customGames[index];

                    _gamePool.Remove(gameToRemove);
                    _removedPool.Remove(gameToRemove);

                    //Remove game from persistence...
                    string myId = Settings.GetInstance().SteamID64.ToString();
                    GamesList games = GameUtilities.LoadGameList(myId, "games");
                    games.Remove(gameToRemove);
                    GameUtilities.SaveGameList(games, myId, "games");

                    RemoveCustomGame(gameToRemove);
                }
            }
        }

        private void button_editgame_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_editgames.SelectedIndex < 0)
            {
                DisplayMessage dm = new DisplayMessage("No Game Selected...", "You must select a game first to edit.", System.Windows.Forms.MessageBoxButtons.OK);
                dm.ShowDialog();
            }
            else
            {
                //int index = listbox_editgames.SelectedIndex;
                int index;
                for (index = 0; index < _editGames.Count; index++)
                {
                    if (_editGames[index].Title.Equals(listbox_editgames.SelectedItem.ToString()))
                        break;
                }

                if (index == _editGames.Count)
                {
                    Logger.LogError("<CustomGameWizard.button_editgame_Click> Could not find the game to edit using name to name comparison between _editGames[index] and listbox.SelectedItem", false);
                    return;
                }
                
                CustomGameWindowManager manager = new CustomGameWindowManager(Settings.Wizard.Edit, this, _gamePool, _removedPool, games[index]);
                manager._wizard.SetManager(manager);
                manager._wizard.ShowDialog();
            }
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

                SetDefaultFilterText(tb);
                textFilterActive = false;
            }
            else
            {
                if (tb.Text.Equals(FILTER_TEXT))
                {
                    if (!textFilterActive)
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

            textFilterActive = true;

            UpdateList(tb);

            if (tb.Focusable)
                tb.Focus();
        }

        private void UpdateList(TextBox tb)
        {
            if (tb.Name.ToLower().Contains("custom") && !tb.Text.Equals(FILTER_TEXT))
            {
                IterateListItems(tb.Text, new List<Game>(_customGames), listbox_customgames);
            }
            else if (tb.Name.ToLower().Contains("edit") && !tb.Text.Equals(FILTER_TEXT))
            {
                IterateListItems(tb.Text, new List<Game>(_editGames), listbox_editgames);
            }
        }

        private void IterateListItems(string text, List<Game> list, ListBox listView)
        {
            List<Game> trimmedList = new List<Game>(list);
            foreach (Game game in list)
            {
                if (!game.Title.ToLower().Contains(text.ToLower()))
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

            if (!_editGames.Contains(game))
            {
                _editGames.Add(game);
                listbox_editgames.Items.Add(game.ToString());
            }
        }

        public void RemoveCustomGame(Game game)
        {
            _customGames.Remove(game);
            _editGames.Remove(game);
            listbox_customgames.Items.Remove(game.ToString());
            listbox_editgames.Items.Remove(game.ToString());
        }

    }
}
