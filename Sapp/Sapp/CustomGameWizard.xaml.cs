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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sapp
{
    /// <summary>
    /// Interaction logic for CustomGameWizard.xaml
    /// </summary>
    public partial class CustomGameWizard : Window
    {

        private CustomGameWindowManager _manager;
        private List<CheckBox> checkboxes;

        public CustomGameWizard(CustomGameWindowManager manager)
        {
            InitializeComponent();

            _manager = manager;

            tabcontrol_mainview.SelectionChanged += ChangeButtonText;

            PopulateCheckboxes();
        }

        //TODO: This is so ugly, make this better.
        private void PopulateCheckboxes()
        {
            checkboxes = new List<CheckBox>();

            checkboxes.Add(chkbxAction);
            checkboxes.Add(chkbxAdventure);
            checkboxes.Add(chkbxArcade);
            checkboxes.Add(chkbxBuilding);
            checkboxes.Add(chkbxCasual);
            checkboxes.Add(chkbxComedy);
            checkboxes.Add(chkbxCompetitive);
            checkboxes.Add(chkbxCoOp);
            checkboxes.Add(chkbxDark);
            checkboxes.Add(chkbxDifficult);
            checkboxes.Add(chkbxDriving);
            checkboxes.Add(chkbxExploration);
            checkboxes.Add(chkbxFantasy);
            checkboxes.Add(chkbxFPS);
            checkboxes.Add(chkbxFreeToPlay);
            checkboxes.Add(chkbxFunny);
            checkboxes.Add(chkbxHorror);
            checkboxes.Add(chkbxIndie);
            checkboxes.Add(chkbxMMO);
            checkboxes.Add(chkbxMOBA);
            checkboxes.Add(chkbxMulti);
            checkboxes.Add(chkbxMystery);
            checkboxes.Add(chkbxOpenWorld);
            checkboxes.Add(chkbxPlatformer);
            checkboxes.Add(chkbxPuzzle);
            checkboxes.Add(chkbxRacing);
            checkboxes.Add(chkbxRelaxing);
            checkboxes.Add(chkbxRetro);
            checkboxes.Add(chkbxRoguelike);
            checkboxes.Add(chkbxRPG);
            checkboxes.Add(chkbxRTS);
            checkboxes.Add(chkbxSandbox);
            checkboxes.Add(chkbxSciFi);
            checkboxes.Add(chkbxShooter);
            checkboxes.Add(chkbxSimulation);
            checkboxes.Add(chkbxSingle);
            checkboxes.Add(chkbxSports);
            checkboxes.Add(chkbxStealth);
            checkboxes.Add(chkbxStoryRich);
            checkboxes.Add(chkbxStrategy);
            checkboxes.Add(chkbxSurvival);
            checkboxes.Add(chkbxTactical);
            checkboxes.Add(chkbxTurnBased);
        }

        private void ChangeButtonText(object sender, RoutedEventArgs e)
        {
            if (tabcontrol_mainview.SelectedIndex == 2)
                button_next.Content = "Finish";
            else
                button_next.Content = "Next >";
        }

        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            _manager.Transition();
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            _manager.Back();
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            _manager.ExitWizard();
        }

        public List<CheckBox> GetCheckboxList()
        {
            return checkboxes;
        }

    }
}
