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
        public string FileName;

        public FilenameTypein()
        {
            InitializeComponent();
            FileName = null;
            txtFileName.Focus();
        }

        private void btnSaveClicked(object sender, RoutedEventArgs e)
        {
            if (txtFileName.Text.Length > 36)
            {
                lblWarning.Content = "Only 36 characters allowed.";
                return;
            }

            //char[] splitValues = new char[System.IO.Path.GetInvalidFileNameChars().Length + System.IO.Path.GetInvalidPathChars().Length];
            List<char> splitValues = new List<char>();
            splitValues.AddRange(System.IO.Path.GetInvalidFileNameChars());
            splitValues.AddRange(System.IO.Path.GetInvalidPathChars());
            splitValues.AddRange(new char[] { ',', '.', '`', ';'});


            string[] fileName = txtFileName.Text.Split(splitValues.ToArray());

            if (fileName.Length > 1)
            {
                lblWarning.Content = "Invalid characters.";
                return;
            }
            else
            {
                FileName = txtFileName.Text;
            }

            DialogResult = true;
            this.Close(); 
        }

        private void btnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close(); 
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
