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
            base.OnNavigatingFrom(e);
        }

        void VShop_VShopInfoUpdated()
        {
            categories.ItemsSource = MNDirect.GetVShopProvider().GetVShopCategoryList();

            MNVShopProvider.VShopPackInfo[] usrItems = MNDirect.GetVShopProvider().GetVShopPackList();
            vshop_packs.ItemsSource = usrItems.Select(item => new VShopPackItem(item)).ToList();
        }

        private void showPackInfo(object sender, RoutedEventArgs e)
        {
            VShopPackItem item = ((Button)sender).DataContext as VShopPackItem;
            if (item != null)
            {
                NavigationService.Navigate(new Uri("/miniview/veconomy/VShopPack.xaml" + String.Format("?Id={0}", item.Id), UriKind.RelativeOrAbsolute));
            }
        }
    }
}