//
//  MNWSRoomListItem.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSRoomListItem : MNWSGenericItem
   {
    public int? GetRoomSFId ()
     {
      return GetIntValue("room_sfid");
     }

    public string GetRoomName ()
     {
      return GetValueByName("room_name");
     }

    public int? GetRoomUserCount ()
     {
      return GetIntValue("room_user_count");
     }

    public bool? GetRoomIsLobby ()
     {
      return GetBooleanValue("room_is_lobby");
     }

    public int? GetGameId ()
     {
      return GetIntValue("game_id");
     }

    public int? GetGameSetId ()
     {
      return GetIntValue("gameset_id");
     }
   }
 }
