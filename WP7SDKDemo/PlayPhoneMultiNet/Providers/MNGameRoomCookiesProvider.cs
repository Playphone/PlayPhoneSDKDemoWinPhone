//
//  MNGameRoomCookiesProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System.Collections.Generic;

using SmartFoxClientAPI;
using SmartFoxClientAPI.Data;

using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNGameRoomCookiesProvider
   {
    public delegate void GameRoomCookieDownloadSucceededEventHandler (int roomSFId, int key, string cookie);
    public delegate void GameRoomCookieDownloadFailedEventHandler    (int roomSFId, int key, string error);

    public event GameRoomCookieDownloadSucceededEventHandler GameRoomCookieDownloadSucceeded;
    public event GameRoomCookieDownloadFailedEventHandler    GameRoomCookieDownloadFailed;

    public MNGameRoomCookiesProvider (MNSession session)
     {
      this.session = session;

      session.PluginMessageReceived += OnSessionPluginMessageReceived;
     }

    public void Shutdown ()
     {
      session.PluginMessageReceived -= OnSessionPluginMessageReceived;
     }

    public void DownloadGameRoomCookie (int roomSFId, int key)
     {
      session.SendPluginMessage(PROVIDER_NAME,"g" + roomSFId.ToString() +
                                              ':' + key.ToString() + ':' +
                                              REQUEST_NUMBER_API.ToString());
     }

    public void SetCurrentGameRoomCookie (int key, string cookie)
     {
      if (key < COOKIE_MIN_KEY || key > COOKIE_MAX_KEY ||
          (cookie != null && cookie.Length > COOKIE_DATA_MAX_LENGTH))
       {
        MNDebug.error("unable to set room cookie - invalid cookie");

        return;
       }

      SmartFoxClient     smartFox = session.GetSmartFox();
      List<RoomVariable> vars     = new List<RoomVariable>();

      vars.Add(new RoomVariable(GetVarNameByCookieKey(key),cookie,false,true));

      smartFox.SetRoomVariables(vars);
     }

    public string GetCurrentGameRoomCookie (int key)
     {
      string value       = null;
      Room   currentRoom = session.GetSmartFox().GetActiveRoom();

      if (currentRoom != null)
       {
        object tempValue = null;
       
        try
         {
          tempValue = currentRoom.GetVariable(GetVarNameByCookieKey(key));
         }
        catch (KeyNotFoundException)
         {
         }

        if (tempValue != null)
         {
          value = tempValue as string;
         }
       }
      else
       {
        MNDebug.error("unable to get room cookie - no active room");
       }

      return value;
     }

    private void HandleGetResponse (string[] components)
     {
      if (components.Length < 4)
       {
        return;
       }

      int key;
      int requestNumber;
      int roomSFId;

      if (!int.TryParse(components[0],out roomSFId) ||
          !int.TryParse(components[1],out key) ||
          !int.TryParse(components[2],out requestNumber))
       {
        return;
       }

      if (requestNumber != REQUEST_NUMBER_API)
       {
        return;
       }

      if (components[3] == RESPONSE_STATUS_OK)
       {
        string data = components.Length < 5 ? null : components[4];

        DispatchRoomCookieDownloadSucceededEvent(roomSFId,key,data);
       }
      else if (components[3] == RESPONSE_STATUS_ERROR)
       {
        string message = components.Length < 5 ? "game room cookie retrieval failed" : components[4];

        DispatchRoomCookieDownloadFailedEvent(roomSFId,key,message);
       }
     }

    private void DispatchRoomCookieDownloadSucceededEvent (int roomSFId, int key, string data)
     {
      GameRoomCookieDownloadSucceededEventHandler handler = GameRoomCookieDownloadSucceeded;

      if (handler != null)
       {
        handler(roomSFId,key,data);
       }
     }

    private void DispatchRoomCookieDownloadFailedEvent (int roomSFId, int key, string message)
     {
      GameRoomCookieDownloadFailedEventHandler handler = GameRoomCookieDownloadFailed;

      if (handler != null)
       {
        handler(roomSFId,key,message);
       }
     }

    private void OnSessionPluginMessageReceived (string     pluginName,
                                                 string     message,
                                                 MNUserInfo sender)
     {
      if (sender != null || pluginName != PROVIDER_NAME)
       {
        return;
       }

      if (message.Length == 0)
       {
        return;
       }

      char cmd = message[0];

      if (cmd == REQUEST_CMD_GET)
       {
        HandleGetResponse(MNUtils.StringSplitWithLimit(message.Substring(1),':',5));
       }
     }

    private static string GetVarNameByCookieKey (int key)
     {
      return "MN_RV_" + key.ToString();
     }

    private MNSession session;

    private const string PROVIDER_NAME = "com.playphone.mn.grc";

    private const int    REQUEST_NUMBER_API = 0;

    private const int    COOKIE_MIN_KEY = 0;
    private const int    COOKIE_MAX_KEY = 99;
    private const int    COOKIE_DATA_MAX_LENGTH = 1024;

    private const char   REQUEST_CMD_GET       = 'g';
    private const string RESPONSE_STATUS_OK    = "s";
    private const string RESPONSE_STATUS_ERROR = "e";
   }
 }
