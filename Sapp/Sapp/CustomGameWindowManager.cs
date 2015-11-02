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
        private SettingsScreen _settings;
        private TabControl _wizardTab;
        private SolidColorBrush baseColor;
        private long _fileSizeInBytes;
        private Game _game;
        private GamesList _gamePool;
        private GamesList _removedPool;

        private const string TEXT_COLOR = "#FFCFCFCF";

        public CustomGameWindowManager(SettingsScreen settings, GamesList gamePool, GamesList removedPool, Game game = null)
        {
            _wizard = new CustomGameWizard();
            _settings = settings;
            _gamePool = gamePool;
            _removedPool = removedPool;
            _wizardTab = _wizard.tabcontrol_customgame;

            baseColor = new SolidColorBrush();
            baseColor.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(TEXT_COLOR);

            if (game != null)
            {
                _game = game;
                autoPopulateInfo();
            }
        }

        private void autoPopulateInfo()
        {
            _wizard.textbox_gamename.Text = _game.Title;
            _wizard.textbox_location.Text = _game.FilePath;

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
                if (VerifyTagsTab())
                {
                    Next();
                    ResetTab(curTab);
                }
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
            else if (_wizard.textbox_gamename.Text.Length > 32)
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
            else if (_wizard.textbox_location.Text.Equals(""))
            {
                _wizard.label_location.Foreground = System.Windows.Media.Brushes.Red;
                _wizard.label_error1.Content = "You must choose a file.";
                _wizard.label_error1.Visibility = System.Windows.Visibility.Visible;
                return false;
            }
            else if (!File.Exists(_wizard.textbox_location.Text))
            {
                _wizard.label_location.Foreground = System.Windows.Media.Brushes.Red;
                _wizard.label_error1.Content = "That file does not exist.";
                _wizard.label_error1.Visibility = System.Windows.Visibility.Visible;
                return false;
            }

            return true;
        }

        private bool VerifyTagsTab()
        {
            bool valid = false;
            List<CheckBox> cbList = _wizard.GetCheckboxList();

            foreach (CheckBox cb in cbList)
            {
                if ((bool)(cb.IsChecked))
                {
                    valid = true;
                    break;
                }
            }

            if (!valid)
                _wizard.label_customizeheader.Foreground = System.Windows.Media.Brushes.Red;
            else
                _wizard.label_customizeheader.Foreground = baseColor;

            return valid;
        }

        private void FinalizeGame()
        {
            Game customGame = new Game(_wizard.textbox_gamename.Text, -(_wizard.getFileSize()), true); //name, id, isInstalled

            customGame.FilePath = _wizard.textbox_location.Text;
            foreach (string tag in _wizard.tagsToApply)
                customGame.AddTag(tag);

            string myId = Settings.GetInstance().SteamID64.ToString();
            GamesList games = GameUtilities.LoadGameList(myId, "games");

            if (_game != null)
            {
                games.Remove(_game);
                _gamePool.Remove(_game);
                _removedPool.Remove(_game);
            }

            games.Add(customGame);
            GameUtilities.SaveGameList(games, myId, "games");
            _gamePool.Add(customGame);
            _settings.addCustomGame(customGame);

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
