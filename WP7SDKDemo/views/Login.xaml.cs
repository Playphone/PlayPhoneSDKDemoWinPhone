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
using PlayPhone.MultiNet.Core;

namespace WP7SDKDemo.views
{
    public partial class Login : PhoneApplicationPage
    {
        public Login()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MNDirect.GetSession().SessionStatusChanged += onUserChanged;
            if( MNDirect.GetSession().IsUserLoggedIn() )
            {
                onUserChanged(0, 0);
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetSession().SessionStatusChanged -= onUserChanged;
            base.OnNavigatingFrom(e);
        }

        private void Loginout_Click(object sender, RoutedEventArgs e)
        {
            if (!MNDirect.GetSession().IsUserLoggedIn())
            {
                MNDirectUIHelper.ShowDashboard();
            }
            else
            {
                MNDirect.GetSession().Logout();
            }
        }

        private void onUserChanged(int newStatus, int oldStatus)
        {
            if(MNDirect.GetSession().IsUserLoggedIn())
            {
                MNUserInfo info = MNDirect.GetSession().GetMyUserInfo();
                status_txt.Text = info.UserName + " is logged in";
                caption.Text = "Logout";
            }
            else
            {
                caption.Text = "Login";
                status_txt.Text = "User in not logged in";
            }
        }
    }
}