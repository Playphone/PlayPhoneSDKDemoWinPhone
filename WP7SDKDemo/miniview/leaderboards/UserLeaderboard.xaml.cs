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
using PlayPhone.MultiNet;

namespace WP7SDKDemo.miniview.leaderboards
{
    public partial class UserLeaderboard : PhoneApplicationPage
    {
        public UserLeaderboard()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            scopeList.ItemsSource = Scope.getFilledList();
            periodList.ItemsSource = Period.getFilledList(); 
            base.OnNavigatedTo(e);
        }

        private void load(object sender, RoutedEventArgs e)
        {
            int gameId = -1;
            if( !Int32.TryParse(game_id.Text, out gameId) )
            {
                gameId = MNDirect.GetSession().GetGameId();
                if(game_id.IsEnabled)
                {
                    MessageBox.Show("Wrong game id. Current used");
                }
            }

            int gameSetId = -1;
            if( !Int32.TryParse(gameset_id.Text, out gameSetId) )
            {
                gameSetId = MNDirect.GetSession().GetDefaultGameSetId();
                if (gameset_id.IsEnabled)
                {
                    MessageBox.Show("Wrong gameset id. Default used");
                }
            }

            ListPickerItem periodItem = periodList.ItemContainerGenerator.ContainerFromIndex(periodList.SelectedIndex) as ListPickerItem;
            ListPickerItem scopeItem = scopeList.ItemContainerGenerator.ContainerFromIndex(scopeList.SelectedIndex) as ListPickerItem;
            if (periodItem != null && scopeItem != null)
            {
                string param = String.Format("?Type={0}&GameId={1}&GameSetId={2}&Period={3}&Scope={4}",
                                                LeadersList.User, gameId, gameSetId, 
                                                (periodItem.DataContext as Period).Index, 
                                                (scopeItem.DataContext as Scope).Index );
                NavigationService.Navigate(new Uri("/miniview/leaderboards/LeadersList.xaml" + param, UriKind.RelativeOrAbsolute));
            }
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            game_id.IsEnabled = true;
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            game_id.IsEnabled = false;
        }
    }
}