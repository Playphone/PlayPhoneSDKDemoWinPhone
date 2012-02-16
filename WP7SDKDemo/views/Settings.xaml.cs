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
using PlayPhone.MultiNet;
using PlayPhone.MultiNet.Providers;
using PlayPhone.MultiNet.Core;

namespace WP7SDKDemo.views
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (MNDirect.GetGameSettingsProvider().IsGameSettingListNeedUpdate())
            {
                MNDirect.GetGameSettingsProvider().GameSettingsListUpdated += Settings_GameSettingsListUpdated;
                MNDirect.GetGameSettingsProvider().DoGameSettingListUpdate();
            }
            else
            {
                Settings_GameSettingsListUpdated();
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetGameSettingsProvider().GameSettingsListUpdated -= Settings_GameSettingsListUpdated;
            base.OnNavigatingFrom(e);
        }

        void Settings_GameSettingsListUpdated()
        {
            MNDirect.GetGameSettingsProvider().GameSettingsListUpdated -= Settings_GameSettingsListUpdated;
            List<MNGameSettingsProvider.GameSettingInfo> items = new List<MNGameSettingsProvider.GameSettingInfo>(MNDirect.GetGameSettingsProvider().GetGameSettingsList());
            foreach (var gameSettingInfo in items.Where(gameSettingInfo => gameSettingInfo.Id == 0 && gameSettingInfo.Name.Length == 0))
            {
                gameSettingInfo.Name = "(Default)";
            }
            settings.ItemsSource = items;
        }

        private void goToSettings(object sender, RoutedEventArgs e)
        {

            MNGameSettingsProvider.GameSettingInfo item = ((Button)sender).DataContext as MNGameSettingsProvider.GameSettingInfo;
            if (item != null)
            {
                item = MNDirect.GetGameSettingsProvider().FindGameSettingById(item.Id);
                string param =
                    String.Format(
                        "?Id={0}&Name={1}&Params={2}&SysParams={3}&MultiplayerEnabled={4}&LeaderboardVisible={5}",
                        item.Id, item.Name, item.Params, item.SysParams, item.MultiplayerEnabled, item.LeaderboardVisible);

                NavigationService.Navigate(new Uri("/miniview/Setting.xaml" + param, UriKind.RelativeOrAbsolute));
            }
        }
    }
}