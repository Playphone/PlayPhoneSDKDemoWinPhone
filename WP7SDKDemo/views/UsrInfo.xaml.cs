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
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MNDirect.GetSession().SessionStatusChanged += onStatusChanged;
            if (MNDirect.GetSession().IsUserLoggedIn())
            {
                onStatusChanged(0, 0);
            } 
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetSession().SessionStatusChanged -= onStatusChanged;
            base.OnNavigatingFrom(e);
        }

        private void onStatusChanged(int newStatus, int oldStatus)
        {
            if (MNDirect.GetSession().IsUserLoggedIn())
            {
                MNUserInfo info = MNDirect.GetSession().GetMyUserInfo();
                this.user_avatar.Source = new BitmapImage(new Uri(info.UserAvatarUrl, UriKind.RelativeOrAbsolute));
                this.user_name.Text = info.UserId.ToString();
                this.user_id.Text = info.UserId.ToString();
                this.room_id.Text = MNDirect.GetSession().GetCurrentRoomId().ToString();
            }
        }
    }
}