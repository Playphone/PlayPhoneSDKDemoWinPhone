using System.Linq;
using System.Windows;
using PlayPhone.MultiNet;
using PlayPhone.MultiNet.Providers;
using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Controls;

namespace WP7SDKDemo.miniview.veconomy
{
    public partial class UserItems
    {
        public class PlayerItem
        {
            public int Id { get; set; }
            public long Count { get; set; }
            public string Name { get; set; }
            public BitmapImage Image { get; set; }

            public PlayerItem(MNVItemsProvider.PlayerVItemInfo item)
            {
                Id = item.Id;
                Count = item.Count;
                Name = MNDirect.GetVItemsProvider().FindGameVItemById(Id).Name;
                Image = new BitmapImage(new Uri(MNDirect.GetVItemsProvider().GetVItemImageURL(item.Id), UriKind.RelativeOrAbsolute));
            }
        }

        public UserItems()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MNDirect.GetVItemsProvider().VItemsTransactionCompleted += UserItems_VItemsTransactionCompleted;
            MNDirect.GetVItemsProvider().VItemsTransactionFailed += UserItems_VItemsTransactionFailed;

            if (MNDirect.GetVItemsProvider().IsGameVItemsListNeedUpdate())
            {
                MNDirect.GetVItemsProvider().VItemsListUpdated += UserItemsVItemsListUpdated;
                MNDirect.GetVItemsProvider().DoGameVItemsListUpdate();
            }
            else
            {
                UserItemsVItemsListUpdated();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetVItemsProvider().VItemsTransactionCompleted -= UserItems_VItemsTransactionCompleted;
            MNDirect.GetVItemsProvider().VItemsTransactionFailed -= UserItems_VItemsTransactionFailed;
            MNDirect.GetVItemsProvider().VItemsListUpdated -= UserItemsVItemsListUpdated;
            base.OnNavigatingFrom(e);
        }

        void UserItems_VItemsTransactionFailed(MNVItemsProvider.TransactionError error)
        {
            MessageBox.Show("Transaction ( id=" + error.ClientTransactionId + " ) failed. " +
                                                    error.FailReasonCode + ":" + error.ErrorMessage);
        }

        void UserItems_VItemsTransactionCompleted(MNVItemsProvider.TransactionInfo transaction)
        {
            MessageBox.Show("Transaction ( id=" + transaction.ClientTransactionId + " ) completed");
        }

        void UserItemsVItemsListUpdated()
        {
            item_chooser.ItemsSource = MNDirect.GetVItemsProvider().GetGameVItemsList();
            item_chooser.SelectedIndex = 0;

            MNVItemsProvider.PlayerVItemInfo[] usrItems = MNDirect.GetVItemsProvider().GetPlayerVItemList();
            List<PlayerItem> processed = usrItems.Select(item => new PlayerItem(item)).ToList();

            user_items.ItemsSource = processed;
        }

        private void remove(object sender, RoutedEventArgs e)
        {
            MNVItemsProvider.GameVItemInfo selectedItem = (MNVItemsProvider.GameVItemInfo)item_chooser.SelectedItem;
            if (selectedItem != null)
            {
                long numOfItems = 0;
                if (Int64.TryParse(count.Text, out numOfItems))
                {
                    long transactionId = MNDirect.GetVItemsProvider().GetNewClientTransactionId();
                    MessageBox.Show("Transaction ( id=" + transactionId + " ) started. " + numOfItems + " items will be removed");
                    MNDirect.GetVItemsProvider().ReqAddPlayerVItem(selectedItem.Id, -1 * numOfItems, transactionId);
                }
            }
        }

        private void add(object sender, RoutedEventArgs e)
        {
            MNVItemsProvider.GameVItemInfo selectedItem = (MNVItemsProvider.GameVItemInfo)item_chooser.SelectedItem;
            if (selectedItem != null)
            {
                long numOfItems;
                if (Int64.TryParse(count.Text, out numOfItems))
                {
                    long transactionId = MNDirect.GetVItemsProvider().GetNewClientTransactionId();
                    MessageBox.Show("Transaction ( id=" + transactionId + " ) started. " + numOfItems + " items will be added");
                    MNDirect.GetVItemsProvider().ReqAddPlayerVItem(selectedItem.Id, numOfItems, transactionId);
                }
            }
        }

        private void showItem(object sender, RoutedEventArgs e)
        {
            PlayerItem item = ((Button)sender).DataContext as PlayerItem;
            if (item != null)
            {
                NavigationService.Navigate(new Uri("/miniview/veconomy/ItemInfo.xaml" + String.Format("?Id={0}", item.Id), UriKind.RelativeOrAbsolute));
            }
        }
    }
}