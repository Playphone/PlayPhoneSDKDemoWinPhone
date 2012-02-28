//
//  MNWSAnyGameItem.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSAnyGameItem : MNWSGenericItem
   {
    public int? GetGameId ()
     {
      return GetIntValue("game_id");
     }

    public string GetGameName ()
     {
      return GetValueByName("game_name");
     }

    public string GetGameDesc ()
     {
      return GetValueByName("game_desc");
     }

    public int? GetGameGenreId ()
     {
      return GetIntValue("gamegenre_id");
     }

    public uint? GetGameFlags ()
     {
      return GetUIntValue("game_flags");
     }

    public int? GetGameStatus ()
     {
      return GetIntValue("game_status");
     }

    public int? GetGamePlayModel ()
     {
      return GetIntValue("game_play_model");
     }

    public string GetGameIconUrl ()
     {
      return GetValueByName("game_icon_url");
     }

    public long? GetDeveloperId ()
     {
      return GetLongValue("developer_id");
     }
   }
 }
