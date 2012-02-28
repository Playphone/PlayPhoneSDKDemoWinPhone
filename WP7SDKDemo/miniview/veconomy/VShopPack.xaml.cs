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
    using PlayPhone.MultiNet;

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
//                MNVItemsProvider.GameVItemInfo item = MNDirect.GetVItemsProvider().FindGameVItemById(itemId);
//                icon.Source = new BitmapImage(new Uri(MNDirect.GetVItemsProvider().GetVItemImageURL(itemId)));
//                id.Text = item.Id.ToString();
//                name.Text = item.Name;
//                model.Text = item.Model.ToString();
//                description.Text = item.Description;
//                additional_params.Text = item.Params;
            }
            base.OnNavigatedTo(e);
        }
    }
}