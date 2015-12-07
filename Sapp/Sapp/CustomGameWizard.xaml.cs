using System;
using System.Collections.Generic;
using System.IO;
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
        public List<string> tagsToApply;
        private Int64 _fileSizeInBytes;

        public CustomGameWizard()
        {
            InitializeComponent();

            tabcontrol_customgame.SelectionChanged += TabChangedEvent;

            HideTabHeaders();
            PopulateCheckboxes();

            tagsToApply = new List<string>();

            CenterWindowOnScreen();
        }

        //CREDIT: http://stackoverflow.com/questions/4019831/how-do-you-center-your-main-window-in-wpf
        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void HideTabHeaders()
        {
            Style newStyle = new Style();
            newStyle.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
            tabcontrol_customgame.ItemContainerStyle = newStyle;
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

        private void MouseDownOnWindow(object sender, RoutedEventArgs e)
        {
            this.DragMove();
        }

        private void TabChangedEvent(object sender, RoutedEventArgs e)
        {
            // reset
            button_back.IsEnabled = true;
            button_Next.Content = "Next >";

            // special cases
            if (tabcontrol_customgame.SelectedIndex == 0)
                button_back.IsEnabled = false;
            else if (tabcontrol_customgame.SelectedIndex == 2)
            {
                label_finalnamedisplay.Content = textbox_gamename.Text;
                int index = textbox_location.Text.LastIndexOf('\\');
                label_finalexeselecteddisplay.Content = textbox_location.Text.Substring(index + 1);

                textbox_tags.Items.Clear();
                if (tagsToApply.Count == 0)
                {
                    ListBoxItem tag = new ListBoxItem();
                    tag.Content = "No Tags";
                    tag.Focusable = false;
                    textbox_tags.Items.Add(tag);
                }
                else
                {
                    foreach (string str in tagsToApply)
                    {
                        ListBoxItem tag = new ListBoxItem();
                        tag.Content = str;
                        tag.Focusable = false;
                        textbox_tags.Items.Add(tag);
                    }
                }
                button_Next.Content = "Finish";
            }

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

        public long getFileSize()
        {
            return _fileSizeInBytes;
        }

        //TODO: Make sure this grabs an exe, not just a valid path...
        private void button_browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.DefaultExt = ".exe";
            dialog.Filter = "EXE files (*.exe)|*.exe";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textbox_location.Text = dialog.FileName;
                _fileSizeInBytes = new FileInfo(dialog.FileName).Length;

                if (textbox_gamename.Text.Equals(""))
                {
                    string filename = dialog.SafeFileName;
                    int index = filename.IndexOf(".");
                    textbox_gamename.Text = filename.Substring(0, index);
                }
            }
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox theSender = (CheckBox)sender;
            tagsToApply.Add(theSender.Content.ToString());
        }

        private void Checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox theSender = (CheckBox)sender;
            tagsToApply.Remove(theSender.Content.ToString());
        }

        public void SetManager(CustomGameWindowManager manager)
        {
            _manager = manager;
        }

        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            _manager.Transition();
        }

    }
}
