//
//  MNWSRequestContent.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Text;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core.WS
 {
  public class MNWSRequestContent
   {
    public const int LEADERBOARD_PERIOD_ALL_TIME   = 0;
    public const int LEADERBOARD_PERIOD_THIS_WEEK  = 1;
    public const int LEADERBOARD_PERIOD_THIS_MONTH = 2;

    public const int LEADERBOARD_SCOPE_GLOBAL = 0;
    public const int LEADERBOARD_SCOPE_LOCAL  = 1;

    public MNWSRequestContent ()
     {
      infoList = new StringBuilder();
     }

    public string AddInfoBlock (string infoBlockSelector)
     {
      if (infoList.Length > 0)
       {
        infoList.Append(',');
       }

      infoList.Append(infoBlockSelector);

      return infoBlockSelector;
     }

    protected string AddInfoBlock (string blockName, string param1)
     {
      AddInfoBlock(blockName + ":" + param1);

      return blockName;
     }

    protected string AddInfoBlock (string blockName, string param1, string param2)
     {
      AddInfoBlock(blockName + ":" + param1 + ":" + param2);

      return blockName;
     }

    protected string AddInfoBlock (string blockName, string param1, string param2, string param3)
     {
      AddInfoBlock(blockName + ":" + param1 + ":" + param2 + ":" + param3);

      return blockName;
     }

    public void AddNameMapping (string blockName, string parserName)
     {
      if (mapping == null)
       {
        mapping = new Dictionary<string,string>();
       }

      mapping[blockName] = parserName;
     }

    public string AddSystemGameNetStats ()
     {
      return AddInfoBlock("systemGameNetStats");
     }

    public string AddCurrentUserInfo ()
     {
      return AddInfoBlock("currentUser");
     }

    public string AddCurrUserBuddyList ()
     {
      return AddInfoBlock("currentUserBuddyList");
     }

    public string AddAnyUser (long userId)
     {
      return AddInfoBlock("anyUser",userId.ToString());
     }

    public string AddAnyGame (int gameId)
     {
      return AddInfoBlock("anyGame",gameId.ToString());
     }

    public string AddCurrGameRoomList ()
     {
      return AddInfoBlock("currentGameRoomList");
     }

    public string AddCurrGameRoomUserList (int roomSFId)
     {
      return AddInfoBlock("currentGameRoomUserList",roomSFId.ToString());
     }

    private static string GetPeriodNameByCode (int period)
     {
      if      (period == LEADERBOARD_PERIOD_THIS_WEEK)
       {
        return "ThisWeek";
       }
      else if (period == LEADERBOARD_PERIOD_THIS_MONTH)
       {
        return "ThisMonth";
       }
      else
       {
        return "AllTime";
       }
     }

    private static String GetScopeNameByCode (int scope)
     {
      if (scope == LEADERBOARD_SCOPE_LOCAL)
       {
        return "Local";
       }
      else
       {
        return "Global";
       }
     }

    public string AddCurrUserLeaderboard (int scope, int period)
     {
      return AddInfoBlock("currentUserLeaderboard" +
                          GetScopeNameByCode(scope) +
                          GetPeriodNameByCode(period));
     }

    public string AddAnyGameLeaderboardGlobal (int gameId, int gameSetId, int period)
     {
      return AddInfoBlock("anyGameLeaderboardGlobal" + GetPeriodNameByCode(period),
                           gameId.ToString(),gameSetId.ToString());
     }

    public string AddAnyUserAnyGameLeaderboardGlobal (long userId, int gameId, int gameSetId, int period)
     {
      return AddInfoBlock("anyUserAnyGameLeaderboardGlobal" +
                           GetPeriodNameByCode(period),
                            userId.ToString(),gameId.ToString(),gameSetId.ToString());
     }

    public string AddCurrUserAnyGameLeaderboardLocal (int gameId, int gameSetId, int period)
     {
      return AddInfoBlock("currentUserAnyGameLeaderboardLocal" +
                           GetPeriodNameByCode(period),gameId.ToString(),gameSetId.ToString());
     }

    public string AddAnyUserGameCookies (long[] userIdList, int[] cookieKeyList)
     {
      MNStringJoiner userIds    = new MNStringJoiner("^");
      MNStringJoiner cookieKeys = new MNStringJoiner("^");

      foreach (long userId in userIdList)
       {
        userIds.Join(userId.ToString());
       }

      foreach (int cookieKey in cookieKeyList)
       {
        cookieKeys.Join(cookieKey.ToString());
       }

      return AddInfoBlock("anyUserGameCookies",userIds.ToString(),cookieKeys.ToString());
     }

    public string AddCurrUserSubscriptionStatus (int socNetId)
     {
      return AddInfoBlock("currentUserSubscriptionStatus",socNetId.ToString());
     }

    public string AddCurrUserSubscriptionStatusPlayPhone ()
     {
      return AddCurrUserSubscriptionStatus(SN_ID_PLAYPHONE);
     }

    public string AddGetSessionSignedClientToken (String payload)
     {
      return AddInfoBlock("getSessionSignedClientToken",Uri.EscapeUriString(payload));
     }

    internal string GetRequestInfoListString ()
     {
      return infoList.ToString();
     }

    internal Dictionary<string,string> GetMapping ()
     {
      return mapping;
     }

    private StringBuilder             infoList;
    private Dictionary<string,string> mapping;

    public const int SN_ID_PLAYPHONE = 4;
   }
 }
