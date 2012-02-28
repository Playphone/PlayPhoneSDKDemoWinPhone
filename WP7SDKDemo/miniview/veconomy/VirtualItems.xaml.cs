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

namespace WP7SDKDemo.miniview.veconomy
{
    using System.Diagnostics;
    using System.Windows.Media.Imaging;
    using PlayPhone.MultiNet;
    using PlayPhone.MultiNet.Providers;

    public partial class VirtualItems : PhoneApplicationPage
    {
        public class GameItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public BitmapImage Image { get; set; }

            public GameItem(MNVItemsProvider.GameVItemInfo item)
            {
                Id = item.Id;
                Name = item.Name;
                Image = new BitmapImage(new Uri(MNDirect.GetVItemsProvider().GetVItemImageURL(item.Id), UriKind.RelativeOrAbsolute));
            }
        }

        public VirtualItems()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (MNDirect.GetVItemsProvider().IsGameVItemsListNeedUpdate())
            {
                MNDirect.GetVItemsProvider().VItemsListUpdated += VirtualItems_VItemsInfoUpdated;
                MNDirect.GetVItemsProvider().DoGameVItemsListUpdate();
            }
            else
            {
                VirtualItems_VItemsInfoUpdated();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetVItemsProvider().VItemsListUpdated -= VirtualItems_VItemsInfoUpdated;
            base.OnNavigatingFrom(e);
        }

        void VirtualItems_VItemsInfoUpdated()
        {
            var allGameItems = MNDirect.GetVItemsProvider().GetGameVItemsList();
            List<GameItem> currency_items = new List<GameItem>();
            List<GameItem> true_items = new List<GameItem>();

            foreach (var playerVItemInfo in allGameItems)
            {
                Debug.Assert(playerVItemInfo != null, "playerVItemInfo != null");
                if((playerVItemInfo.Model & 1) != 0)
                {
                    currency_items.Add(new GameItem(playerVItemInfo));
                }
                else
                {
                    true_items.Add(new GameItem(playerVItemInfo));
                }
            }

            items.ItemsSource = true_items;
            currencies.ItemsSource = currency_items;
        }

        private void showItem(object sender, RoutedEventArgs e)
        {
            GameItem item = ((Button)sender).DataContext as GameItem;
            if (item != null)
            {
                NavigationService.Navigate(new Uri("/miniview/veconomy/ItemInfo.xaml" + String.Format("?Id={0}", item.Id), UriKind.RelativeOrAbsolute));
            }
        }
    }
}