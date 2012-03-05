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
    using System.Windows.Media.Imaging;
    using PlayPhone.MultiNet;
    using PlayPhone.MultiNet.Providers;

    public partial class VShop : PhoneApplicationPage
    {
        private int selectedIndex = 0;

        public class VShopPackItem
        {
            public int Id { get; set; }
            public String Price { get; set; }
            public string Name { get; set; }
            public BitmapImage Image { get; set; }

            public VShopPackItem(MNVShopProvider.VShopPackInfo item)
            {
                Id = item.Id;
                Price = "( $" + (double)item.PriceValue/100 + " )";
                Name = item.Name;
                Image = new BitmapImage(new Uri(MNDirect.GetVShopProvider().GetVShopPackImageURL(item.Id), UriKind.RelativeOrAbsolute));
            }
        }

        public VShop()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MNDirect.GetVShopProvider().HideDashboard += VShop_HideDashboard;
            MNDirect.GetVShopProvider().ShowDashboard += VShop_ShowDashboard;
            MNDirect.GetVShopProvider().CheckoutVShopPackFail += VShop_CheckoutVShopPackFail;
            MNDirect.GetVShopProvider().CheckoutVShopPackSuccess += VShop_CheckoutVShopPackSuccess;

            if (MNDirect.GetVShopProvider().IsVShopInfoNeedUpdate())
            {
                MNDirect.GetVShopProvider().VShopInfoUpdated += VShop_VShopInfoUpdated;
                MNDirect.GetVShopProvider().DoVShopInfoUpdate();
            }
            else
            {
                VShop_VShopInfoUpdated();
            }


            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetVShopProvider().VShopInfoUpdated -= VShop_VShopInfoUpdated;
            MNDirect.GetVShopProvider().HideDashboard -= VShop_HideDashboard;
            MNDirect.GetVShopProvider().ShowDashboard -= VShop_ShowDashboard;
            MNDirect.GetVShopProvider().CheckoutVShopPackFail -= VShop_CheckoutVShopPackFail;
            MNDirect.GetVShopProvider().CheckoutVShopPackSuccess -= VShop_CheckoutVShopPackSuccess;
            base.OnNavigatingFrom(e);
        }

        void VShop_VShopInfoUpdated()
        {
            categories.ItemsSource = MNDirect.GetVShopProvider().GetVShopCategoryList();

            MNVShopProvider.VShopPackInfo[] usrItems = MNDirect.GetVShopProvider().GetVShopPackList();
            vshop_packs.ItemsSource = usrItems.Select(item => new VShopPackItem(item)).ToList();
            var old = selectedIndex;
            shop_pack_picker.ItemsSource = vshop_packs.ItemsSource;
            shop_pack_picker.SelectedIndex = old;
        }

        private void showPackInfo(object sender, RoutedEventArgs e)
        {
            VShopPackItem item = ((Button)sender).DataContext as VShopPackItem;
            if (item != null)
            {
                NavigationService.Navigate(new Uri("/miniview/veconomy/VShopPack.xaml" + String.Format("?Id={0}", item.Id), UriKind.RelativeOrAbsolute));
            }
        }

        private void buyPack(object sender, RoutedEventArgs e)
        {
            ListPickerItem selectedPack = shop_pack_picker.ItemContainerGenerator.ContainerFromIndex(shop_pack_picker.SelectedIndex) as ListPickerItem;
            if (selectedPack != null)
            {
                MNDirect.GetVShopProvider().ExecCheckoutVShopPacks(new int[1] { ((VShopPackItem)selectedPack.DataContext).Id }, new int[1] {1}, 
                                                                   MNDirect.GetVItemsProvider().GetNewClientTransactionId());
            }
        }

        private void VShop_CheckoutVShopPackSuccess(MNVShopProvider.CheckoutVShopPackSuccessInfo result)
        {
            MNVItemsProvider.TransactionVItemInfo item = result.Transaction.VItems[0];
            MNVItemsProvider.GameVItemInfo info = MNDirect.GetVItemsProvider().FindGameVItemById(item.Id);
            MessageBox.Show("Transaction " + result.Transaction.ClientTransactionId + 
                            " finished. Item " + info.Name + "( id= " + info.Id + " ) succsessfully bought.");
        }

        private void VShop_CheckoutVShopPackFail(MNVShopProvider.CheckoutVShopPackFailInfo result)
        {
            MessageBox.Show("Transaction " + result.ClientTransactionId + " failed! Error " + result.ErrorCode +
                            " : " + result.ErrorMessage);
        }

        private void VShop_ShowDashboard()
        {
            MNDirectUIHelper.ShowDashboard();
        }

        private void VShop_HideDashboard()
        {
            MNDirectUIHelper.HideDashboard();
        }

        private void buyForCurrency(object sender, RoutedEventArgs e)
        {
            ListPickerItem selectedPack = shop_pack_picker.ItemContainerGenerator.ContainerFromIndex(shop_pack_picker.SelectedIndex) as ListPickerItem;
            if (selectedPack != null)
            {
                MNDirect.GetVShopProvider().ProcCheckoutVShopPacksSilent(new int[1] { ((VShopPackItem)selectedPack.DataContext).Id }, new int[1] { 1 },
                                                                   MNDirect.GetVItemsProvider().GetNewClientTransactionId());
            }
        }

        private void shop_pack_picker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                if (shop_pack_picker.SelectedItem != null)
                {
                    selectedIndex = shop_pack_picker.SelectedIndex;
                }
            }
        }
    }
}