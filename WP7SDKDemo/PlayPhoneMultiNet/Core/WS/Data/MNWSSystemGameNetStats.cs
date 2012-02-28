//
//  MNWSSystemGameNetStats.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSSystemGameNetStats : MNWSGenericItem
   {
    public long? GetServTotalUsers ()
     {
      return GetLongValue("serv_total_users");
     }

    public long? GetServTotalGames ()
     {
      return GetLongValue("serv_total_games");
     }

    public long? GetServOnlineUsers ()
     {
      return GetLongValue("serv_online_users");
     }

    public long? GetServOnlineRooms ()
     {
      return GetLongValue("serv_online_rooms");
     }

    public long? GetServOnlineGames ()
     {
      return GetLongValue("serv_online_games");
     }

    public long? GetGameOnlineUsers ()
     {
      return GetLongValue("game_online_users");
     }

    public long? GetGameOnlineRooms ()
     {
      return GetLongValue("game_online_rooms");
     }
   }
 }
