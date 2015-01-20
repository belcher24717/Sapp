using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sapp
{
    public class VerificationClass
    {

        public static bool VerifyHours(TextBox control, ref double hours)
        {
            try
            {
                hours = double.Parse(control.Text);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

    }
}
