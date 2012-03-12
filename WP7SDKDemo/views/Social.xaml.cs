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
    using System.Diagnostics;
    using PlayPhone.MultiNet.Providers;

    public partial class Social : PhoneApplicationPage
    {
        private string blockName;

        public Social()
        {
            InitializeComponent();
        }

        private void loadBuddies(object sender, RoutedEventArgs e)
        {
            MNWSInfoRequestCurrUserBuddyList req = new MNWSInfoRequestCurrUserBuddyList(onInfoReqComplete);
            MNDirect.GetWSProvider().Send(req);
        }

        private void onInfoReqComplete(MNWSInfoRequestCurrUserBuddyList.RequestResult result)
        {
            if(!result.HadError)
            {
                MNWSBuddyListItem[] buddies = result.GetDataEntry();
                List<BuddyListItem> data = buddies.Select(o => new BuddyListItem(o)).ToList();
                this.buddy_list.ItemsSource = data;
            }
            else
            {
                MessageBox.Show("Request failed: " + result.ErrorMessage);
            }
        }

        private void showDetails(object sender, RoutedEventArgs e)
        {
            BuddyListItem item = ((Button)sender).DataContext as BuddyListItem;
            ((App) Application.Current).choosenBuddy = item.Source;
            NavigationService.Navigate(new Uri("/miniview/BuddyInfo.xaml", UriKind.RelativeOrAbsolute));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((App) Application.Current).choosenBuddy = null;
            base.OnNavigatedTo(e);
        }
    }
}