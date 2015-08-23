using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sapp
{

    enum Labels
    {
        Pre,
        Post
    }

    public class HoursHandler
    {

        private CheckBox isHandlerEnabled;
        private Label[] labelsAssociated;
        private ComboBox greaterOrLessThanOption;
        private TextBox txtNumberEntered;
        private double lastValidNumber;


        public HoursHandler(ref CheckBox cbRelated, ref Label lblPre, ref Label lblPost, ref ComboBox gtOrltOption, ref TextBox userData)
        {
            this.isHandlerEnabled = cbRelated;
            this.labelsAssociated = new Label[] { lblPre, lblPost };
            this.greaterOrLessThanOption = gtOrltOption;
            this.txtNumberEntered = userData;
            this.lastValidNumber = double.Parse(userData.Text);//This should be default 30 from the textbox on creation
        }

        public void Update()
        {
            if ((bool)isHandlerEnabled.IsChecked)
            {
                labelsAssociated[(int)Labels.Pre].Visibility = System.Windows.Visibility.Visible;
                labelsAssociated[(int)Labels.Post].Visibility = System.Windows.Visibility.Visible;
                greaterOrLessThanOption.Visibility = System.Windows.Visibility.Visible;
                txtNumberEntered.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                labelsAssociated[(int)Labels.Pre].Visibility = System.Windows.Visibility.Hidden;
                labelsAssociated[(int)Labels.Post].Visibility = System.Windows.Visibility.Hidden;
                greaterOrLessThanOption.Visibility = System.Windows.Visibility.Hidden;
                txtNumberEntered.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public void Verify()
        {
            //This will make sure values typed in the textbox wont blow up
            if (txtNumberEntered.Text.Equals(""))
            {
                lastValidNumber = 0;
                return;
            }

            try
            {
                double test = double.Parse(txtNumberEntered.Text);
                lastValidNumber = test;
            }
            catch
            {
                txtNumberEntered.Text = "" + lastValidNumber;
                txtNumberEntered.Select(txtNumberEntered.Text.Length, 0);
            }
        }

        public double GetHours()
        {
            return lastValidNumber;
        }

    }
}
