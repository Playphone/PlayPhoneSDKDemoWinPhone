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

namespace WP7SDKDemo.miniview.leaderboards
{
    using PlayPhone.MultiNet;
    using WP7SDKDemo.common;

    public partial class ScorePost : PhoneApplicationPage
    {
        private int selectedIndex = 0;
        public ScorePost()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            complexityList.ItemsSource = Complexity.getProviderList();
            complexityList.SelectedIndex = selectedIndex;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            selectedIndex = 0;
            base.OnNavigatingFrom(e);
        }

        private void postScore(object sender, RoutedEventArgs e)
        {
            long scores = 0;
            if( Int64.TryParse(score.Text, out scores) )
            {
                MNDirect.PostGameScore(scores);
                ListPickerItem complexityItem = complexityList.ItemContainerGenerator.ContainerFromIndex(complexityList.SelectedIndex) as ListPickerItem;
                string param = String.Format("?Type={0}&Scope={1}&Period={2}&Complexity={3}",
                                    LeadersList.Simple,
                                    Scope.Global,
                                    Period.Week,
                                    (complexityItem.DataContext as Complexity).Value);

                NavigationService.Navigate(new Uri("/miniview/leaderboards/LeadersList.xaml" + param, UriKind.RelativeOrAbsolute));
            }
            else
            {
                MessageBox.Show("Wrong score!");
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