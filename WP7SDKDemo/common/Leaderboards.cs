using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using PlayPhone.MultiNet.Core.WS;
using PlayPhone.MultiNet.Core.WS.Data;
using System.Windows.Media.Imaging;

namespace WP7SDKDemo.common
{
    public class Complexity
    {
        public const int Default = 0;
        public const int Simple = 1;
        public const int Advanced = 2;

        public string Name { get; set; }
        public int Value { get; set; }

        public Complexity(int value, string name)
        {
            Name = name;
            Value = value;
        }

        public static List<Complexity> getFilledList()
        {
            List<Complexity> ret = new List<Complexity>
                                       {
                                           new Complexity(Default, "Default"),
                                           new Complexity(Simple, "Simple"),
                                           new Complexity(Advanced, "Advanced")
                                       };
            return ret;
        }
    }

    public class Scope
    {
        public const int Global = MNWSRequestContent.LEADERBOARD_SCOPE_GLOBAL;
        public const int Local = MNWSRequestContent.LEADERBOARD_SCOPE_LOCAL;

        public int Index { get; set; }
        public string Name { get; set; }

        public Scope(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public static List<Scope> getFilledList()
        {
            List<Scope> ret = new List<Scope>
                                  {
                                      new Scope(Global, "Global"), 
                                      new Scope(Local, "Local")
                                  };
            return ret;
        }
    }

    public class Period
    {
        public const int Week = MNWSRequestContent.LEADERBOARD_PERIOD_THIS_WEEK;
        public const int Month = MNWSRequestContent.LEADERBOARD_PERIOD_THIS_MONTH;
        public const int All = MNWSRequestContent.LEADERBOARD_PERIOD_ALL_TIME;

        public int Index { get; set; }
        public string Name { get; set; }

        public Period(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public static List<Period> getFilledList()
        {
            List<Period> ret = new List<Period>
                                   {
                                       new Period(Week, "Week"),
                                       new Period(Month, "Month"),
                                       new Period(All, "All periods")
                                   };
            return ret;
        }
    }

    public class LeaderListItem
    {
        public MNWSLeaderboardListItem Source { get; internal set; }
        public BitmapImage Avatar { get; internal set; }
        public string UserName { get; internal set; }
        public string Score { get; internal set; }

        public LeaderListItem(MNWSLeaderboardListItem item)
        {
            Source = item;
            Avatar = new BitmapImage(new Uri(item.GetUserAvatarUrl()));
            UserName = item.GetUserNickName();
            Score = item.GetOutHiScore().ToString();
        }
    }
}
