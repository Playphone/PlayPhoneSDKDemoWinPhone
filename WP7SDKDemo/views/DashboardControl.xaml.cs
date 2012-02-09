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
    using PlayPhone.MultiNet;

    public partial class DashboardControl : PhoneApplicationPage
    {
        public DashboardControl()
        {
            InitializeComponent();
        }

        private void gotoLoginScreen(object sender, RoutedEventArgs e)
        {
            MNDirect.ExecAppCommand("jumpToUserLogin", null);
            MNDirectUIHelper.ShowDashboard();
        }

        private void gotoLeaderboards(object sender, RoutedEventArgs e)
        {
            MNDirect.ExecAppCommand("jumpToLeaderboard", null);
            MNDirectUIHelper.ShowDashboard();
        }

        private void gotoAchievements(object sender, RoutedEventArgs e)
        {
            MNDirect.ExecAppCommand("jumpToAchievements", null);
            MNDirectUIHelper.ShowDashboard();
        }

        private void gotoHome(object sender, RoutedEventArgs e)
        {
            MNDirect.ExecAppCommand("jumpToUserHome", null);
            MNDirectUIHelper.ShowDashboard();
        }
    }
}