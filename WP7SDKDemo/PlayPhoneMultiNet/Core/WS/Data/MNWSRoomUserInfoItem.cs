//
//  MNWSRoomUserInfoItem.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSRoomUserInfoItem : MNWSGenericItem
   {
    public int? GetRoomSFId ()
     {
      return GetIntValue("room_sfid");
     }

    public long? GetUserId ()
     {
      return GetLongValue("user_id");
     }

    public string GetUserNickName ()
     {
      return GetValueByName("user_nick_name");
     }

    public bool? GetUserAvatarExists ()
     {
      return GetBooleanValue("user_avatar_exists");
     }

    public string GetUserAvatarUrl ()
     {
      return GetValueByName("user_avatar_url");
     }

    public bool? GetUserOnlineNow ()
     {
      return GetBooleanValue("user_online_now");
     }
   }
 }
