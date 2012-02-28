//
//  MNPlayerListProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

using PlayPhone.MultiNet;
using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNPlayerListProvider
   {
    public delegate void PlayerJoinEventHandler (MNUserInfo player);
    public delegate void PlayerLeftEventHandler (MNUserInfo player);

    public event PlayerJoinEventHandler PlayerJoin;
    public event PlayerLeftEventHandler PlayerLeft;

    public MNPlayerListProvider (MNSession session)
     {
      this.session = session;

      players = new Dictionary<int,MNUserInfo>();
      chaters = new Dictionary<int,MNUserInfo>();

      session.RoomUserLeave         += OnSessionRoomUserLeave;
      session.PluginMessageReceived += OnSessionPluginMessageReceived;
      session.SessionStatusChanged  += OnSessionStatusChanged;
     }

    public void Shutdown ()
     {
      session.RoomUserLeave         -= OnSessionRoomUserLeave;
      session.PluginMessageReceived -= OnSessionPluginMessageReceived;
      session.SessionStatusChanged  -= OnSessionStatusChanged;

      players.Clear();
      chaters.Clear();
     }

    public MNUserInfo[] GetPlayerList ()
     {
      ICollection<MNUserInfo> playerCollection = players.Values;

      MNUserInfo[] result = new MNUserInfo[playerCollection.Count];

      playerCollection.CopyTo(result,0);

      return result;
     }

    private void DispatchPlayerJoinEvent (MNUserInfo userInfo)
     {
      PlayerJoinEventHandler handler = PlayerJoin;

      if (handler != null)
       {
        handler(userInfo);
       }
     }

    private void DispatchPlayerLeftEvent (MNUserInfo userInfo)
     {
      PlayerLeftEventHandler handler = PlayerLeft;

      if (handler != null)
       {
        handler(userInfo);
       }
     }

    private void OnSessionRoomUserLeave (MNUserInfo userInfo)
     {
      MNUserInfo playerInfo;

      int playerSFId = userInfo.UserSFId;

      if (players.TryGetValue(playerSFId,out playerInfo))
       {
        DispatchPlayerLeftEvent(playerInfo);

        players.Remove(playerSFId);
       }
      else
       {
        chaters.Remove(playerSFId);
       }
     }

    private bool ParseSFIdStatusPair (out int sfId, out int status, string str)
     {
      int pos = str.IndexOf(':');

      sfId   = MNConst.MN_USER_SFID_UNDEFINED;
      status = MNConst.MN_USER_CHATER;

      if (pos < 0)
       {
        return false;
       }

      if (!int.TryParse(str.Substring(0,pos),out sfId))
       {
        return false;
       }

      return int.TryParse(str.Substring(pos + 1),out status);
     }

    private void ProcessInitMessage (string message)
     {
      players.Clear();
      chaters.Clear();

      string[] entries = message.Split(';');

      foreach (string entry in entries)
       {
        int sfId;
        int status;

        if (ParseSFIdStatusPair(out sfId,out status,entry))
         {
          MNUserInfo userInfo = session.GetUserInfoBySFId(sfId);

          if (userInfo != null)
           {
            players[sfId] = userInfo;
           }
         }
       }
     }

    private void ProcessModifyMessage (string message)
     {
      int sfId;
      int status;

      if (ParseSFIdStatusPair(out sfId,out status,message))
       {
        MNUserInfo playerInfo;

        if (players.TryGetValue(sfId,out playerInfo))
         {
          if (status != MNConst.MN_USER_PLAYER)
           {
            chaters[sfId] = playerInfo;
            players.Remove(sfId);

            DispatchPlayerLeftEvent(playerInfo);
           }
         }
        else if (chaters.TryGetValue(sfId,out playerInfo))
         {
          if (status == MNConst.MN_USER_PLAYER)
           {
            players[sfId] = playerInfo;
            chaters.Remove(sfId);

            DispatchPlayerJoinEvent(playerInfo);
           }
         }
        else
         {
          playerInfo = session.GetUserInfoBySFId(sfId);

          if (playerInfo != null)
           {
            if (status == MNConst.MN_USER_PLAYER)
             {
              players[sfId] = playerInfo;

              DispatchPlayerJoinEvent(playerInfo);
             }
            else
             {
              chaters[sfId] = playerInfo;
             }
           }
         }
       }
     }

    private void OnSessionPluginMessageReceived (string     pluginName,
                                                 string     message,
                                                 MNUserInfo sender)
     {
      if (pluginName == PROVIDER_NAME)
       {
        if      (message.StartsWith("i"))
         {
          ProcessInitMessage(message.Substring(1));
         }
        else if (message.StartsWith("m"))
         {
          ProcessModifyMessage(message.Substring(1));
         }
       }
     }

    private void OnSessionStatusChanged (int newStatus, int oldStatus)
     {
      if (newStatus == MNConst.MN_OFFLINE || newStatus == MNConst.MN_LOGGEDIN)
       {
        players.Clear();
        chaters.Clear();
       }
     }

    private MNSession                  session;
    private Dictionary<int,MNUserInfo> players;
    private Dictionary<int,MNUserInfo> chaters;

    private const string PROVIDER_NAME = "com.playphone.mn.psi";
   }
 }
