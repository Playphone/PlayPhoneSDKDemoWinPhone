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

using WP7SDKDemo.common;
using PlayPhone.MultiNet;

namespace WP7SDKDemo
{
    public partial class MainPage : PhoneApplicationPage
    {

        private const int GAME_ID = 10900;
        private const uint APISECRET1 = 0xae2b10f2;
        private const uint APISECRET2 = 0x248f58d9;
        private const uint APISECRET3 = 0xc9654f24;
        private const uint APISECRET4 = 0x37960337;

        public MainPage()
        {
            InitializeComponent();
            MNDirect.Init(GAME_ID, MNDirect.MakeGameSecretByComponents(APISECRET1, APISECRET2, APISECRET3, APISECRET4));
            MNDirectButton.Show();

            initList();
        }

        private void initList()
        {
            List<MainListItem> data = new List<MainListItem>();

            data.Add(new MainListItem("1.Required integration"));
            data.Add(new MainListItem("    Login user", "/views/Login.xaml"));
            data.Add(new MainListItem("    Dashboard", "/views/Dashboard.xaml"));
            data.Add(new MainListItem("    Virtual economy", "/views/VEconomy.xaml"));
            data.Add(new MainListItem("2.Advanced features"));
            data.Add(new MainListItem("    Current user info", "/views/UsrInfo.xaml"));
            data.Add(new MainListItem("    Settings info", "/views/Settings.xaml"));
            data.Add(new MainListItem("    Leaderboards", "/views/Leaderboards.xaml"));
            data.Add(new MainListItem("    Achievements", "/views/Achievements.xaml"));
            data.Add(new MainListItem("    Social graph", "/views/Social.xaml"));
            data.Add(new MainListItem("    Dashboard control", "/views/DashboardControl.xaml"));
            data.Add(new MainListItem("    Cloud storage", "/views/Cloud.xaml"));
            data.Add(new MainListItem("    Room cookies", "/views/RoomCookies.xaml"));
            data.Add(new MainListItem("    Application info", "/views/AppInfo.xaml"));
            data.Add(new MainListItem("    Multinet", "/views/Multinet.xaml"));
            data.Add(new MainListItem("    Server info", "/views/ServerInfo.xaml"));
            lb1.ItemsSource = data;
        }

        private void changeView(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).DataContext as MainListItem;
            if (item.ViewLocation != null)
            {
                this.NavigationService.Navigate(new Uri(item.ViewLocation, UriKind.Relative));
            }
        }

        protected override void OnOrientationChanged(Microsoft.Phone.Controls.OrientationChangedEventArgs e)
        {
            MNDirectUIHelper.changeDashboardOrientation(e.Orientation);
        }
    }
}