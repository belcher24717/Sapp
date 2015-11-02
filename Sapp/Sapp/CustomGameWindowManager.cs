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
        private TabControl _wizardTab;
        private SolidColorBrush baseColor;
        private long _fileSizeInBytes;

        private const string TEXT_COLOR = "#FFCFCFCF";

        public CustomGameWindowManager(/*CustomGameWizard wizard*/)
        {
            //_wizard = wizard;
            _wizard = new CustomGameWizard();
            _wizardTab = _wizard.tabcontrol_customgame;

            baseColor = new SolidColorBrush();
            baseColor.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(TEXT_COLOR);

            //_wizard.Show();
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
            else if (MainWindow.GetAllGames().ContainsGame(_wizard.textbox_gamename.Text))
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

            MainWindow.AddGame(customGame);
            //TODO: write this game out to file

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
