//
//  MNMyHiScoresProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNMyHiScoresProvider
   {
    public const uint MN_HS_PERIOD_MASK_ALLTIME = 0x0001;
    public const uint MN_HS_PERIOD_MASK_WEEK    = 0x0002;
    public const uint MN_HS_PERIOD_MASK_MONTH   = 0x0004;

    public delegate void NewHiScoreEventHandler (long newScore, int gameSetId, uint periodMask);

    public event NewHiScoreEventHandler NewHiScore;

    public MNMyHiScoresProvider (MNSession session)
     {
      this.session = session;

      scores = new Dictionary<int,long>();

      session.PluginMessageReceived += OnSessionPluginMessageReceived;
     }

    public void Shutdown ()
     {
      session.PluginMessageReceived -= OnSessionPluginMessageReceived;
     }

    public long? GetMyHiScore (int gameSetId)
     {
      long result;

      if (scores.TryGetValue(gameSetId,out result))
       {
        return result;
       }
      else
       {
        return null;
       }
     }

    public IDictionary<int,long> GetMyHiScores ()
     {
      return new Dictionary<int,long>(scores);
     }

    private void ProcessInitMessage (string message)
     {
      scores.Clear();

      string[] entries = message.Split(';');

      foreach (string entry in entries)
       {
        string[] parts = entry.Split(':');

        if (parts.Length == 2)
         {
          int  gameSetId;
          long score;

          if (int.TryParse(parts[0],out gameSetId) &&
              long.TryParse(parts[1],out score))
           {
            scores[gameSetId] = score;
           }
         }
       }
     }

    private void ProcessModifyMessage (string message)
     {
      string[] parts = message.Split(':');

      if (parts.Length != 3)
       {
        return;
       }

      int  gameSetId;
      long score;

      if (!int.TryParse(parts[0],out gameSetId) ||
          !long.TryParse(parts[1],out score))
       {
        return;
       }

      uint periodMask = 0;

      foreach (char periodChar in parts[2])
       {
        if (periodChar == 'W')
         {
          periodMask |= MN_HS_PERIOD_MASK_WEEK;
         }
        else if (periodChar == 'M')
         {
          periodMask |= MN_HS_PERIOD_MASK_MONTH;
         }
        else if (periodChar == 'A')
         {
          periodMask |= MN_HS_PERIOD_MASK_ALLTIME;
         }
       }

      if ((periodMask & MN_HS_PERIOD_MASK_ALLTIME) != 0)
       {
        scores[gameSetId] = score;
       }

      NewHiScoreEventHandler handler = NewHiScore;

      if (handler != null)
       {
        handler(score,gameSetId,periodMask);
       }
     }

    private void OnSessionPluginMessageReceived (string     pluginName,
                                                 string     message,
                                                 MNUserInfo sender)
     {
      if (pluginName != PROVIDER_NAME)
       {
        return;
       }

      if (message.Length == 0)
       {
        return;
       }

      char cmdChar = message[0];

      if      (cmdChar == 'i')
       {
        ProcessInitMessage(message.Substring(1));
       }
      else if (cmdChar == 'm')
       {
        ProcessModifyMessage(message.Substring(1));
       }
     }

    private const string PROVIDER_NAME = "com.playphone.mn.scorenote";

    private MNSession            session;
    private Dictionary<int,long> scores;
   }
 }

