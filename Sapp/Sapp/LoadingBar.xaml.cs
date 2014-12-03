using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Sapp
{
    /// <summary>
    /// Interaction logic for LoadingBar.xaml
    /// </summary>
    public partial class LoadingBar : Window
    {
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private UpdateProgressBarDelegate updatePbDelegate;

        public LoadingBar(int numGames, string message)
        {
            InitializeComponent();

            lblMessage.Content = message;
            pbGamesLoaded.Maximum = numGames;
            updatePbDelegate = new UpdateProgressBarDelegate(pbGamesLoaded.SetValue);

        }

        public void Progress()
        {
            lock (pbGamesLoaded)
            {
                if (pbGamesLoaded.Value < pbGamesLoaded.Maximum)
                    pbGamesLoaded.Value++;

                Dispatcher.Invoke(updatePbDelegate,
                    System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { ProgressBar.ValueProperty, pbGamesLoaded.Value });
            }
            ShouldClose();
        }

        private void ShouldClose()
        {
            if (pbGamesLoaded.Maximum == pbGamesLoaded.Value)
                this.Close();
        }
    }
}
