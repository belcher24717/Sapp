using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace Sapp
{
    public class CustomGameWindowManager
    {

        private CustomGameWizard _wizard;
        private TabControl _wizardTab;

        public CustomGameWindowManager(CustomGameWizard wizard)
        {
            _wizard = wizard;
            _wizardTab = _wizard.tabcontrol_mainview;
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
                return false;

            if (!Directory.Exists(_wizard.textbox_location.Text))
                return false;

            return true;
        }

        private bool VerifyTagsTab()
        {
            bool valid = false;
            List<CheckBox> cbList = _wizard.GetCheckboxList();

            foreach (CheckBox cb in cbList)
                if ((bool)(cb.IsChecked))
                    valid = true;

            return valid;
        }

        private void FinalizeGame()
        {
            //TODO: Figure out how we're going to assign an id to custom games.
            Game customGame = new Game(_wizard.textbox_gamename.Text, -1, true); //name, id, isInstalled
            MainWindow.AddGame(customGame);

            ExitWizard();
        }


        public void ExitWizard()
        {
            _wizard.Close();
        }
    }
}
