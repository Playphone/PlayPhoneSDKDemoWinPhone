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
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        private BitmapImage avatar;
        public BitmapImage Avatar
        {
            get { return avatar; }
            set { avatar = value; }
        }

        private MNWSBuddyListItem source;
        public MNWSBuddyListItem Source
        {
            get { return source; }
            set { source = value; }
        }


        public BuddyListItem(MNWSBuddyListItem item)
        {
            source = item;
            Name = item.GetFriendUserNickName();
            Status = item.GetFriendUserOnlineNow().Value ? "online" : "offline";
            Avatar = new BitmapImage(new Uri(item.GetFriendUserAvatarUrl()));
        }
    }
}
