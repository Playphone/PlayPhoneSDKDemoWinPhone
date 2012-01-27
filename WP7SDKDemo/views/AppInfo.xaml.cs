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
    public partial class AppInfo : PhoneApplicationPage
    {
        public AppInfo()
        {
            InitializeComponent();
            
            this.sdk_version.Text = MNSession.CLIENT_API_VERSION;
            /*this.multinet_config.Text =*/
            this.web_server_url.Text = MNDirect.GetSession().GetWebServerURL();
            /*this.sf_url.Text = MNDirect.GetSession()*/
            //TODO: Finish IT!!!!
        }
    }
}