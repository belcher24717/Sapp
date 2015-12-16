using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;


namespace Sapp
{
    public class CustomGameWindowManager
    {

        public CustomGameWizard _wizard;
        private CustomizeGamesWindow _gamesWindow;
        private TabControl _wizardTab;
        private SolidColorBrush baseColor;
        private long _fileSizeInBytes;
        private Game _game;
        private GamesList _gamePool;
        private GamesList _removedPool;
        private Settings.Wizard _wizardType;
        private string _originalName;

        private const string TEXT_COLOR = "#FFCFCFCF";

        // might be able to remove Settings.Wizard enumType since game will always be passed when editing. Can keep though incase more enums are added...
        public CustomGameWindowManager(Settings.Wizard enumType, CustomizeGamesWindow gamesWindow, GamesList gamePool, GamesList removedPool, Game game = null)
        {
            _wizard = new CustomGameWizard();
            _wizardType = enumType;

            switch (_wizardType)
            {
                case Settings.Wizard.Add:
                    _wizard.label_wizardheader.Content = "CUSTOM GAME WIZARD";
                    disableIsInstalled();
                    break;
                case Settings.Wizard.Edit:
                    //TODO: This is hardcoded, make a better way of dealing with these 2 cases. Possibly a more dedicated Factory than CustomGameWindowManager that it uses to create its Wizard?
                    _wizard.label_wizardheader.Content = "EDIT GAME";
                    _wizard.checkbox_isinstalled.Visibility = System.Windows.Visibility.Visible;
                    _originalName = game.Title;
                    // if game is not custom, hide .exe location feature...
                    if (game != null && game.GetAppID() > 0)
                    {
                        // If the game was determined to be installed programmatically, do not allow the user to say the game is not installed.
                        if (game.IsInstalled == true && !game.IsInstalledManually)
                        {
                            disableIsInstalled();
                        }
                        _wizard.textbox_location.Visibility = System.Windows.Visibility.Hidden;
                        _wizard.textbox_location.IsEnabled = false;
                        _wizard.button_browse.Visibility = System.Windows.Visibility.Hidden;
                        _wizard.button_browse.IsEnabled = false;
                        _wizard.label_location.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else // game is custom
                    {
                        //Left, Top, Right, Bottom - double

                        // NOTE: Changes below are because, for now, we are not allowing them to mark a custom game as uninstalled. If they move their installation, they can
                        //       simply change the .exe file location. If they uninstall the game, they can simply remove the game. There is no need to keep track of isInstalled.

                        //System.Windows.Thickness margin = _wizard.checkbox_isinstalled.Margin;
                        //margin.Bottom -= 50;
                        //margin.Top += 50;
                        //_wizard.checkbox_isinstalled.Margin = margin;
                        disableIsInstalled();
                    }
                    _game = game;
                    _game.SetIsInstalled(game.IsInstalled, false);
                    autoPopulateInfo();
                    _wizard.SetIsInstalledHandlers();
                    break;
            }

            _gamesWindow = gamesWindow;
            _gamePool = gamePool;
            _removedPool = removedPool;
            _wizardTab = _wizard.tabcontrol_customgame;

            baseColor = new SolidColorBrush();
            baseColor.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(TEXT_COLOR);

//            if (game != null)
//            {
//                _game = game;
//                _game.SetIsInstalled(game.IsInstalled, false);
//                autoPopulateInfo();
//            }

        }

        private void disableIsInstalled()
        {
            _wizard.checkbox_isinstalled.IsEnabled = false;
            _wizard.checkbox_isinstalled.Visibility = System.Windows.Visibility.Hidden;
        }

        private void autoPopulateInfo()
        {
            _wizard.textbox_gamename.Text = _game.Title;
            _wizard.textbox_location.Text = _game.FilePath;
            _wizard.checkbox_isinstalled.IsChecked = _game.IsInstalled;

            List<CheckBox> checkboxes = _wizard.GetCheckboxList();
            List<GameUtilities.Tags> tags = _game.GetTags();

            foreach (GameUtilities.Tags tag in tags)
            {
                foreach (CheckBox cb in checkboxes)
                {
                    if (cb.Name.Equals("chkbx" + tag.ToString()))
                    {
                        cb.IsChecked = true;
                    }
                }
            }
        }

        public void Transition()
        {
            int curTab = _wizardTab.SelectedIndex;

            if (curTab == 0)
            {
                if (VerifyInitializeTab())
                {
                    Next();
                    ResetTab(curTab);
                }            
            }
            else if (curTab == 1)
            {
                Next();
            }
            else if (curTab == 2)
                FinalizeGame();          
        }

        private void Next()
        {
            int curTab = _wizardTab.SelectedIndex;

            if (curTab < _wizardTab.Items.Count)
                _wizardTab.SelectedIndex = curTab + 1;
        }

        public void Back()
        {
            int curTab = _wizardTab.SelectedIndex;

            if (curTab > 0)
                _wizardTab.SelectedIndex = curTab - 1;
        }

        private bool VerifyInitializeTab()
        {
            _wizard.label_gamename.Foreground = System.Windows.Media.Brushes.White;
            _wizard.label_location.Foreground = System.Windows.Media.Brushes.White;

            if (_wizard.textbox_gamename.Text.Equals(""))
            {
                _wizard.label_gamename.Foreground = System.Windows.Media.Brushes.Red;
                _wizard.label_error1.Content = "You must enter a name.";
                _wizard.label_error1.Visibility = System.Windows.Visibility.Visible;
                return false;
            }
            else if (Regex.Matches(_wizard.textbox_gamename.Text, @"[a-zA-Z]").Count < 1)
            {
                _wizard.label_gamename.Foreground = System.Windows.Media.Brushes.Red;
                _wizard.label_error1.Content = "Name must contain letters.";
                _wizard.label_error1.Visibility = System.Windows.Visibility.Visible;
                return false;
            }
            else if (!_wizard.textbox_gamename.Text.Equals(_originalName) && _wizard.textbox_gamename.Text.Length > 32)
            {
                _wizard.label_gamename.Foreground = System.Windows.Media.Brushes.Red;
                _wizard.label_error1.Content = "Name must be 32 characters or less.";
                _wizard.label_error1.Visibility = System.Windows.Visibility.Visible;
                return false;
            }
            else if (_game == null && MainWindow.GetAllGames().ContainsGame(_wizard.textbox_gamename.Text))
            {
                _wizard.label_gamename.Foreground = System.Windows.Media.Brushes.Red;
                _wizard.label_error1.Content = "That game name already exists.";
                _wizard.label_error1.Visibility = System.Windows.Visibility.Visible;
                return false;
            }
            else if (_wizardType != Settings.Wizard.Edit && _wizard.textbox_location.Text.Equals(""))
            {
                _wizard.label_location.Foreground = System.Windows.Media.Brushes.Red;
                _wizard.label_error1.Content = "You must choose a file.";
                _wizard.label_error1.Visibility = System.Windows.Visibility.Visible;
                return false;
            }
            else if (_wizardType != Settings.Wizard.Edit && !File.Exists(_wizard.textbox_location.Text))
            {
                _wizard.label_location.Foreground = System.Windows.Media.Brushes.Red;
                _wizard.label_error1.Content = "That file does not exist.";
                _wizard.label_error1.Visibility = System.Windows.Visibility.Visible;
                return false;
            }

            return true;
        }

        private void FinalizeGame()
        {
            Game game = null;
            string myId = Settings.GetInstance().SteamID64.ToString();
            GamesList games = GameUtilities.LoadGameList(myId, "games");

            if (_wizardType == Settings.Wizard.Add)
            {
                game = new Game(_wizard.textbox_gamename.Text, -(_wizard.getFileSize()), true); //name, id, isInstalled
                game.FilePath = _wizard.textbox_location.Text;
            }
            else if (_wizardType == Settings.Wizard.Edit)
            {
                game = _game;
                game.SetIsInstalled((bool)(_wizard.checkbox_isinstalled.IsChecked), _wizard.DidIsInstalledChange());
                game.ChangeTitle(_wizard.GetGameTitle());
                game.ClearTags();
                 
                games.Remove(_game);
                _gamePool.Remove(_game);
                _removedPool.Remove(_game);
            }
            else
            {
                Logger.LogError("<CustomGameWindowManager.FinalizeGame> Wizard had an invalid type (Niether Custom or Edit)", false);
                ExitWizard();
            }

            if (_wizard.tagsToApply.Count == 0)
            {
                game.AddTag(GameUtilities.Tags.NoTags);
            }
            else
                foreach (string tag in _wizard.tagsToApply)
                    game.AddTag(tag);

            games.Add(game);
            GameUtilities.SaveGameList(games, myId, "games");
            _gamePool.Add(game);

            if (game.GetAppID() < 0)
                _gamesWindow.AddCustomGame(game);

            ExitWizard();
        }

        private void ResetTab(int curTab)
        {
            if (curTab == 0)
            {
                _wizard.label_error1.Visibility = System.Windows.Visibility.Hidden;
                _wizard.label_gamename.Foreground = new SolidColorBrush(Colors.White);
                _wizard.label_location.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (curTab == 1)
            {
                _wizard.label_customizeheader.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        public void ExitWizard()
        {
            _wizard.Close();
        }
    }
}
