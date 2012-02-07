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
using PlayPhone.MultiNet.Providers;
using PlayPhone.MultiNet.Core;

namespace WP7SDKDemo.views
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MNDirect.GetServerInfoProvider().ServerInfoItemReceived += onServerInfoItemReceivedEventHandler;
            MNDirect.GetServerInfoProvider().ServerInfoItemRequestFailed += onServerInfoItemRequestFailedEventHandler;
            MNDirect.GetServerInfoProvider().RequestServerInfoItem(MNServerInfoProvider.SERVER_TIME_INFO_KEY);
        }

        private void onServerInfoItemReceivedEventHandler(int key, string value)
        {
            MNDebug.debug("onServerInfoItemReceivedEventHandler");
        }

        private void onServerInfoItemRequestFailedEventHandler(int key, string error)
        {
            MNDebug.debug("onServerInfoItemRequestFailedEventHandler");
        }
    }
}