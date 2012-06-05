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
    using PlayPhone.MultiNet.Core;

    public partial class Achievements : PhoneApplicationPage
    {
        public Achievements()
        {
            InitializeComponent();


        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MNDirect.GetAchievementsProvider().PlayerAchievementUnlocked += Achievements_PlayerAchievementUnlocked;
            MNDirect.GetSession().SessionStatusChanged += onStatusChanged;

            if (MNDirect.GetSession().IsUserLoggedIn())
            {
                onStatusChanged(0, 0);
            }

            if (MNDirect.GetAchievementsProvider().IsGameAchievementListNeedUpdate())
            {
                MNDirect.GetAchievementsProvider().GameAchievementListUpdated += onListUpdated;
                MNDirect.GetAchievementsProvider().DoGameAchievementListUpdate();
            }
            else
            {
                onListUpdated();
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetAchievementsProvider().PlayerAchievementUnlocked -= Achievements_PlayerAchievementUnlocked;
            MNDirect.GetSession().SessionStatusChanged -= onStatusChanged;
            MNDirect.GetAchievementsProvider().GameAchievementListUpdated -= onListUpdated;
            base.OnNavigatingFrom(e);
        }

        private void onStatusChanged(int newstatus, int oldstatus)
        {
            if(MNDirect.GetSession().IsUserLoggedIn() && oldstatus < MNConst.MN_LOGGEDIN)
            {
                if (MNDirect.GetAchievementsProvider().GetPlayerAchievementsList().Length > 0)
                {
                    Achievements_PlayerAchievementUnlocked(-1);
                }
            }
        }

        void Achievements_PlayerAchievementUnlocked(int achievementId)
        {
            MNAchievementsProvider.PlayerAchievementInfo[] user_achs =
                MNDirect.GetAchievementsProvider().GetPlayerAchievementsList();
            var full_user_achs = 
                user_achs.Select(
                                playerAchievementInfo => MNDirect.GetAchievementsProvider().FindGameAchievementById(playerAchievementInfo.Id)
                                ).ToList();
            user_achievements_list.ItemsSource = full_user_achs;
        }

        private void unlockAchievement(object sender, RoutedEventArgs e)
        {
            try
            {
                MNDirect.GetAchievementsProvider().UnlockPlayerAchievement(Int32.Parse(achievement_id.Text));
            }
            catch (FormatException)
            {
                Console.WriteLine("Wrong format");
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Wrong value (null)");
            }
            catch (OverflowException)
            {
                Console.WriteLine("Value too big for integer");
            }
        }

        private void onListUpdated()
        {
            MNAchievementsProvider.GameAchievementInfo[] rawAchievements = MNDirect.GetAchievementsProvider().GetGameAchievementsList();
            achievements_list.ItemsSource = rawAchievements;
        }
    }
}