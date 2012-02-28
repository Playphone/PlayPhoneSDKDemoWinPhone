//
//  MNWSBuddyListItem.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSBuddyListItem : MNWSGenericItem
   {
    public long? GetFriendUserId ()
     {
      return GetLongValue("friend_user_id");
     }

    public string GetFriendUserNickName ()
     {
      return GetValueByName("friend_user_nick_name");
     }

    public string GetFriendSnIdList ()
     {
      return GetValueByName("friend_sn_id_list");
     }

    public string GetFriendSnUserAsnIdList ()
     {
      return GetValueByName("friend_sn_user_asnid_list");
     }

    public int? GetFriendInGameId ()
     {
      return GetIntValue("friend_in_game_id");
     }

    public string GetFriendInGameName ()
     {
      return GetValueByName("friend_in_game_name");
     }

    public string GetFriendInGameIconUrl ()
     {
      return GetValueByName("friend_in_game_icon_url");
     }

    public bool? GetFriendHasCurrentGame ()
     {
      return GetBooleanValue("friend_has_current_game");
     }

    public string GetFriendUserLocale ()
     {
      return GetValueByName("friend_user_locale");
     }

    public string GetFriendUserAvatarUrl ()
     {
      return GetValueByName("friend_user_avatar_url");
     }

    public bool? GetFriendUserOnlineNow ()
     {
      return GetBooleanValue("friend_user_online_now");
     }

    public int? GetFriendUserSfid ()
     {
      return GetIntValue("friend_user_sfid");
     }

    public int? GetFriendSnId ()
     {
      return GetIntValue("friend_sn_id");
     }

    public long? GetFriendSnUserAsnId ()
     {
      return GetLongValue("friend_sn_user_asnid");
     }

    public uint? GetFriendFlags ()
     {
      return GetUIntValue("friend_flags");
     }

    public bool? GetFriendIsIgnored ()
     {
      return GetBooleanValue("friend_is_ignored");
     }

    public int? GetFriendInRoomSfid ()
     {
      return GetIntValue("friend_in_room_sfid");
     }

    public bool? GetFriendInRoomIsLobby ()
     {
      return GetBooleanValue("friend_in_room_is_lobby");
     }

    public string GetFriendCurrGameAchievementsList ()
     {
      return GetValueByName("friend_curr_game_achievemenets_list");
     }
   }
 }
