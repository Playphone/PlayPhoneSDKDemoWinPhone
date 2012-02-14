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
        private bool _isLoggedIn = false;

        public bool isLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                this.caption.Text = value ? "Logout" : "Login";
            }
        }

        public Login()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MNDirect.GetSession().SessionStatusChanged += this.onUserChanged;
            isLoggedIn = MNDirect.GetSession().IsUserLoggedIn(); 
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetSession().SessionStatusChanged -= this.onUserChanged;
            base.OnNavigatingFrom(e);
        }

        private void Loginout_Click(object sender, RoutedEventArgs e)
        {
            
            if (!isLoggedIn)
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
            isLoggedIn = newStatus >= MNConst.MN_LOGGEDIN;
        }
    }
}