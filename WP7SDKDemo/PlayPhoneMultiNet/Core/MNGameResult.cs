//
//  MNGameResult.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

namespace PlayPhone.MultiNet.Core
 {
  public class MNGameResult
   {
    public long   Score           { get; set; }
    public string ScorePostLinkId { get; set; }
    public int    GameSetId       { get; set; }

    public MNGameResult (MNGameParams gameParams)
     {
      Score = 0;

      if (gameParams != null)
       {
        ScorePostLinkId = gameParams.ScorePostLinkId;
        GameSetId       = gameParams.GameSetId;
       }
      else
       {
        ScorePostLinkId = null;
        GameSetId       = MNGameParams.MN_GAMESET_ID_DEFAULT;
       }
     }
   }
 }
