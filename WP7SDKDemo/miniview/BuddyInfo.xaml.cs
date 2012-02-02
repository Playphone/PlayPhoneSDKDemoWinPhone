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
using System.Windows.Navigation;
using WP7SDKDemo.views;
using PlayPhone.MultiNet.Core.WS.Data;
using System.Windows.Media.Imaging;


namespace WP7SDKDemo.miniview
{
    public partial class BuddyInfo : PhoneApplicationPage
    {
        public BuddyInfo()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            MNWSBuddyListItem buddy = (Application.Current as App).choosenBuddy;
            if (buddy != null)
            {
                avatar.Source = new BitmapImage(new Uri(buddy.GetFriendUserAvatarUrl()));
                name.Text = buddy.GetFriendUserNickName();
                if (buddy.GetFriendUserOnlineNow().HasValue)
                {
                    status.Text = buddy.GetFriendUserOnlineNow().Value ? "online" : "offline";
                }
                user_id.Text = buddy.GetFriendUserId().ToString();
                current_game.Text = buddy.GetFriendInGameName();
                this_game.Text = buddy.GetFriendHasCurrentGame().ToString();
                ignored.Text = buddy.GetFriendIsIgnored().ToString();
            }
            base.OnNavigatedTo(args);
        }
    }
}