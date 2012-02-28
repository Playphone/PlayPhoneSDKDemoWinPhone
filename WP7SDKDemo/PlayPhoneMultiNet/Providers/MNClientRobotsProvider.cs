//
//  MNClientRobotsProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

using SmartFoxClientAPI;
using SmartFoxClientAPI.Data;

using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNClientRobotsProvider
   {
    public MNClientRobotsProvider (MNSession session)
     {
      this.session = session;

      robots = new Dictionary<int,bool>();

      session.PluginMessageReceived += OnSessionPluginMessageReceived;
      session.SessionStatusChanged  += OnSessionStatusChanged;
      session.RoomUserLeave         += OnSessionRoomUserLeave;
     }

    public void Shutdown ()
     {
      session.PluginMessageReceived -= OnSessionPluginMessageReceived;
      session.SessionStatusChanged  -= OnSessionStatusChanged;
      session.RoomUserLeave         -= OnSessionRoomUserLeave;

      robots.Clear();
     }

    public bool IsRobot (MNUserInfo userInfo)
     {
      return robots.ContainsKey(userInfo.UserSFId);
     }

    public void PostRobotScore (MNUserInfo userInfo, long score)
     {
      session.SendPluginMessage
       (PROVIDER_NAME,
        "robotScore:" + userInfo.UserSFId.ToString() + ":" + score.ToString());
     }

    public void SetRoomRobotLimit (int robotCount)
     {
      SmartFoxClient     smartFox = session.GetSmartFox();
      List<RoomVariable> vars     = new List<RoomVariable>();

      vars.Add(new RoomVariable(ROBOT_ROOM_LIMIT_VARNAME,robotCount.ToString(),false,true));

      smartFox.SetRoomVariables(vars);
     }

    public int GetRoomRobotLimit ()
     {
      Room currentRoom = session.GetSmartFox().GetActiveRoom();

      if (currentRoom != null)
       {
        return session.GetRoomVariableAsInt
                (currentRoom,ROBOT_ROOM_LIMIT_VARNAME) ?? 0;
       }
      else
       {
        return 0;
       }
     }

    private void OnSessionRoomUserLeave (MNUserInfo userInfo)
     {
      robots.Remove(userInfo.UserSFId);
     }

    private void OnSessionPluginMessageReceived (string     pluginName,
                                                 string     message,
                                                 MNUserInfo sender)
     {
      if (pluginName != PROVIDER_NAME)
       {
        return;
       }

      if (message.StartsWith(IROBOT_MESSAGE_PREFIX))
       {
        string data     = message.Substring(IROBOT_MESSAGE_PREFIX_LEN);
        int    colonPos = data.IndexOf(':');

        if (colonPos >= 0)
         {
          data = data.Substring(0,colonPos);
         }

        int? sfId = MNUtils.ParseInt(data);

        if (sfId != null)
         {
          robots[sfId.Value] = true;
         }
       }
     }

    private void OnSessionStatusChanged (int newStatus, int oldStatus)
     {
      if (!session.IsInGameRoom())
       {
        robots.Clear();
       }
     }

    private MNSession            session;
    private Dictionary<int,bool> robots;

    private const string IROBOT_MESSAGE_PREFIX = "irobot:";
    private readonly int IROBOT_MESSAGE_PREFIX_LEN = IROBOT_MESSAGE_PREFIX.Length;
    private const string ROBOT_ROOM_LIMIT_VARNAME = "MN_robot_limit";
    private const string PROVIDER_NAME = "com.playphone.mn.robotscore";
   }
 }
