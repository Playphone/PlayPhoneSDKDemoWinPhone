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
using PlayPhone.MultiNet.Core;
using System.Windows.Threading;

namespace WP7SDKDemo.views
{
    public partial class Multinet : PhoneApplicationPage
    {
        private const int GameTime = 60;//seconds
        private DispatcherTimer counter;
        private int tickNum = GameTime;
        private int totalScore = 0;

        public Multinet()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MNDirect.GetSession().SessionStatusChanged += onSessionStatusChangedHandler;
            MNDirect.DoFinishGame += onDoFinishGameHandler;
            MNDirect.DoCancelGame += onDoCancelGameHandler; base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MNDirect.GetSession().SessionStatusChanged -= onSessionStatusChangedHandler;
            MNDirect.DoFinishGame -= onDoFinishGameHandler;
            MNDirect.DoCancelGame -= onDoCancelGameHandler;
            base.OnNavigatingFrom(e);
        }

        private void onDecrease(object sender, RoutedEventArgs e)
        {
            totalScore -= 10;
        }

        private void onIncrease(object sender, RoutedEventArgs e)
        {
            totalScore += 10;
        }

        private void onSessionStatusChangedHandler(int newStatus, int oldStatus)
        {
            MNSession activeSession = MNDirect.GetSession();
            int sessionStatus = activeSession.GetStatus();
            int userStatus = activeSession.GetRoomUserStatus();

            MNDebug.debug("Multinet.onSessionStatusChangedHandler (" + sessionStatus.ToString() + " : " + userStatus.ToString() + ")");

            if ((sessionStatus == MNConst.MN_OFFLINE) || (sessionStatus == MNConst.MN_CONNECTING))
            {
                tip.Text = "Player should be logged in to use Multiplayer features. Please open PlayPhone " +
                           "dashboard and login to PlayPhone network";
            }
            else if (sessionStatus == MNConst.MN_LOGGEDIN)
            {
                tip.Text = "Please open PlayPhone dashboard and press PlayNow button to start Multiplater game.";
            }
            else
            {
                if (userStatus == MNConst.MN_USER_CHATER)
                {
                    tip.Text = "Currently you are \"CHATTER\". Please wait for end of current game round and then press " +
                               "\"Play next round\" button on PPS Dashboard.";
                }

                if (sessionStatus == MNConst.MN_IN_GAME_WAIT)
                {
                    tip.Text = "Waiting for opponents";
                }
                else if (sessionStatus == MNConst.MN_IN_GAME_START)
                {
                    tip.Text = "Starting the game";
                }
                else if (sessionStatus == MNConst.MN_IN_GAME_PLAY)
                {
                    tip.Text = "Use buttons to change your score. You will see the progress on the top indicator.";

                    MNDirectUIHelper.HideDashboard();
                    MNDirectButton.Show();

                    counter = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1000)};
                    counter.Tick += countdown_timerHandler;
                    tickNum = GameTime;
                    timer_display.Text = tickNum.ToString();
                    totalScore = 0;
                    counter.Start();
                }
                else if (sessionStatus == MNConst.MN_IN_GAME_END)
                {
                    tip.Text = "Posting the scores.\nYou can use \"Post Score\" button to send current " +
                               "score to PPS server";
                }
                else
                {
                    tip.Text = "Undefined state: SessionState: " + sessionStatus.ToString() + " UserState: " + userStatus.ToString();
                }
            }
        }

        void countdown_timerHandler(object sender, EventArgs e)
        {
            if (tickNum > 0)
            {
                tickNum--;
                MNDebug.debug("Multinet.countdown_timerHandler (" + tickNum.ToString() + ")");
                timer_display.Text = tickNum.ToString();
                counter.Interval = TimeSpan.FromMilliseconds(1000);
                counter.Start();
            }
            else
            {
                StopCounter();
            }
        }

        private void onDoFinishGameHandler()
        {
            MNDebug.debug("Multinet.onDoFinishGameHandler");
            StopCounter();
            //             post_score.visible = true;
            totalScore = 0;
        }

        private void onDoCancelGameHandler()
        {
            MNDebug.debug("Multinet.onDoCancelGameHandler");
            StopCounter();
            totalScore = 0;
        }

        private void StopCounter()
        {
            MNDebug.debug("Multinet.StopCounter");
            if (counter != null)
            {
                counter.Stop();
                counter.Tick -= countdown_timerHandler;
                counter = null;
                timer_display.Text = "";
            }
        }

        private void joinRandomroom(object sender, RoutedEventArgs e)
        {
            MNDirect.GetSession().ReqJoinRandomRoom(MNDirect.GetSession().GetDefaultGameSetId().ToString());
        }
    }
}