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
using System.Windows.Shapes;

namespace Sapp
{
    /// <summary>
    /// Interaction logic for FilenameTypein.xaml
    /// </summary>
    public partial class FilenameTypein : Window
    {
        public FilenameTypein()
        {
            InitializeComponent();
        }

        private void btnSaveClicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelClicked(object sender, RoutedEventArgs e)
        {

        }

        private void VerifyFileName()
        {

        }

        private void MouseDownOnWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
