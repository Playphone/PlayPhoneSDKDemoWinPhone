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
        private bool isLoggedIn = false;

        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
            set 
            { 
                isLoggedIn = value;
                if (value)
                {
                    this.caption.Text = "Logout";
                }
                else
                {
                    this.caption.Text = "Login";
                }
            }
        }

        public Login()
        {
            InitializeComponent();
            MNDirect.GetSession().MNSessionStatusChanged += new MNSession.MNSessionStatusChangedEventHandler(this.onUserChanged);
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
            isLoggedIn = MNDirect.GetSession().IsUserLoggedIn();
        }
    }
}