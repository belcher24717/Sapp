using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace Sapp
{
    public class CustomGameWindowManager
    {
        // not sure if i need to keep track of currentTab at this point
        private TabItem currentTab;
        private TabControl wizardTab;

        public CustomGameWindowManager(TabControl wizardTab, TabItem startingTab)
        {
            this.wizardTab = wizardTab;
            this.currentTab = startingTab;
            this.wizardTab.SelectedItem = this.currentTab;
        }

        public void SwitchTab(TabItem nextTab)
        {
            this.currentTab = nextTab;
            this.wizardTab.SelectedItem = this.currentTab;
        }

        // not sure if i need this functionality at this point
        public TabItem GetCurrentTab()
        {
            return this.currentTab;
        }

    }
}
