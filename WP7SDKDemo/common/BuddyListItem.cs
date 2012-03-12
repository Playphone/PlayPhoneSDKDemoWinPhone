using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PlayPhone.MultiNet.Core.WS.Data;
using System.Windows.Media.Imaging;

namespace WP7SDKDemo.common
{
    public class BuddyListItem
    {
        public string Name { get; set; }

        public string Status { get; set; }

        public BitmapImage Avatar { get; set; }

        public MNWSBuddyListItem Source { get; set; }


        public BuddyListItem(MNWSBuddyListItem item)
        {
            Source = item;
            Name = item.GetFriendUserNickName();
            Status = item.GetFriendUserOnlineNow().Value ? "online" : "offline";
            Avatar = new BitmapImage(new Uri(item.GetFriendUserAvatarUrl()));
        }
    }
}
