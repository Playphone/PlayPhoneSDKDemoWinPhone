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
using WP7SDKDemo.common;
using PlayPhone.MultiNet.Core;
using PlayPhone.MultiNet;
using PlayPhone.MultiNet.Providers;
using PlayPhone.MultiNet.Core.WS.Data;

namespace WP7SDKDemo.miniview.leaderboards
{


    public partial class SimpleLeaderboard : PhoneApplicationPage
    {
        private int selectedIndex = 0;

        public SimpleLeaderboard()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            complexityList.ItemsSource = Complexity.getProviderList();
            complexityList.SelectedIndex = selectedIndex;
            scopeList.ItemsSource = Scope.getFilledList();
            periodList.ItemsSource = Period.getFilledList(); 
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            selectedIndex = 0;
            base.OnNavigatingFrom(e);
        }
        
        private void loadLeaderboard(object sender, RoutedEventArgs e)
        {
            ListPickerItem complexityItem = complexityList.ItemContainerGenerator.ContainerFromIndex(complexityList.SelectedIndex) as ListPickerItem;
            ListPickerItem scopeItem = scopeList.ItemContainerGenerator.ContainerFromIndex(scopeList.SelectedIndex) as ListPickerItem;
            ListPickerItem periodItem = periodList.ItemContainerGenerator.ContainerFromIndex(periodList.SelectedIndex) as ListPickerItem;

            if (complexityItem != null && scopeItem != null && periodItem != null)
            {
                string param = String.Format("?Type={0}&Scope={1}&Period={2}&Complexity={3}",
                                                LeadersList.Simple, 
                                                (scopeItem.DataContext as Scope).Index,
                                                (periodItem.DataContext as Period).Index, 
                                                (complexityItem.DataContext as Complexity).Value);

                NavigationService.Navigate(new Uri("/miniview/leaderboards/LeadersList.xaml" + param, UriKind.RelativeOrAbsolute));
            }
        }

        private void complexityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                if (complexityList.SelectedItem != null)
                {
                    selectedIndex = complexityList.SelectedIndex;
                }
            }
        }
    }
}