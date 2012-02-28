//
//  MNWSCurrentUserInfo.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSCurrentUserInfo : MNWSGenericItem
   {
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

    public string GetUserEmail ()
     {
      return GetValueByName("user_email");
     }

    public int? GetUserStatus ()
     {
      return GetIntValue("user_status");
     }

    public bool? GetUserAvatarHasCustomImg ()
     {
      return GetBooleanValue("user_avatar_has_custom_img");
     }

    public bool? GetUserAvatarHasExternalUrl ()
     {
      return GetBooleanValue("user_avatar_has_external_url");
     }

    public int? GetUserGamePoints ()
     {
      return GetIntValue("user_gamepoints");
     }
   }
 }
