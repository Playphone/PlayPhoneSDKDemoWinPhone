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
using PlayPhone.MultiNet.Core;

namespace WP7SDKDemo.views
{
    public partial class Leaderboards : PhoneApplicationPage
    {
        public Leaderboards()
        {
            InitializeComponent();
        }

        private void showSimple(object sender, RoutedEventArgs e)
        {
            MNDebug.debug("Leaderboards showSimple");
            NavigationService.Navigate( new Uri("/miniview/leaderboards/SimpleLeaderboard.xaml", UriKind.RelativeOrAbsolute) );
        }

        private void showGame(object sender, RoutedEventArgs e)
        {
            MNDebug.debug("Leaderboards showGame");
            NavigationService.Navigate(new Uri("/miniview/leaderboards/GameLeaderboard.xaml", UriKind.RelativeOrAbsolute));
        }

        private void showUser(object sender, RoutedEventArgs e)
        {
            MNDebug.debug("Leaderboards showUser");
            NavigationService.Navigate(new Uri("/miniview/leaderboards/UserLeaderboard.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}