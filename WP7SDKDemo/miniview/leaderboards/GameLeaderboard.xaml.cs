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
    public partial class GameLeaderboard : PhoneApplicationPage
    {
        public GameLeaderboard()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            periodList.ItemsSource = Period.getFilledList(); 
            base.OnNavigatedTo(e);
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            player_id.IsEnabled = true;
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            player_id.IsEnabled = false;
        }

        private void ToggleSwitch_Checked_1(object sender, RoutedEventArgs e)
        {
            game_id.IsEnabled = true;
        }

        private void ToggleSwitch_Unchecked_1(object sender, RoutedEventArgs e)
        {
            game_id.IsEnabled = false;
        }

        private void load(object sender, RoutedEventArgs e)
        {
            long playerId = -1;
            if( !Int64.TryParse(player_id.Text, out playerId) )
            {
                playerId = MNDirect.GetSession().GetMyUserId();
                if(player_id.IsEnabled)
                {
                    MessageBox.Show("Wrong player id. Current used");
                }
            }
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
                if(gameset_id.IsEnabled)
                {
                    MessageBox.Show("Wrong gameset id. Default used");
                }
            }

            ListPickerItem periodItem = periodList.ItemContainerGenerator.ContainerFromIndex(periodList.SelectedIndex) as ListPickerItem;
            if (periodItem != null)
            {
                string param = String.Format("?Type={0}&UserId={1}&GameSetId={2}&Period={3}&GameId={4}",
                                                LeadersList.Game, playerId, gameSetId, (periodItem.DataContext as Period).Index, gameId );
                NavigationService.Navigate(new Uri("/miniview/leaderboards/LeadersList.xaml" + param, UriKind.RelativeOrAbsolute));
            }
        }
    }
}