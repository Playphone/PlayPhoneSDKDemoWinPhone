//
//  MNScoreProgressProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using PlayPhone.MultiNet.Core;
using System.Collections.Generic;
using System.Windows.Threading;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNScoreProgressProvider
   {
    public delegate void ScoresUpdatedEventHandler (ScoreItem[] scoreBoard);

    public event ScoresUpdatedEventHandler ScoresUpdated;

    public class ScoreItem
     {
      public MNUserInfo UserInfo { get; set; }
      public long       Score    { get; set; }
      public int        Place    { get; set; }

      public ScoreItem (MNUserInfo userInfo, long score, int place)
       {
        UserInfo = userInfo;
        Score    = score;
        Place    = place;
       }
     }

    public MNScoreProgressProvider (MNSession session) : this(session,0,0)
     {
     }

    public MNScoreProgressProvider (MNSession session, int refreshInterval, int updateDelay)
     {
      this.session = session;

      if (refreshInterval <= 0)
       {
        scoreProgress = new ScoreProgressASync(this);
       }
      else
       {
        scoreProgress = new ScoreProgressSync(this,refreshInterval,updateDelay);
       }
     }

    public void Shutdown ()
     {
      if (scoreProgress != null)
       {
        scoreProgress.Stop();
       }
     }

    public void SetRefreshIntervalAndUpdateDelay (int refreshInterval,
                                                  int updateDelay)
     {
      if (scoreProgress.Running)
       {
        return;
       }

      Stop();

      if (refreshInterval <= 0)
       {
        scoreProgress = new ScoreProgressASync(this);
       }
      else
       {
        scoreProgress = new ScoreProgressSync(this,refreshInterval,updateDelay);
       }
     }

    public void Start ()
     {
      scoreProgress.Start();
     }

    public void Stop ()
     {
      scoreProgress.Stop();
     }

    public void SetScoreComparator (IComparer<ScoreItem> comparator)
     {
      scoreProgress.SetScoreComparator(comparator);
     }

    public void PostScore (long score)
     {
      scoreProgress.PostScore(score);
     }

    public class ScoreComparatorMoreIsBetter : IComparer<ScoreItem>
     {
      public int Compare (ScoreItem item1, ScoreItem item2)
       {
        if (item2.Score == item1.Score)
         {
          return 0;
         }
        else if (item2.Score > item1.Score)
         {
          return 1;
         }
        else
         {
          return -1;
         }
       }
     }

    public class ScoreComparatorLessIsBetter : IComparer<ScoreItem>
     {
      public int Compare (ScoreItem item1, ScoreItem item2)
       {
        if (item1.Score == item2.Score)
         {
          return 0;
         }
        else if (item1.Score > item2.Score)
         {
          return 1;
         }
        else
         {
          return -1;
         }
       }
     }

    private class ScoreStateSlice
     {
      public ScoreStateSlice ()
       {
        scores = new Dictionary<int,ScoreItem>();
       }

      public void Clear ()
       {
        scores.Clear();
       }

      public void UpdateUser (MNUserInfo userInfo, long score)
       {
        ScoreItem item = new ScoreItem(userInfo,score,0);

        scores[userInfo.UserSFId] = item;
       }

      public ScoreItem[] GetSortedScores (IComparer<ScoreItem> comparator)
       {
        int         count  = scores.Values.Count;
        ScoreItem[] result = new ScoreItem[count];

        if (count > 0)
         {
          scores.Values.CopyTo(result,0);

          Array.Sort(result,comparator);

          ScoreItem item = result[0];

          long score = item.Score;
          int  place = 1;

          for (int index = 1; index < count; index++)
           {
            item = result[index];

            if (item.Score != score)
             {
              place++;
              score = item.Score;
             }

            item.Place = place;
           }
         }

        return result;
       }

      Dictionary<int,ScoreItem> scores;
     }

    private abstract class ScoreProgress
     {
      public bool Running { get; set; }

      public ScoreProgress (MNScoreProgressProvider parent)
       {
        this.parent          = parent;
        this.scoreComparator = null;

        Running   = false;
        startTime = 0;
       }

      public void SetScoreComparator (IComparer<ScoreItem> comparator)
       {
        scoreComparator = comparator ?? new ScoreComparatorMoreIsBetter();
       }

      public virtual void Start ()
       {
        if (Running)
         {
          Stop();
         }

        if (scoreComparator == null)
         {
          scoreComparator = new ScoreComparatorMoreIsBetter();
         }

        startTime = GetCurrentTimeInMillis();

        parent.session.PluginMessageReceived += OnSessionPluginMessageReceived;
       }

      public virtual void Stop ()
       {
        if (Running)
         {
          parent.session.PluginMessageReceived -= OnSessionPluginMessageReceived;
         }
       }

      public abstract void PostScore (long score);
      public abstract void OnScoreUpdateReceived (MNUserInfo userInfo,
                                                  long       score,
                                                  long       scoreTime);

      protected void DispatchScoreChangedEvent (ScoreItem[] scoreBoard)
       {
        ScoresUpdatedEventHandler handler = parent.ScoresUpdated;

        if (handler != null)
         {
          handler(scoreBoard);
         }
       }

      protected void SendScore (MNUserInfo userInfo, long score, long scoreTime)
       {
        string message = scoreTime.ToString() + ':' + score.ToString();

        parent.session.SendPluginMessage
         (MNScoreProgressProvider.PROVIDER_NAME,message);
       }

      private void OnSessionPluginMessageReceived (string     pluginName,
                                                   string     message,
                                                   MNUserInfo sender)
       {
        if (!Running || sender == null || pluginName != MNScoreProgressProvider.PROVIDER_NAME)
         {
          return;
         }

        string[] components = message.Split(':');

        if (components.Length != 2)
         {
          return;
         }

        long? scoreTime = MNUtils.ParseLong(components[0]);
        long? score     = MNUtils.ParseLong(components[1]);

        if (scoreTime == null || score == null)
         {
          return;
         }

        OnScoreUpdateReceived(sender,score.Value,scoreTime.Value);
       }

      protected static long GetCurrentTimeInMillis ()
       {
        return DateTime.Now.Ticks / 10000;
       }

      protected MNScoreProgressProvider parent;
      protected long                    startTime;
      protected IComparer<ScoreItem>    scoreComparator;
     }

    private class ScoreProgressASync : ScoreProgress
     {
      public ScoreProgressASync (MNScoreProgressProvider parent) : base(parent)
       {
        scoreState = new ScoreStateSlice();
       }

      public override void Start ()
       {
        scoreState.Clear();

        base.Start();

        Running = true;
       }

      public override void Stop ()
       {
        base.Stop();

        Running = false;

        scoreState.Clear();
       }

      public override void PostScore (long score)
       {
        if (!Running || !parent.session.IsInGameRoom())
         {
          return;
         }

        long scoreTime = GetCurrentTimeInMillis() - startTime;

        MNUserInfo myUserInfo = parent.session.GetMyUserInfo();

        if (myUserInfo == null)
         {
          return; // we're not logged in
         }

        scoreState.UpdateUser(myUserInfo,score);

        DispatchScoreChangedEvent(scoreState.GetSortedScores(scoreComparator));

        SendScore(myUserInfo,score,scoreTime);
       }

      public override void OnScoreUpdateReceived (MNUserInfo userInfo,
                                                  long       score,
                                                  long       scoreTime)
       {
        scoreState.UpdateUser(userInfo,score);

        DispatchScoreChangedEvent(scoreState.GetSortedScores(scoreComparator));
       }

      private ScoreStateSlice scoreState;
     }

    private class ScoreProgressSync : ScoreProgress
     {
      public ScoreProgressSync (MNScoreProgressProvider parent,
                                int                     refreshInterval,
                                int                     updateDelay) : base(parent)
       {
        scoreSlices = new ScoreStateSlice[SLICES_COUNT];

        for (int index = 0; index < SLICES_COUNT; index++)
         {
          scoreSlices[index] = new ScoreStateSlice();
         }

        this.refreshInterval = refreshInterval > MIN_REFRESH_INTERVAL ?
                               refreshInterval : MIN_REFRESH_INTERVAL;

        this.updateDelay = updateDelay > 0 ?
                           updateDelay : this.refreshInterval / 3;

        postScoreTimer   = null;
        updateScoreTimer = null;

        currentScore = 0;
       }

      public void OnPostScoreTimerFired (object sender, EventArgs args)
       {
        if (!Running || !parent.session.IsInGameRoom())
         {
          return;
         }

        long scoreTime = GetCurrentTimeInMillis() - startTime +
                          refreshInterval / 2;

        scoreTime = (scoreTime / refreshInterval) * refreshInterval;

        MNUserInfo myUserInfo = parent.session.GetMyUserInfo();

        if (myUserInfo == null)
         {
          return; // we're not logged in
         }

        SendScore(myUserInfo,currentScore,scoreTime);

        if (scoreTime < baseTime)
         {
          return;
         }

        int index;
        int offset = (int)((scoreTime - baseTime) / refreshInterval);

        if (offset < SLICES_COUNT)
         {
          if (offset > 0)
           {
            for (index = offset; index < SLICES_COUNT; index++)
             {
              scoreSlices[index - offset] = scoreSlices[index];
             }

            for (index = SLICES_COUNT - offset; index < SLICES_COUNT; index++)
             {
              scoreSlices[index] = new ScoreStateSlice();
             }
           }
         }
        else
         {
          ClearSlices();
         }

        baseTime = scoreTime;

        scoreSlices[0].UpdateUser(myUserInfo,currentScore);

        if (updateScoreTimer == null)
         {
          updateScoreTimer = new DispatcherTimer();
          updateScoreTimer.Tick += (eventSender,eventArgs) =>
           {
            DispatchScoreChangedEvent
             (scoreSlices[0].GetSortedScores(scoreComparator));

            updateScoreTimer.Stop();
            updateScoreTimer = null;
           };

          updateScoreTimer.Interval = new TimeSpan(0,0,0,0,updateDelay);
         }
       }

      public override void Start ()
       {
        ClearSlices();

        base.Start();

        postScoreTimer = new DispatcherTimer();
        postScoreTimer.Tick += OnPostScoreTimerFired;
        postScoreTimer.Interval = new TimeSpan(0,0,0,0,refreshInterval);

        currentScore = 0;
        baseTime     = 0;
        Running      = true;

        postScoreTimer.Start();
       }

      public override void Stop ()
       {
        base.Stop();

        if (!Running)
         {
          return;
         }

        Running = false;

        postScoreTimer.Stop();
        postScoreTimer = null;

        if (updateScoreTimer != null)
         {
          updateScoreTimer.Stop();
          updateScoreTimer = null;
         }

        ClearSlices();
       }

      public override void PostScore (long score)
       {
        currentScore = score;
       }

      public override void OnScoreUpdateReceived (MNUserInfo userInfo,
                                                  long       score,
                                                  long       scoreTime)
       {
        if (!Running || !parent.session.IsInGameRoom())
         {
          return;
         }

        if (scoreTime < baseTime)
         {
          return; /* too late for this score */
         }

        int sliceIndex = (int)((scoreTime - baseTime) / refreshInterval);

        if (sliceIndex >= SLICES_COUNT)
         {
          return; /* too far in the future */
         }

        scoreSlices[sliceIndex].UpdateUser(userInfo,score);
       }

      private void ClearSlices ()
       {
        for (int index = 0; index < SLICES_COUNT; index++)
         {
          scoreSlices[index].Clear();
         }
       }

      private ScoreStateSlice[] scoreSlices;

      private long  currentScore;
      private long  baseTime;

      private int   refreshInterval;
      private int   updateDelay;

      private DispatcherTimer postScoreTimer;
      private DispatcherTimer updateScoreTimer;

      private const int SLICES_COUNT = 4;
      private const int MIN_REFRESH_INTERVAL = 500;
     }

    private MNSession     session;
    private ScoreProgress scoreProgress;

    private const string PROVIDER_NAME = "com.playphone.mn.ps1";
   }
 }
