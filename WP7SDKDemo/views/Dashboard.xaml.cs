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
using PlayPhone.MultiNet;

namespace WP7SDKDemo.views
{
    public partial class Dashboard : PhoneApplicationPage
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void showButton(object sender, RoutedEventArgs e)
        {
            MNDirectButton.Show();
        }

        private void hideButton(object sender, RoutedEventArgs e)
        {
            MNDirectButton.Hide();
        }

        private void showDashboard(object sender, RoutedEventArgs e)
        {
            MNDirectUIHelper.ShowDashboard();
        }
    }
}