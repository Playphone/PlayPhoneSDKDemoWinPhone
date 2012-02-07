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

namespace WP7SDKDemo.views
{
    public partial class Cloud : PhoneApplicationPage
    {
        public Cloud()
        {
            InitializeComponent();
            MNDirect.GetGameCookiesProvider().GameCookieUploadFailed += gameCookieUploadFailedEventHandler;
            MNDirect.GetGameCookiesProvider().GameCookieUploadSucceeded += gameCookieUploadSucceededEventHandler;
            MNDirect.GetGameCookiesProvider().GameCookieDownloadFailed += gameCookieDownloadFailedEventHandler;
            MNDirect.GetGameCookiesProvider().GameCookieDownloadSucceeded += gameCookieDownloadSucceededEventHandler;
        }

        private void gameCookieDownloadSucceededEventHandler(int key, string cookie)
        {
            MessageBox.Show("Cookie downloaded successfully! Key= " + key.ToString() + " Value= " + cookie);
        }

        private void gameCookieDownloadFailedEventHandler(int key, string error)
        {
            MessageBox.Show("Cookie download failed! " + error);
        }

        private void gameCookieUploadSucceededEventHandler(int key)
        {
            MessageBox.Show("Cookie uploaded successfully! Key= " + key.ToString());
        }

        private void gameCookieUploadFailedEventHandler(int key, string error)
        {
            MessageBox.Show("Cookie upload failed! " + error);
        }

        private void upload(object sender, RoutedEventArgs e)
        {
            try
            {
                MNDirect.GetGameCookiesProvider().UploadUserCookie(Int32.Parse(upload_key.Text), upload_data.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("There should be integer value between 1 and 99!");
            }
        }

        private void download(object sender, RoutedEventArgs e)
        {
            try
            {
                MNDirect.GetGameCookiesProvider().DownloadUserCookie(Int32.Parse(download_key.Text));
            }
            catch (FormatException ex)
            {
                MessageBox.Show("There should be integer value between 1 and 99!");
            }
        }
    }
}