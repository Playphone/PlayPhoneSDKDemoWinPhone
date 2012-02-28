using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace WP7SDKDemo.views
{
    public partial class VEconomy : PhoneApplicationPage
    {
        public VEconomy()
        {
            InitializeComponent();
        }

        private void openPPStore(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/miniview/veconomy/VShop.xaml", UriKind.RelativeOrAbsolute));
        }

        private void openVirtualItems(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/miniview/veconomy/VirtualItems.xaml", UriKind.RelativeOrAbsolute));
        }

        private void openUserItems(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/miniview/veconomy/UserItems.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}