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

        private CustomGameWizard _wizard;
        private TabControl _wizardTab;
        private SolidColorBrush baseColor;

        private const string TEXT_COLOR = "#FFCFCFCF";

        public CustomGameWindowManager(CustomGameWizard wizard)
        {
            _wizard = wizard;
            _wizardTab = _wizard.tabcontrol_customgame;

            baseColor = new SolidColorBrush();
            baseColor.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(TEXT_COLOR);

            _wizard.Show();
        }

        public void Transition()
        {
            int curTab = _wizardTab.SelectedIndex;

            if (curTab == 0)
            {
                if (VerifyInitializeTab())
                    Next();
                //else
                    //show error message window
                    
            }
            else if (curTab == 1)
            {
                if (VerifyTagsTab())
                    Next();
                //else
                    //show error message window
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
            if (_wizard.textbox_gamename.Text.Equals("") || Regex.Matches(_wizard.textbox_gamename.Text, @"[a-zA-Z]").Count < 1)
            {
                _wizard.label_gamename.Foreground = System.Windows.Media.Brushes.Red;
                return false;
            }
            _wizard.label_gamename.Foreground = baseColor;

            if (!File.Exists(_wizard.textbox_location.Text))
            {
                _wizard.label_location.Foreground = System.Windows.Media.Brushes.Red;
                return false;
            }
            _wizard.label_location.Foreground = baseColor;

            return true;
        }

        private bool VerifyTagsTab()
        {
            bool valid = false;
            List<CheckBox> cbList = _wizard.GetCheckboxList();

            foreach (CheckBox cb in cbList)
                if ((bool)(cb.IsChecked))
                    valid = true;

            if (!valid)
                _wizard.label_customizeheader.Foreground = System.Windows.Media.Brushes.Red;
            else
                _wizard.label_customizeheader.Foreground = baseColor;

            return valid;
        }

        private void FinalizeGame()
        {
            //TODO: Figure out how we're going to assign an id to custom games.
            Game customGame = new Game(_wizard.textbox_gamename.Text, -1, true); //name, id, isInstalled

            customGame.FilePath = _wizard.textbox_location.Text;
            foreach (string tag in _wizard.tagsToApply)
                customGame.AddTag(tag);

            MainWindow.AddGame(customGame);

            ExitWizard();
        }


        public void ExitWizard()
        {
            _wizard.Close();
        }
    }
}
