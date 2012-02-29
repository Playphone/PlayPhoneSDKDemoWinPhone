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

    public partial class VShopPack : PhoneApplicationPage
    {
        public VShopPack()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            IDictionary<string, string> parameters = this.NavigationContext.QueryString;
            int itemId = -1;
            if (Int32.TryParse(parameters["Id"], out itemId))
            {
                icon.Source = new BitmapImage(new Uri(MNDirect.GetVShopProvider().GetVShopPackImageURL(itemId)));

                MNVShopProvider.VShopPackInfo info = MNDirect.GetVShopProvider().FindVShopPackById(itemId);
                id.Text = info.Id.ToString();
                name.Text = info.Name;

                MNVShopProvider.VShopCategoryInfo ci = MNDirect.GetVShopProvider().FindVShopCategoryById(info.CategoryId);
                if(ci != null)
                {
                    category.Text = ci.Name;
                }

                description.Text = info.Description;
                price.Text = "( $" + (double)info.PriceValue / 100 + " )";


                is_hidden.IsChecked = (info.Model & MNVShopProvider.VShopPackInfo.IS_HIDDEN_MASK) != 0;
                hold_sales.IsChecked = (info.Model & MNVShopProvider.VShopPackInfo.IS_HOLD_SALES_MASK) != 0;

                pack_params.Text = info.AppParams;
            }
            base.OnNavigatedTo(e);
        }
    }
}