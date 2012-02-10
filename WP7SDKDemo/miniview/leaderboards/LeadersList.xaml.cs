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
using System.Windows.Navigation;
using PlayPhone.MultiNet;
using PlayPhone.MultiNet.Providers;
using WP7SDKDemo.common;
using PlayPhone.MultiNet.Core.WS.Data;

namespace WP7SDKDemo.miniview.leaderboards
{
    public partial class LeadersList : PhoneApplicationPage
    {
        public const int Simple = 1;
        public const int Game = 2;
        public const int User = 3;

        public LeadersList()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            IDictionary<string, string> parameters = this.NavigationContext.QueryString;
            int type = Int32.Parse(parameters["Type"]);
            switch (type)
            {
                case Simple:
                    loadSimple(parameters);
                    break;

                case Game:
                    loadGame(parameters);
                    break;

                case User:
                    loadUser(parameters);
                    break;
            }

            base.OnNavigatedTo(args);
        }

        private void loadSimple(IDictionary<string, string> parameters)
        {
            int complexity = Int32.Parse(parameters["Complexity"]);
            int scope = Int32.Parse(parameters["Scope"]);
            int period = Int32.Parse(parameters["Period"]);

            MNDirect.SetDefaultGameSetId(complexity);

            MNWSInfoRequestLeaderboard req = new MNWSInfoRequestLeaderboard(
                                                    new MNWSInfoRequestLeaderboard.LeaderboardModeCurrentUser(scope, period),
                                                    OnCompleted);
            MNDirect.GetWSProvider().Send(req);
        }


        private void loadGame(IDictionary<string, string> parameters)
        {
            long user_id = Int32.Parse(parameters["UserId"]);
            int game_id = Int32.Parse(parameters["GameId"]);
            int gameset_id = Int32.Parse(parameters["GameSetId"]);
            int period = Int32.Parse(parameters["Period"]);

            MNDirect.SetDefaultGameSetId(gameset_id);

            MNWSInfoRequestLeaderboard req = new MNWSInfoRequestLeaderboard(
                                        new MNWSInfoRequestLeaderboard.LeaderboardModeAnyUserAnyGameGlobal(user_id, game_id, gameset_id, period),
                                        OnCompleted);
            MNDirect.GetWSProvider().Send(req);
        }

        private void loadUser(IDictionary<string, string> parameters)
        {
            int game_id = Int32.Parse(parameters["GameId"]);
            int gameset_id = Int32.Parse(parameters["GameSetId"]);
            int period = Int32.Parse(parameters["Period"]);
            int scope = Int32.Parse(parameters["Scope"]);

            MNDirect.SetDefaultGameSetId(gameset_id);
            if (scope == Scope.Global)
            {
                MNWSInfoRequestLeaderboard req = new MNWSInfoRequestLeaderboard(
                                        new MNWSInfoRequestLeaderboard.LeaderboardModeAnyGameGlobal(game_id, gameset_id, period),
                                        OnCompleted);
                MNDirect.GetWSProvider().Send(req);
            }
            else
            {
                MNWSInfoRequestLeaderboard req = new MNWSInfoRequestLeaderboard(
                                        new MNWSInfoRequestLeaderboard.LeaderboardModeCurrUserAnyGameLocal(game_id, gameset_id, period),
                                        OnCompleted);
                MNDirect.GetWSProvider().Send(req);
            }
        }

        public void OnCompleted(MNWSInfoRequestLeaderboard.RequestResult result)
        {
            if (!result.HadError)
            {
                List<LeaderListItem> leaders = result.GetDataEntry().Select(item => new LeaderListItem(item)).ToList();
                scores.ItemsSource = leaders;
            }
            else
            {
                MessageBox.Show(result.ErrorMessage);
            }
        }
    }
}