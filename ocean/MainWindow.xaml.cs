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
using MahApps.Metro.Controls;

namespace ocean
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: MetroWindow
    {
        Frame frame1 = new Frame() { Content=new UI.Settings()};
        Frame frame2 = new Frame() { Content=new UI.controlpage()};

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeployCupCakes(object sender, RoutedEventArgs e)
        {

        }

        private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            this.HamburgerMenuControl.Content = e.InvokedItem;

            //if (!e.IsItemOptions && this.HamburgerMenuControl.IsPaneOpen)
            if (!e.IsItemOptions )
            {
                switch(HamburgerMenuControl.SelectedIndex)
                {
                    case 0:
                        
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    default:
                        break;
                }

            }

            if(e.IsItemOptions)
            {
                MessageBox.Show("版本0.0");
            }


        }

        private void GoBack_OnClick(object sender, RoutedEventArgs e)
        {
            //this.navigationServiceEx.GoBack();
        }

    }
}
