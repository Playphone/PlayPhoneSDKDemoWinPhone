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
using PlayPhone.MultiNet.Core.WS;
using PlayPhone.MultiNet.Core.WS.Data;
using WP7SDKDemo.common;
using System.Windows.Navigation;

namespace WP7SDKDemo.views
{
    public partial class Social : PhoneApplicationPage
    {
        private string blockName;

        public Social()
        {
            InitializeComponent();
        }

        private void loadBuddies(object sender, RoutedEventArgs e)
        {
            MNWSRequestContent content = new MNWSRequestContent();
            blockName = content.AddCurrUserBuddyList();
            MNWSRequestSender reqSender = new MNWSRequestSender(MNDirect.GetSession());
            reqSender.SendWSRequestSmartAuth(content, OnWSRequestCompleted, OnWSRequestFailed);
        }

        private void OnWSRequestCompleted(MNWSResponse ret)
        {
            List<MNWSBuddyListItem> buddies = (List<MNWSBuddyListItem>)ret.GetDataForBlock(blockName);
            List<BuddyListItem> data = new List<BuddyListItem>();

            foreach (MNWSBuddyListItem o in buddies)
            {
                data.Add(new BuddyListItem(o));
            }
            this.buddy_list.ItemsSource = data;
        }

        private void OnWSRequestFailed(MNWSRequestError ret)
        {
            MNDebug.error(ret.Message);
        }

        private void showDetails(object sender, RoutedEventArgs e)
        {
            BuddyListItem item = ((Button)sender).DataContext as BuddyListItem;
            (Application.Current as App).choosenBuddy = item.Source;
            NavigationService.Navigate(new Uri("/miniview/BuddyInfo.xaml", UriKind.RelativeOrAbsolute));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (Application.Current as App).choosenBuddy = null;
            base.OnNavigatedTo(e);
        }
    }
}