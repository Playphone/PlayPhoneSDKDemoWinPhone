//
//  MNWSLeaderboardListItem.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSLeaderboardListItem : MNWSGenericItem
   {
    public long? GetUserId ()
     {
      return GetLongValue("user_id");
     }

    public string GetUserNickName ()
     {
      return GetValueByName("user_nick_name");
     }

    public string GetUserAvatarUrl ()
     {
      return GetValueByName("user_avatar_url");
     }

    public bool? GetUserIsFriend ()
     {
      return GetBooleanValue("user_is_friend");
     }

    public bool? GetUserOnlineNow ()
     {
      return GetBooleanValue("user_online_now");
     }

    public int? GetUserSfid ()
     {
      return GetIntValue("user_sfid");
     }

    public bool? GetUserIsIgnored ()
     {
      return GetBooleanValue("user_is_ignored");
     }

    public string GetUserLocale ()
     {
      return GetValueByName("user_locale");
     }

    public long? GetOutHiScore ()
     {
      return GetLongValue("out_hi_score");
     }

    public string GetOutHiScoreText ()
     {
      return GetValueByName("out_hi_score_text");
     }

    public long? GetOutHiDateTime ()
     {
      return GetLongValue("out_hi_datetime");
     }

    public long? GetOutHiDateTimeDiff ()
     {
      return GetLongValue("out_hi_datetime_diff");
     }

    public long? GetOutUserPlace ()
     {
      return GetLongValue("out_user_place");
     }

    public int? GetGameId ()
     {
      return GetIntValue("game_id");
     }

    public int? GetGamesetId ()
     {
      return GetIntValue("gameset_id");
     }

    public string GetUserAchievementsList ()
     {
      return GetValueByName("user_achievemenets_list");
     }
   }
 }
