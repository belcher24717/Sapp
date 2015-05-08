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

        public static bool VerifyHours(string text)
        {
            try
            {
                double.Parse(text);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

    }
}
