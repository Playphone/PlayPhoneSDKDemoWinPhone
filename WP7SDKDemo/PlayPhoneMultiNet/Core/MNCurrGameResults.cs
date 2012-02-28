//
//  MNCurrGameResults.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

namespace PlayPhone.MultiNet.Core
 {
  public class MNCurrGameResults
   {
    public int          GameId { get; set; }
    public int          GameSetId { get; set; }
    public bool         FinalResult { get; set; }
    public long         PlayRoundNumber { get; set; }
    public int[]        UserPlaces { get; set; }
    public long[]       UserScores { get; set; }
    public MNUserInfo[] Users { get; set; }

    public MNCurrGameResults (int gameId, int gameSetId, bool finalResult,
                              long playRoundNumber, int[] userPlaces,
                              long[] userScores, MNUserInfo[] users)
     {
      GameId          = gameId;
      GameSetId       = gameSetId;
      FinalResult     = finalResult;
      PlayRoundNumber = playRoundNumber;
      UserPlaces      = userPlaces;
      UserScores      = userScores;
      Users           = users;
     }
   }
 }
