﻿using System;
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
using PlayPhone.MultiNet.Providers;

namespace WP7SDKDemo.views
{
    public partial class ServerInfo : PhoneApplicationPage
    {
        public ServerInfo()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MNDirect.GetServerInfoProvider().ServerInfoItemReceived += serverInfoItemReceivedEventHandler;
            MNDirect.GetServerInfoProvider().ServerInfoItemRequestFailed += serverInfoItemRequestFailedEventHandler;
            MNDirect.GetSession().SessionStatusChanged += onStatusChanged;
            onStatusChanged(0, 0);
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetServerInfoProvider().ServerInfoItemReceived -= serverInfoItemReceivedEventHandler;
            MNDirect.GetServerInfoProvider().ServerInfoItemRequestFailed -= serverInfoItemRequestFailedEventHandler;
            MNDirect.GetSession().SessionStatusChanged -= onStatusChanged;
            base.OnNavigatingFrom(e);
        }

        private void serverInfoItemReceivedEventHandler(int key, string value)
        {
            tips.Text = "Key: " + key.ToString() + " Value: " + value;
        }

        private void serverInfoItemRequestFailedEventHandler(int key, string error)
        {
            MessageBox.Show(string.Format("Could not download server info: {0}", error));
        }

        private void onStatusChanged(int newStatus, int oldStatus)
        {
            if (MNDirect.GetSession().IsUserLoggedIn())
            {
                tips.Text = "";
            }
            else
            {
                tips.Text = "User should be logged in";
            }
        }

        private void downloadInfo(object sender, RoutedEventArgs e)
        {
            MNDirect.GetServerInfoProvider().RequestServerInfoItem(MNServerInfoProvider.SERVER_TIME_INFO_KEY);
        }
    }
}