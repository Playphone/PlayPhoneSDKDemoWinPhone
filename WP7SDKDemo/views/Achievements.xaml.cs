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

namespace WP7SDKDemo.views
{
    public partial class Achievements : PhoneApplicationPage
    {
        public Achievements()
        {
            InitializeComponent();

            if (MNDirect.GetAchievementsProvider().IsGameAchievementListNeedUpdate())
            {
                MNDirect.GetAchievementsProvider().GameAchievementListUpdated += onListUpdated;
                MNDirect.GetAchievementsProvider().DoGameAchievementListUpdate();
            }
            else
            {
                onListUpdated();
            }
        }

        private void unlockAchievement(object sender, RoutedEventArgs e)
        {
            MNDirect.GetAchievementsProvider().UnlockPlayerAchievement(Int32.Parse(achievement_id.Text));
        }

        private void onListUpdated()
        {
            MNAchievementsProvider.GameAchievementInfo[] rawAchievements = MNDirect.GetAchievementsProvider().GetGameAchievementsList();
            achievements_list.ItemsSource = rawAchievements;
        }
    }
}