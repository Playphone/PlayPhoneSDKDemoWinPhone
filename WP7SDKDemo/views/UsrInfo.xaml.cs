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
using System.Windows.Media.Imaging;

namespace WP7SDKDemo.views
{
    public partial class UsrInfo : PhoneApplicationPage
    {
        public UsrInfo()
        {
            InitializeComponent();
            MNDirect.GetSession().MNSessionStatusChanged += new MNSession.MNSessionStatusChangedEventHandler(this.onStatusChanged);
            if (MNDirect.GetSession().IsUserLoggedIn())
            {
                onStatusChanged(0, 0);
            }
        }

        private void onStatusChanged(int newStatus, int oldStatus)
        {
            if (MNDirect.GetSession().IsUserLoggedIn())
            {
                MNUserInfo info = MNDirect.GetSession().GetMyUserInfo();
                this.user_avatar.Source = new BitmapImage(new Uri(info.AvatarUrl, UriKind.RelativeOrAbsolute));
                this.user_name.Text = info.UserId.ToString();
                this.user_id.Text = info.UserId.ToString();
                this.room_id.Text = MNDirect.GetSession().GetCurrentRoomId().ToString();
            }
        }
    }
}