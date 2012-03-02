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
            MNDirectButton.InitWithLocation(MNDirectButton.MNDIRECTBUTTON_TOPRIGHT);
            MNDirectButton.Show();

            initList();
        }

        private void initList()
        {
            List<MainListItem> data = new List<MainListItem>
                                          {
                                              new MainListItem("1.Required integration"),
                                              new MainListItem("    Login user", "/views/Login.xaml"),
                                              new MainListItem("    Dashboard", "/views/Dashboard.xaml"),
                                              new MainListItem("    Virtual economy", "/views/VEconomy.xaml"),
                                              new MainListItem("2.Advanced features"),
                                              new MainListItem("    Current user info", "/views/UsrInfo.xaml"),
                                              new MainListItem("    Leaderboards", "/views/Leaderboards.xaml"),
                                              new MainListItem("    Achievements", "/views/Achievements.xaml"),
                                              new MainListItem("    Social graph", "/views/Social.xaml"),
                                              new MainListItem("    Dashboard control", "/views/DashboardControl.xaml"),
                                              new MainListItem("    Cloud storage", "/views/Cloud.xaml"),
                                              new MainListItem("    Game settings", "/views/Settings.xaml"),
                                              new MainListItem("    Room cookies", "/views/RoomCookies.xaml"),
                                              new MainListItem("    Multiplayer basics", "/views/Multinet.xaml"),
                                              new MainListItem("    Server info", "/views/ServerInfo.xaml"),
                                              new MainListItem("    Application info", "/views/AppInfo.xaml"),
                                          };

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
    }
}