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

        public CustomGameWizard()
        {
            InitializeComponent();

            tabcontrol_mainview.SelectionChanged += TabChangedEvent;

            HideTabHeaders();
            PopulateCheckboxes();

            // maybe move this to onload event?
            _manager = new CustomGameWindowManager(this);
        }

        private void HideTabHeaders()
        {
            Style newStyle = new Style();
            newStyle.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
            tabcontrol_mainview.ItemContainerStyle = newStyle;
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

        private void TabChangedEvent(object sender, RoutedEventArgs e)
        {
            // reset
            button_back.IsEnabled = true;
            button_next.Content = "Next >";

            // special cases
            if (tabcontrol_mainview.SelectedIndex == 0)
                button_back.IsEnabled = false;
            else if (tabcontrol_mainview.SelectedIndex == 2)
            {
                label_finalnamedisplay.Content = textbox_gamename.Text;
                //TODO: Make it only show the .exe file, not the absolute path...
                label_finalexeselected.Content = textbox_location.Text;
                button_next.Content = "Finish";
            }

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

        //TODO: Make sure this grabs an exe, not just a valid path...
        private void button_browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textbox_location.Text = dialog.SelectedPath;
        }

    }
}
