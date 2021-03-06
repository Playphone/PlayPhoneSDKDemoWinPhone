﻿using System;
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

namespace WP7SDKDemo.views
{
    using PlayPhone.MultiNet;
    using PlayPhone.MultiNet.Core.WS.Data;
    using PlayPhone.MultiNet.Providers;

    public partial class RoomCookies : PhoneApplicationPage
    {
        public RoomCookies()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MNDirect.GetGameRoomCookiesProvider().GameRoomCookieDownloadFailed += RoomCookies_GameRoomCookieDownloadFailed;
            MNDirect.GetGameRoomCookiesProvider().GameRoomCookieDownloadSucceeded += RoomCookies_GameRoomCookieDownloadSucceeded;
            MNDirect.GetWSProvider().Send(new MNWSInfoRequestCurrGameRoomList(OnCompleted));
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetGameRoomCookiesProvider().GameRoomCookieDownloadFailed -= RoomCookies_GameRoomCookieDownloadFailed;
            MNDirect.GetGameRoomCookiesProvider().GameRoomCookieDownloadSucceeded -= RoomCookies_GameRoomCookieDownloadSucceeded;
            base.OnNavigatingFrom(e);
        }

        private void OnCompleted(MNWSInfoRequestCurrGameRoomList.RequestResult result)
        {
            List<RoomListItem> items = result.GetDataEntry().Select(room => new RoomListItem(room)).ToList();
            roomlist.ItemsSource = items;
        }

        void RoomCookies_GameRoomCookieDownloadSucceeded(int roomSFId, int key, string cookie)
        {
            MessageBox.Show("Cookie downloaded successfully! Key= " + key + " Value= " + cookie);
        }

        void RoomCookies_GameRoomCookieDownloadFailed(int roomSFId, int key, string error)
        {
            MessageBox.Show("Cookie download failed! " + error);
        }

        private void upload(object sender, RoutedEventArgs e)
        {
            try
            {
                MNDirect.GetGameRoomCookiesProvider().SetCurrentGameRoomCookie(Int32.Parse(upload_key.Text), upload_data.Text);
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
                RoomListItem currentItem = roomlist.SelectedItem as RoomListItem;
                if( currentItem != null )
                {
                    if( currentItem.IsCurrent )
                    {
                        string cookie = MNDirect.GetGameRoomCookiesProvider().GetCurrentGameRoomCookie(Int32.Parse(download_key.Text));
                        MessageBox.Show("Cookie downloaded successfully! Key= " + download_key.Text + " Value= " + cookie);
                    }
                    else
                    {
                        MNDirect.GetGameRoomCookiesProvider().DownloadGameRoomCookie(currentItem.RoomSfid, Int32.Parse(download_key.Text));
                    }
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show("There should be integer value between 1 and 99!");
            }
        }
    }

    public class RoomListItem
    {
        private readonly int roomSfid;
        private readonly string roomName;
        private readonly bool isCurrent = false;

        public RoomListItem(MNWSRoomListItem room)
        {
            var roomSfId = room.GetRoomSFId();
            if (roomSfId != null)
            {
                roomSfid = roomSfId.Value;
            }

            if (MNDirect.GetSession().GetCurrentRoomId() == roomSfid)
            {
                roomName = "{" + roomSfid + "} " + room.GetRoomName() + " (Current)";
                isCurrent = true;
            }
            else
            {
                roomName = "{" + roomSfid + "} " + room.GetRoomName();
            }
        }

        public bool IsCurrent
        {
            get { return isCurrent; }
        }

        public string RoomName
        {
            get { return roomName; }
        }

        public int RoomSfid
        {
            get { return roomSfid; }
        }
    }
}