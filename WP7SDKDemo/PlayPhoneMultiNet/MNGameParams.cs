//
//  MNGameParams.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet
 {
  public class MNGameParams
   {
    public const int MN_GAMESET_ID_DEFAULT = 0;

    public const int MN_PLAYMODEL_SINGLEPLAY     = 0x0000;
    public const int MN_PLAYMODEL_SINGLEPLAY_NET = 0x0100;
    public const int MN_PLAYMODEL_MULTIPLAY      = 0x1000;

    public int                        GameSetId         { get; set; }
    public string                     GameSetParams     { get; set; }
    public string                     ScorePostLinkId   { get; set; }
    public int                        GameSeed          { get; set; }
    public int                        PlayModel         { get; set; }
    public IDictionary<string,string> GameSetPlayParams
     {
      get
       {
        return GameSetPlayParams;
       }
     }

    public MNGameParams (int gameSetId, string gameSetParams,
                         string scorePostLinkId, int gameSeed, int playModel)
     {
      GameSetId         = gameSetId;
      GameSetParams     = gameSetParams;
      ScorePostLinkId   = scorePostLinkId;
      GameSeed          = gameSeed;
      gameSetPlayParams = new Dictionary<string,string>();
      PlayModel         = playModel;
     }

    public void AddGameSetPlayParam (string paramName, string paramValue)
     {
      gameSetPlayParams[paramName] = paramValue;
     }

    public string GetGameSetPlayParamByName (string paramName)
     {
      string value;

      if (gameSetPlayParams.TryGetValue(paramName,out value))
       {
        return value;
       }
      else
       {
        return null;
       }
     }

    private Dictionary<string,string> gameSetPlayParams;
   }
 }
