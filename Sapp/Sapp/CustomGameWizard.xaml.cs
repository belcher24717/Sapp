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
    public partial class CustomGameWizard : Page
    {
        public CustomGameWizard()
        {
            InitializeComponent();

            tabcontrol_mainview.SelectionChanged += ChangeButtonText;
        }

        private void ChangeButtonText(object sender, RoutedEventArgs e)
        {
            if (tabcontrol_mainview.SelectedIndex == 2)
                button_next.Content = "Finish";
            else
                button_next.Content = "Next >";
        }
    }
}
