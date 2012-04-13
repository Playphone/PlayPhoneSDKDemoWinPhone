//
//  MNSmartFoxFacade.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

using SmartFoxClientAPI;
using SmartFoxClientAPI.Data;

namespace PlayPhone.MultiNet.Core
 {
  class MNSmartFoxFacade
   {
    public delegate void OnPreLoginSucceeded (long userId, string userName, string SId, int lobbyRoomId, string userAuthSign);
    public delegate void OnLoginSucceeded    ();
    public delegate void OnLoginFailed       (string error);
    public delegate void OnConnectionLost    ();

    public delegate void OnConfigLoadStarted ();
    public delegate void OnConfigLoaded      ();
    public delegate void OnConfigLoadFailed  (string error);

    public OnPreLoginSucceeded onPreLoginSucceeded;
    public OnLoginSucceeded    onLoginSucceeded;
    public OnLoginFailed       onLoginFailed;
    public OnConnectionLost    onConnectionLost;
    public OnConfigLoadStarted onConfigLoadStarted;
    public OnConfigLoaded      onConfigLoaded;
    public OnConfigLoadFailed  onConfigLoadFailed;

    public MNSmartFoxFacade(MNSession session, Uri configUri)
     {
      smartFox            = new SmartFoxClient(false);
      state               = State.Disconnected;
      this.session        = session;
      configData          = new MNConfigData(configUri);
      loginOnConfigLoaded = false;
      lobbyRoomId         = MNSession.MN_LOBBY_ROOM_ID_UNDEFINED;

//      MNDebug.todo("MNSmartFoxFacade: connection activity");

      SFSEvent.onConnection        += OnSmartFoxConnection;
      SFSEvent.onConnectionLost    += OnSmartFoxConnectionLost;
      SFSEvent.onLogin             += OnSmartFoxLogin;
      SFSEvent.onExtensionResponse += OnSmartFoxExtensionResponse;
      SFSEvent.onRoomListUpdate    += OnSmartFoxRoomListUpdate;
     }

    public void LoadConfig ()
     {
      if (state == State.Disconnected)
       {
        startConfigLoad();
       }
     }

    private void startConfigLoad ()
     {
      state = State.LoadingConfig;

      if (onConfigLoadStarted != null)
       {
        onConfigLoadStarted();
       }

      configData.Load(OnConfigDataLoaded,OnConfigDataLoadFailed);
     }

    private void loginWithStoredLoginAndConfigInfo ()
     {
      state = State.Connecting;

      smartFox.ipAddress        = configData.smartFoxAddr;
      smartFox.port             = configData.smartFoxPort;
      smartFox.blueBoxIpAddress = configData.blueBoxAddr;
      smartFox.blueBoxPort      = configData.blueBoxPort;
      smartFox.smartConnect     = configData.smartConnect;

      smartFox.Connect(smartFox.ipAddress,smartFox.port);
     }

    private void loginWithStoredLoginInfo ()
     {
      autoJoinInvoked = true; // to prevent autoJoin to to be invoked in onRoomListUpdate handler

      if (state != State.LoadingConfig)
       {
        state = State.Disconnected;
       }

      if (smartFox.IsConnected())
       {
        smartFox.Disconnect();
       }

      autoJoinInvoked = false;

      if (configData.IsLoaded())
       {
        loginWithStoredLoginAndConfigInfo();
       }
      else
       {
        loginOnConfigLoaded = true;

        if (state != State.LoadingConfig)
         {
          startConfigLoad();
         }
       }
     }

    public void Login (String zone, String userLogin, StructuredPassword userPassword)
     {
//      connectionActivity.cancel();

//      MNDebug.todo("MNSmartFoxFacade: connection activity");

      autoJoinInvoked = true; /* to prevent autoJoin to to be invoked in onRoomListUpdate handler */

      if (smartFox.IsConnected())
       {
        smartFox.Disconnect();
       }

      this.zone         = zone;
      this.userLogin    = userLogin;
      this.userPassword = userPassword;

      loginWithStoredLoginInfo();
     }

    public void Logout ()
     {
//      MNDebug.todo("MNSmartFoxFacade: connection activity");

      // connectionActivity.cancel();

      smartFox.Disconnect();

      configData.Clear();
     }

    public void ReLogin ()
     {
      MNDebug.todo("MNSmartFoxFacade: ReLogin is not implemented");

     //      connectionActivity.start();
     }

    public bool HaveLoginInfo ()
     {
      return userLogin != null;
     }

    public bool IsLoggedIn ()
     {
      return state == State.LoggedIn;
     }

    public string GetLoginInfoLogin ()
     {
      return userLogin;
     }

    public void UpdateLoginInfo (string userLogin, StructuredPassword userPassword)
     {
      this.userLogin    = userLogin;
      this.userPassword = userPassword;
     }

    public string GetUserNameBySFId (int sfId)
     {
      if (!smartFox.IsConnected())
       {
        return null;
       }

      Room currentRoom = smartFox.GetRoom(smartFox.activeRoomId);

      if (currentRoom == null)
       {
        return null;
       }

      User userInfo = currentRoom.GetUser(sfId);

      if (userInfo == null)
       {
        return null;
       }

      return userInfo.GetName();
     }

    private void OnConfigDataLoaded (MNConfigData configData)
     {
      state = State.Disconnected;

      if (loginOnConfigLoaded)
       {
        loginOnConfigLoaded = false;

        loginWithStoredLoginAndConfigInfo();
       }

      if (onConfigLoaded != null)
       {
        onConfigLoaded();
       }
     }

    private void OnConfigDataLoadFailed (string errorMessage)
     {
      state = State.Disconnected;

      if (loginOnConfigLoaded)
       {
        loginOnConfigLoaded = false;

        if (onLoginFailed != null)
         {
          onLoginFailed(errorMessage);
         }
       }

      if (onConfigLoadFailed != null)
       {
        onConfigLoadFailed(errorMessage);
       }
     }

    private void OnSmartFoxConnection (bool success, string error)
     {
      if (state == State.Connecting)
       {
        if (success)
         {
//          MNDebug.todo("MNSmartFoxFacade: connection activity");

          // connectionActivity.connectionEstablished();

          state = State.Connected;

          smartFox.Login(zone,userLogin,userPassword.BuildPassword(MNUtils.StringGetMD5String(session.GetUniqueAppId())));
         }
        else
         {
          sessionInfo = null;

          state = State.Disconnected;

          if (onLoginFailed != null)
           {
            onLoginFailed(error);
           }

//          MNDebug.todo("MNSmartFoxFacade: connection activity");

          // connectionActivity.connectionFailed();
         }
       }
     }

    private void OnSmartFoxConnectionLost ()
     {
      sessionInfo = null;

      if (state != State.Disconnected)
       {
        state = State.Disconnected;

        if (onConnectionLost != null)
         {
          onConnectionLost();
         }
       }
     }

    private void OnSmartFoxLogin (bool success, string name, string error)
     {
      if (success)
       {
        sessionInfo = new MNSmartFoxFacadeSessionInfo
                           (MNConst.MN_USER_ID_UNDEFINED,
                            name,
                            null,
                            MNSession.MN_LOBBY_ROOM_ID_UNDEFINED,
                            null);
       }
      else
       {
        sessionInfo = null;

        state = State.Disconnected;

        smartFox.Disconnect();

        if (onLoginFailed != null)
         {
          onLoginFailed(error);
         }
       }
     }

    private static string SFSObjectGetStringSafe (SFSObject data, string key)
     {
      try
       {
        return data.GetString(key);
       }
      catch (KeyNotFoundException)
       {
        return null;
       }
      catch (InvalidCastException)
       {
        return null;
       }
     }

    private void OnSmartFoxExtensionResponse (object data, string type)
     {
      if (state != State.Connected || type != SmartFoxClient.XTMSG_TYPE_XML)
       {
        return;
       }

		    SFSObject responseData = (SFSObject)data;

      string cmd = SFSObjectGetStringSafe(responseData,"_cmd");
      
      if (cmd == null)
       {
        return;
       }

      if (cmd == "MN_logOK")
       {
        bool    ok           = true;
        string  userName     = SFSObjectGetStringSafe(responseData,"MN_user_name");
        string  userSId      = SFSObjectGetStringSafe(responseData,"MN_user_sid");
        string  userAuthSign = SFSObjectGetStringSafe(responseData,"MN_user_auth_sign");
        long    userId       = MNConst.MN_USER_ID_UNDEFINED;
        int     userSFId     = MNConst.MN_USER_SFID_UNDEFINED;

        lobbyRoomId  = MNSession.MN_LOBBY_ROOM_ID_UNDEFINED;

        try
         {
          userId      = long.Parse(SFSObjectGetStringSafe(responseData,"MN_user_id"));
          userSFId    = int.Parse(SFSObjectGetStringSafe(responseData,"MN_user_sfid"));
          lobbyRoomId = int.Parse(SFSObjectGetStringSafe(responseData,"MN_lobby_room_sfid"));
         }
        catch (FormatException)
         {
          ok = false;
         }
        catch (OverflowException)
         {
          ok = false;
         }
        catch (ArgumentNullException)
         {
          ok = false;
         }

        if (ok)
         {
          if (userName.Length == 0 || userSId.Length == 0)
           {
            ok = false;
           }
         }

        if (ok)
         {
          smartFox.amIModerator = false;
          smartFox.myUserId     = userSFId;
          smartFox.myUserName   = userName;
          smartFox.playerId     = -1;

          sessionInfo = new MNSmartFoxFacadeSessionInfo(userId,userName,userSId,lobbyRoomId,userAuthSign);

          if (onPreLoginSucceeded != null)
           {
            onPreLoginSucceeded
             (sessionInfo.userId,
              sessionInfo.userName,
              sessionInfo.sId,
              sessionInfo.lobbyRoomId,
              sessionInfo.userAuthSign);
           }
         }
        else
         {
          state = State.Disconnected;

          smartFox.Disconnect();

          if (onLoginFailed != null)
           {
            MNDebug.todo("MNSmartFoxFacade: localization required");

            onLoginFailed("login extension error - invalid user_id or lobby_room_sfid received");
           }
         }
       }
      else if (cmd == "MN_logKO")
       {
        state = State.Disconnected;

        if (onLoginFailed != null)
         {
          onLoginFailed(SFSObjectGetStringSafe(responseData,"MN_err_msg"));
         }

        smartFox.Disconnect();
       }
      else if (cmd == "initUserInfo")
       {
        if (sessionInfo != null)
         {
          state = State.LoggedIn;

          if (onLoginSucceeded != null)
           {
            onLoginSucceeded();
           }

          sessionInfo = null;
         }
       }
     }

    private void OnSmartFoxRoomListUpdate(Dictionary<int,Room> roomList)
     {
      if (!autoJoinInvoked)
       {
        autoJoinInvoked = true;

        if (lobbyRoomId != MNSession.MN_LOBBY_ROOM_ID_UNDEFINED)
         {
          smartFox.JoinRoom(lobbyRoomId);

          lobbyRoomId = MNSession.MN_LOBBY_ROOM_ID_UNDEFINED;
         }
       }
     }

    private enum State
     {
      Disconnected, LoadingConfig, Connecting, Connected, LoggedIn
     }

    public class StructuredPassword
     {
      public  StructuredPassword (string passwordPrefix, string passwordSuffix)
       {
        prefix = passwordPrefix;
        suffix = passwordSuffix;
       }

      public string BuildPassword (string uniqueId)
       {
        return prefix + uniqueId + suffix;
       }

      private string prefix;
      private string suffix;
     }

    public MNConfigData    configData;

    internal SmartFoxClient    smartFox;
    private MNSession          session;
    private string             zone;
    private string             userLogin;
    private StructuredPassword userPassword;
    private State              state;
    private bool               autoJoinInvoked;
    private bool               loginOnConfigLoaded;
    private int                lobbyRoomId;

    private MNSmartFoxFacadeSessionInfo sessionInfo;
   }
 
  internal class MNSmartFoxFacadeSessionInfo
   {
    public MNSmartFoxFacadeSessionInfo (long   userId,
                                        string userName,
                                        string sId,
                                        int    lobbyRoomId,
                                        string userAuthSign)
     {
      this.userId       = userId;
      this.userName     = userName;
      this.sId          = sId;
      this.lobbyRoomId  = lobbyRoomId;
      this.userAuthSign = userAuthSign;
     }

    public long   userId { get; set; }
    public string userName { get; set; }
    public string sId { get; set; }
    public int    lobbyRoomId { get; set; }
    public string userAuthSign { get; set; }
   }
 }
