//
//  MNSession.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Text;
using System.Collections.Generic;
using SmartFoxClientAPI;
using SmartFoxClientAPI.Data;

namespace PlayPhone.MultiNet.Core
 {
  public class MNSession
   {
    public delegate void SessionStatusChangedEventHandler   (int newStatus, int oldStatus);
    public delegate void LoginInitiatedEventHandler         ();
    public delegate void WebFrontURLReadyEventHandler       (string webServerUrl);
    public delegate void ConfigLoadedEventHandler           ();
    public delegate void UserChangedEventHandler            (long userId);
    public delegate void ErrorOccurredEventHandler          (MNErrorInfo errorInfo);
    public delegate void DevUsersInfoChangedEventHandler    ();
    public delegate void ExecAppCommandReceivedEventHandler (string cmdName, string cmdParam);
    public delegate void ExecUICommandReceivedEventHandler  (string cmdName, string cmdParam);
    public delegate void WebEventReceivedEventHandler       (string eventName, string eventParam, string callbackId);
    public delegate void SysEventReceivedEventHandler       (string eventName, string eventParam, string callbackId);
    public delegate void AppHostCallReceivedEventHandler    (MNAppHostCallEventArgs arg);
    public delegate void DoStartGameWithParamsEventHandler  (MNGameParams gameParams);
    public delegate void GameFinishedWithResultEventHandler (MNGameResult gameResult);
    public delegate void DefaultGameSetIdChangedToEventHandler (int gameSetId);
    public delegate void ConfigLoadStartedEventHandler      ();
    public delegate void ChatPublicMessageReceivedEventHandler  (MNChatMessage chatMessage);
    public delegate void ChatPrivateMessageReceivedEventHandler (MNChatMessage chatMessage);
    public delegate void RoomUserStatusChangedEventHandler      (int userStatus);
    public delegate void GameStartCountdownTickEventHandler (int secondsLeft);
    public delegate void DoCancelGameEventHandler ();
    public delegate void DoFinishGameEventHandler ();
    public delegate void RoomUserJoinEventHandler (MNUserInfo userInfo);
    public delegate void RoomUserLeaveEventHandler (MNUserInfo userInfo);
    public delegate void CurrGameResultsReceivedEventHandler (MNCurrGameResults gameResults);
    public delegate void JoinRoomInvitationReceivedEventHandler (MNJoinRoomInvitationParams invParams);
    public delegate void GameMessageReceivedEventHandler (String message, MNUserInfo sender);
    public delegate void PluginMessageReceivedEventHandler (String pluginName, String message, MNUserInfo sender);
    public delegate void AppBeaconResponseReceivedEventHandler (MNAppBeaconResponse response);
    public delegate void VShopReadyStatusChangedEventHandler (bool isVShopReady);

    public event SessionStatusChangedEventHandler       SessionStatusChanged;
    public event LoginInitiatedEventHandler             LoginInitiated;
    public event WebFrontURLReadyEventHandler           WebFrontURLReady;
    public event ConfigLoadedEventHandler               ConfigLoaded;
    public event UserChangedEventHandler                UserChanged;
    public event ErrorOccurredEventHandler              ErrorOccurred;
    public event DevUsersInfoChangedEventHandler        DevUsersInfoChanged;
    public event ExecAppCommandReceivedEventHandler     ExecAppCommandReceived;
    public event ExecUICommandReceivedEventHandler      ExecUICommandReceived;
    public event WebEventReceivedEventHandler           WebEventReceived;
    public event SysEventReceivedEventHandler           SysEventReceived;
    public event AppHostCallReceivedEventHandler        AppHostCallReceived;
    public event DoStartGameWithParamsEventHandler      DoStartGameWithParams;
    public event GameFinishedWithResultEventHandler     GameFinishedWithResult;
    public event DefaultGameSetIdChangedToEventHandler  DefaultGameSetIdChangedTo;
    public event ConfigLoadStartedEventHandler          ConfigLoadStarted;
    public event ChatPublicMessageReceivedEventHandler  ChatPublicMessageReceived;
    public event ChatPrivateMessageReceivedEventHandler ChatPrivateMessageReceived;
    public event RoomUserStatusChangedEventHandler      RoomUserStatusChanged;
    public event GameStartCountdownTickEventHandler     GameStartCountdownTick;
    public event DoCancelGameEventHandler               DoCancelGame;
    public event DoFinishGameEventHandler               DoFinishGame;
    public event RoomUserJoinEventHandler               RoomUserJoin;
    public event RoomUserLeaveEventHandler              RoomUserLeave;
    public event CurrGameResultsReceivedEventHandler    CurrGameResultsReceived;
    public event JoinRoomInvitationReceivedEventHandler JoinRoomInvitationReceived;
    public event GameMessageReceivedEventHandler        GameMessageReceived;
    public event PluginMessageReceivedEventHandler      PluginMessageReceived;
    public event AppBeaconResponseReceivedEventHandler  AppBeaconResponseReceived;
    public event VShopReadyStatusChangedEventHandler    VShopReadyStatusChanged;

    public MNSession (int gameId, string gameSecret)
     {
      this.gameId      = gameId;
      this.gameSecret  = gameSecret;

      status      = MNConst.MN_OFFLINE;
      userId      = MNConst.MN_USER_ID_UNDEFINED;
      lobbyRoomId = MNSession.MN_LOBBY_ROOM_ID_UNDEFINED;
      userStatus  = MNConst.MN_USER_STATUS_UNDEFINED;
      userName    = null;
      userSId     = null;

      roomExtraInfoReceived = false;
      pendingGameResult     = null;
      defaultGameSetId      = MNGameParams.MN_GAMESET_ID_DEFAULT;
      fastResumeEnabled     = true;
      webShopIsReady        = false;

      useInstallIdInsteadOfUDID = false;

      appExtParams = MNPlatformWinPhone.ReadAppExtParams();

      if (!MNPlatformWinPhone.CreateDataDirectory())
       {
        MNDebug.debug("warning: unable to create data dir");
       }

      smartFoxFacade = new MNSmartFoxFacade(this,BuildConfigRequestUri());

      smartFoxFacade.onPreLoginSucceeded = OnSmartFoxFacadePreLoginSucceeded;
      smartFoxFacade.onLoginSucceeded    = OnSmartFoxFacadeLoginSucceeded;
      smartFoxFacade.onLoginFailed       = OnSmartFoxFacadeLoginFailed;
      smartFoxFacade.onConnectionLost    = OnSmartFoxFacadeConnectionLost;
      smartFoxFacade.onConfigLoadStarted = OnSmartFoxFacadeConfigLoadStarted;
      smartFoxFacade.onConfigLoaded      = OnSmartFoxFacadeConfigLoaded;
      smartFoxFacade.onConfigLoadFailed  = OnSmartFoxFacadeConfigLoadFailed;

      SFSEvent.onPublicMessage       += OnSmartFoxPublicMessage;
      SFSEvent.onPrivateMessage      += OnSmartFoxPrivateMessage;
      SFSEvent.onRoomVariablesUpdate += OnSmartFoxRoomVariablesUpdate;
      SFSEvent.onUserVariablesUpdate += OnSmartFoxUserVariablesUpdate;
      SFSEvent.onJoinRoom            += OnSmartFoxJoinRoom;
      SFSEvent.onUserEnterRoom       += OnSmartFoxUserEnterRoom;
      SFSEvent.onUserLeaveRoom       += OnSmartFoxUserLeaveRoom;
      SFSEvent.onExtensionResponse   += OnSmartFoxExtensionResponse;

      socNetSessionFB = new MNSocNetSessionFB();

      varStorage = new MNVarStorage();

      webServerUrl = null;
      launchParam  = null;

      launchTime = MNUtils.GetUnixTime();
      launchId   = MNUtils.GenerateUniqueId();
      installId  = LoadInstallId();

      trackingSystem = null;
      appConfigVars = new Dictionary<string,string>();

      gameVocabulary = new MNGameVocabulary(this);
     }

    private Uri BuildConfigRequestUri ()
     {
      string multiNetConfigUrl = MNPlatformWinPhone.GetMultiNetConfigURL();

      return new Uri(multiNetConfigUrl + "?" +
                     MNUtils.HttpGetRequestBuildParamsString(BuildDefaultQueryParamsDict(false)));
     }

    internal Dictionary<string,string> BuildDefaultQueryParamsDict (bool addDevId)
     {
      Dictionary<string,string> queryParams = new Dictionary<string,string>();
      string appVerExternal = MNPlatformWinPhone.GetAppVerExternal();
      string appVerInternal = MNPlatformWinPhone.GetAppVerInternal();

      queryParams["game_id"]       = gameId.ToString();
      queryParams["dev_type"]      = MNPlatformWinPhone.GetDeviceType().ToString();
      queryParams["client_ver"]    = CLIENT_API_VERSION;
      queryParams["client_locale"] = System.Globalization.CultureInfo.CurrentCulture.Name;
      queryParams["app_ver_ext"]   = appVerExternal != null ? appVerExternal : "";
      queryParams["app_ver_int"]   = appVerInternal != null ? appVerInternal : "";

      if (addDevId)
       {
        queryParams["dev_id"] = MNUtils.StringGetMD5String(GetUniqueAppId());
       }

      foreach (var appExtParam in appExtParams)
       {
        queryParams[appExtParam.Key] = appExtParam.Value;
       }

      return queryParams;
     }

    public static string MakeGameSecretByComponents (uint secret1, uint secret2, uint secret3, uint secret4)
     {
      return MNUtils.MakeGameSecretByComponents(secret1,secret2,secret3,secret4);
     }

     private MNSmartFoxFacade.StructuredPassword MakeStructuredPasswordFromParams (string loginModel, string gameSecret, string passwordHash, bool userDevSetHome)
     {
      string appVerInternal = MNPlatformWinPhone.GetAppVerInternal();
      string appVerExternal = MNPlatformWinPhone.GetAppVerExternal();

      if (appVerInternal == null)
       {
        appVerInternal = "";
       }

      if (appVerExternal == null)
       {
        appVerExternal = "";
       }

      return new MNSmartFoxFacade.StructuredPassword
                  (CLIENT_API_VERSION + "," +
                   loginModel + "," +
                   passwordHash + "," +
                   gameSecret + "," +
                   MNPlatformWinPhone.GetDeviceType().ToString() + ",",
                   "," +
                   (userDevSetHome ? "1" : "0") + "," +
                   MNPlatformWinPhone.GetDeviceInfoString().Replace(',','-') + "," +
                   (launchId + "|" +
                    appVerInternal.Replace('|',' ') + "|" +
                    appVerExternal.Replace('|',' ')).Replace(',','-'));
     }

    private string MakeNewGuestPassword ()
     {
      Random   rng = new Random();
      DateTime now = DateTime.Now;

      return MNUtils.StringGetMD5String
              (Guid.NewGuid().ToString() +
               MNUtils.GetUnixTime().ToString() +
               now.Ticks.ToString() +
               rng.Next().ToString() +
               rng.Next().ToString());
     }

    private string LoadInstallId ()
     {
      string installId = varStorage.GetValue(INSTALL_ID_VAR_NAME);

      if (installId == null)
       {
        installId = MNUtils.GenerateUniqueId();

        varStorage.SetValue(INSTALL_ID_VAR_NAME,installId);
        varStorage.WriteToFile();
       }

      return installId;
     }

    public bool LoginWithUserLoginAndPassword (string login, string password, bool saveCredentials)
     {
      return LoginWithUserLoginAndStructuredPassword
              (login,MakeStructuredPasswordFromParams
                      (LOGIN_MODEL_LOGIN_PLUS_PASSWORD,
                       gameSecret,
                       MNUtils.StringGetMD5String(password),
                       saveCredentials));
     }

    public bool LoginWithUserIdAndPasswordHash (long id, string passwordHash, bool saveCredentials)
     {
      return LoginWithUserLoginAndStructuredPassword
              (id.ToString(),
               MakeStructuredPasswordFromParams
                (LOGIN_MODEL_ID_PLUS_PASSWORD_HASH,gameSecret,passwordHash,saveCredentials));
     }

    public bool LoginWithDeviceCredentials ()
     {
      return LoginWithUserLoginAndStructuredPassword
              (LOGIN_MODEL_GUEST_USER_LOGIN,
               MakeStructuredPasswordFromParams
                (LOGIN_MODEL_GUEST,gameSecret,MakeNewGuestPassword(),true));
     }

    public bool LoginWithUserIdAndAuthSign (long id, string authSign)
     {
      return LoginWithUserLoginAndStructuredPassword
              (id.ToString(),
               MakeStructuredPasswordFromParams
                (LOGIN_MODEL_AUTH_SIGN,gameSecret,authSign,true));
     }

    private bool RegisterLoginOffline (long id, string name, string authSign)
     {
      userId      = id;
      userName    = name;
      userSId     = null;
      lobbyRoomId = MNSession.MN_LOBBY_ROOM_ID_UNDEFINED;

      MNUserCredentials.UpdateCredentials(varStorage,new MNUserCredentials(id,name,authSign,DateTime.Now,null));

      DispatchDevUsersInfoChanged();
      DispatchUserChangedEvent(id);

      return true;
     }

    public bool LoginOfflineWithUserIdAndAuthSign (long id, string authSign)
     {
      if (status != MNConst.MN_OFFLINE)
       {
        DispatchLoginFailed(MNI18n.GetLocalizedString("Cannot login offline while connected to server",MNI18n.MESSAGE_CODE_OFFLINE_CANNOT_LOGIN_OFFLINE_WHILE_CONNECTED_TO_SERVER_ERROR));

        return false;
       }

      MNUserCredentials credentials = MNUserCredentials.GetCredentialsByUserId(varStorage,id);

      if (credentials == null || authSign != credentials.userAuthSign)
       {
        DispatchLoginFailed(MNI18n.GetLocalizedString("Invalid login or password",MNI18n.MESSAGE_CODE_OFFLINE_INVALID_AUTH_SIGN_ERROR));

        return false;
       }

      return RegisterLoginOffline(userId,credentials.userName,authSign);
     }

    public bool SignupOffline ()
     {
      if (status != MNConst.MN_OFFLINE)
       {
        DispatchLoginFailed(MNI18n.GetLocalizedString("Cannot login offline while connected to server",MNI18n.MESSAGE_CODE_OFFLINE_CANNOT_LOGIN_OFFLINE_WHILE_CONNECTED_TO_SERVER_ERROR));

        return false;
       }

      long currentTime = MNUtils.GetUnixTime();
      long id          = -currentTime;

      while (MNUserCredentials.GetCredentialsByUserId(varStorage,id) != null)
       {
        id++;
       }

      return RegisterLoginOffline
              (id,"Guest_" + currentTime.ToString(),
               "TMP_" + currentTime.ToString() + id.ToString());
     }

    public bool LoginAuto ()
     {
      MNUserCredentials lastUserCredentials = MNUserCredentials.GetMostRecentlyLoggedUserCredentials(varStorage);

      if (lastUserCredentials != null)
       {
        return LoginWithUserIdAndAuthSign
                (lastUserCredentials.userId,lastUserCredentials.userAuthSign);
       }
      else
       {
        return LoginWithDeviceCredentials();
       }
     }

    private bool LoginWithStoredCredentials ()
     {
      MNUserCredentials lastUserCredentials = MNUserCredentials.GetMostRecentlyLoggedUserCredentials(varStorage);

      if (lastUserCredentials != null)
       {
        return LoginWithUserIdAndAuthSign
                (lastUserCredentials.userId,lastUserCredentials.userAuthSign);
       }
      else
       {
        return false;
       }
     }

    private bool LoginWithUserLoginAndStructuredPassword (string login, MNSmartFoxFacade.StructuredPassword structuredPassword)
     {
      if (status != MNConst.MN_OFFLINE)
       {
        if (OFFLINE_MODE_DISABLED)
         {
          Logout();
         }
        else
         {
          smartFoxFacade.Logout();
         }
       }

      LoginInitiatedEventHandler handler = LoginInitiated;

      if (handler != null)
       {
        handler();
       }

      string zone = GAME_ZONE_NAME_PREFIX + gameId.ToString();

      SetNewStatus(MNConst.MN_CONNECTING);

      smartFoxFacade.Login(zone,login,structuredPassword);

      return true;
     }

    public void Logout ()
     {
      LogoutAndWipeUserCredentialsByMode(MN_CREDENTIALS_WIPE_NONE);
     }

    public void LogoutAndWipeUserCredentialsByMode (int wipeMode)
     {
      if      (wipeMode == MN_CREDENTIALS_WIPE_ALL)
       {
        MNUserCredentials.WipeAllCredentials(varStorage);

        varStorage.RemoveVariablesByMask("user.*");
       }
      else if (wipeMode == MN_CREDENTIALS_WIPE_USER)
       {
        if (IsUserLoggedIn())
         {
          MNUserCredentials.WipeCredentialsByUserId(varStorage,userId);

          varStorage.RemoveVariablesByMask("user." + userId.ToString() + ".*");
         }
       }

      DispatchDevUsersInfoChanged();

      if (wipeMode != 0)
       {
        if (socNetSessionFB.IsAuthorized())
         {
          socNetSessionFB.Logout();
         }
       }

      userSId     = null;
      lobbyRoomId = MNSession.MN_LOBBY_ROOM_ID_UNDEFINED;

      if (status != MNConst.MN_OFFLINE)
       {
        SetNewStatus(MNConst.MN_OFFLINE);

        smartFoxFacade.Logout();
       }

      if (userId != MNConst.MN_USER_ID_UNDEFINED)
       {
        userId   = MNConst.MN_USER_ID_UNDEFINED;
        userName = null;

        DispatchUserChangedEvent(userId);
       }

      varStorage.WriteToFile();
     }

    public void Shutdown ()
     {
      GetTrackingSystem().TrackShutdown(this);

      Logout();

      varStorage.WriteToFile();
     }

    public bool IsReLoginPossible ()
     {
      return smartFoxFacade.HaveLoginInfo();
     }

    public void ReLogin ()
     {
      if (IsReLoginPossible())
       {
        smartFoxFacade.ReLogin();
       }
     }

    public bool IsOnline ()
     {
      return status != MNConst.MN_OFFLINE && status != MNConst.MN_CONNECTING;
     }

    public bool IsUserLoggedIn ()
     {
      return userId != MNConst.MN_USER_ID_UNDEFINED;
     }

    public int GetGameId ()
     {
      return gameId;
     }

    public int GetStatus ()
     {
      return status;
     }

    public bool IsInGameRoom ()
     {
      return status == MNConst.MN_IN_GAME_WAIT || status == MNConst.MN_IN_GAME_START ||
             status == MNConst.MN_IN_GAME_PLAY || status == MNConst.MN_IN_GAME_END;
     }

    public string GetMyUserName ()
     {
      if (IsUserLoggedIn())
       {
        return userName;
       }
      else
       {
        return null;
       }
     }

    public long GetMyUserId()
     {
      return userId;
     }

    public int GetMyUserSFId ()
     {
      if (smartFoxFacade.IsLoggedIn())
       {
        return smartFoxFacade.smartFox.myUserId;
       }
      else
       {
        return MNConst.MN_USER_SFID_UNDEFINED;
       }
     }

    public MNUserInfo GetMyUserInfo ()
     {
      if (IsUserLoggedIn())
       {
        return new MNUserInfo(GetMyUserId(),GetMyUserSFId(),GetMyUserName(),webServerUrl);
       }
      else
       {
        return null;
       }
     }

    public int GetCurrentRoomId ()
     {
      if (smartFoxFacade.IsLoggedIn())
       {
        return smartFoxFacade.smartFox.activeRoomId;
       }
      else
       {
        return MNSession.MN_ROOM_ID_UNDEFINED;
       }
     }

    public String GetWebServerURL ()
     {
      if (smartFoxFacade.configData.IsLoaded())
       {
        return webServerUrl;
       }
      else
       {
        smartFoxFacade.LoadConfig();

        return null;
       }
     }

    public String GetWebFrontURL ()
     {
      //FIXME: offline mode is not supported
      return GetWebServerURL();
     }

    private MNUserInfo CreateUserInfoBySFIdAndName (int userSFId, String structuredName)
     {
      string shortUserName;
      long   userId;

      if (MNUtils.ParseMNUserName (structuredName,out shortUserName,out userId))
       {
        return new MNUserInfo(userId,userSFId,shortUserName,webServerUrl);
       }
      else
       {
        return new MNUserInfo(MNConst.MN_USER_ID_UNDEFINED,userSFId,structuredName,webServerUrl);
       }
     }

    public MNUserInfo[] GetRoomUserList ()
     {
      if (!IsOnline())
       {
        return new MNUserInfo[0];
       }

      Room currentRoom = smartFoxFacade.smartFox.GetRoom
                          (smartFoxFacade.smartFox.activeRoomId);

      if (currentRoom == null)
       {
        return new MNUserInfo[0];
       }

      Dictionary<int,User> sfUserList = currentRoom.GetUserList();

      MNUserInfo[] userList = new MNUserInfo[sfUserList.Count];

      uint index = 0;

      foreach (var sfUser in sfUserList)
       {
        userList[index++] = CreateUserInfoBySFIdAndName(sfUser.Value.GetId(),sfUser.Value.GetName());
       }

      return userList;
     }

    public MNUserInfo GetUserInfoBySFId (int sfId)
     {
      if (!IsOnline())
       {
        return null;
       }

      Room currentRoom = smartFoxFacade.smartFox.GetRoom
                          (smartFoxFacade.smartFox.activeRoomId);

      if (currentRoom == null)
       {
        return null;
       }

      User sfUser = currentRoom.GetUser(sfId);

      if (sfUser == null)
       {
        return null;
       }

      long   userId;
      string shortUserName;

      if (MNUtils.ParseMNUserName(sfUser.GetName(),out shortUserName,out userId))
       {
        return new MNUserInfo(userId,sfId,shortUserName,webServerUrl);
       }
      else
       {
        return null;
       }
     }

    public int GetRoomUserStatus ()
     {
      return IsOnline() ? userStatus : MNConst.MN_USER_STATUS_UNDEFINED;
     }

    public int GetRoomGameSetId ()
     {
      int? gameSetId = null;

      if (IsInGameRoom())
       {
        Room activeRoom = smartFoxFacade.smartFox.GetActiveRoom();

        if (activeRoom != null)
         {
          gameSetId = GetRoomVariableAsInt(activeRoom,SF_GAME_ROOM_VAR_NAME_GAMESET_ID);
         }
       }

      return gameSetId ?? 0;
     }

    public String GetMySId ()
     {
      if (status != MNConst.MN_OFFLINE)
       {
        return userSId;
       }
      else
       {
        return null;
       }
     }

    public void SendAppBeacon (string actionName, string beaconData)
     {
      GetTrackingSystem().SendBeacon(actionName,beaconData,0,this);
     }

    public void SendAppBeacon (string actionName, string beaconData, long beaconCallSeqNumber)
     {
      GetTrackingSystem().SendBeacon(actionName,beaconData,beaconCallSeqNumber,this);
     }

    public void ExecAppCommand(string name, string param)
     {
      if (name.StartsWith(APP_COMMAND_SET_APP_PROPERTY_PREFIX))
       {
        string varName = APP_PROPERTY_VAR_PATH_PREFIX +
                          name.Substring(APP_COMMAND_SET_APP_PROPERTY_PREFIX_LEN);

        if (param == null)
         {
          varStorage.RemoveVariablesByMask(varName);
         }
        else
         {
          varStorage.SetValue(varName,param);
         }
       }

      ExecAppCommandReceivedEventHandler handler = ExecAppCommandReceived;

      if (handler != null)
       {
        handler(name,param);
       }
     }

    public void ExecUICommand (string name, string param)
     {
      ExecUICommandReceivedEventHandler handler = ExecUICommandReceived;

      if (handler != null)
       {
        handler(name,param);
       }
     }

    public void ProcessWebEvent (string eventName, string eventParam, string callbackId)
     {
      WebEventReceivedEventHandler handler = WebEventReceived;

      if (handler != null)
       {
        handler(eventName,eventParam,callbackId);
       }
     }

    public void PostSysEvent (string eventName, string eventParam, string callbackId)
     {
      SysEventReceivedEventHandler handler = SysEventReceived;

      if (handler != null)
       {
        handler(eventName,eventParam,callbackId);
       }
     }

    public bool PreprocessAppHostCall (MNAppHostCallInfo appHostCallInfo)
     {
      String commandName = appHostCallInfo.CommandName;

      if (commandName == MNAppHostCallInfo.CommandVarSave         ||
          commandName == MNAppHostCallInfo.CommandVarsClear       ||
          commandName == MNAppHostCallInfo.CommandVarsGet         ||
          commandName == MNAppHostCallInfo.CommandSetHostParam    ||
          commandName == MNAppHostCallInfo.CommandSendHttpRequest ||
          commandName == MNAppHostCallInfo.CommandAddSourceDomain ||
          commandName == MNAppHostCallInfo.CommandRemoveSourceDomain)
       {
        return false;
       }

      AppHostCallReceivedEventHandler handler = AppHostCallReceived;

      if (handler != null)
       {
        MNAppHostCallEventArgs args = new MNAppHostCallEventArgs(appHostCallInfo);

        handler(args);

        return args.IsCancelled;
       }
      else
       {
        return false;
       }
     }

    public string GetApplicationStartParam ()
     {
      return launchParam;
     }

    public void SendPrivateMessage (string message, int toUserSFId)
     {
      if (IsOnline())
       {
        smartFoxFacade.smartFox.SendPrivateMessage(message,toUserSFId);
       }
     }

    public void SendChatMessage (string message)
     {
      if (IsOnline())
       {
        smartFoxFacade.smartFox.SendPublicMessage(message);
       }
     }

    public void SendMultiNetXtMessage (string cmd, object msgParams)
     {
      smartFoxFacade.smartFox.SendXtMessage(SMARTFOX_EXT_NAME,cmd,msgParams);
     }

    private void SendMultiNetXtRawMessage (string cmd, string msg, int roomId)
     {
      List<object> msgParams = new List<object>();

      msgParams.Add(msg);

      smartFoxFacade.smartFox.SendXtMessage
       (SMARTFOX_EXT_NAME,
        cmd,
        msgParams,
        SmartFoxClient.XTMSG_TYPE_STR,
        roomId);
     }

    private void SendMultiNetXtXmlMessage (string cmd, params string[] keyValueList)
     {
      Dictionary<object,object> msgParams = new Dictionary<object,object>();

      for (uint i = 0; i < keyValueList.Length / 2; i++)
       {
        msgParams[keyValueList[i * 2]] = keyValueList[i * 2 + 1];
       }

      SendMultiNetXtMessage(cmd,msgParams);
     }

    public void SendGameMessage (string message)
     {
      if (IsInGameRoom())
       {
        SendMultiNetXtRawMessage(SF_EXTCMD_SEND_GAME_MESSAGE,
                                 EXTCMD_SEND_GAME_MESSAGE_RAW_PREFIX +
                                  MNUtils.StringEscapeSimple
                                   (message,
                                    smartFoxFacade.smartFox.GetRawProtocolSeparator()[0],
                                    GAME_MESSAGE_ESCAPE_CHAR),
                                 smartFoxFacade.smartFox.activeRoomId);
       }
     }

    public void SendPluginMessage (string pluginName, string message)
     {
      if (IsOnline())
       {
        char rawProtocolSeparator = smartFoxFacade.smartFox.GetRawProtocolSeparator()[0];

        string escapedPluginName = MNUtils.StringEscapeCharSimple
                                    (MNUtils.StringEscapeSimple
                                      (pluginName,rawProtocolSeparator,GAME_MESSAGE_ESCAPE_CHAR),
                                     PLUGIN_MESSAGE_PLUGIN_NAME_TERM_CHAR,
                                     GAME_MESSAGE_ESCAPE_CHAR);

        string escapedMessage = MNUtils.StringEscapeCharSimple
                                 (MNUtils.StringEscapeSimple
                                   (message,rawProtocolSeparator,GAME_MESSAGE_ESCAPE_CHAR),
                                  PLUGIN_MESSAGE_PLUGIN_NAME_TERM_CHAR,
                                  GAME_MESSAGE_ESCAPE_CHAR);

        SendMultiNetXtRawMessage(SF_EXTCMD_SEND_PLUGIN_MESSAGE,
                                 EXTCMD_SEND_PLUGIN_MESSAGE_RAW_PREFIX +
                                  escapedPluginName +
                                   PLUGIN_MESSAGE_PLUGIN_NAME_TERM_CHAR +
                                    escapedMessage,
                                 smartFoxFacade.smartFox.activeRoomId);
       }
     }

    public void ReqJoinBuddyRoom (int roomSFId)
     {
      if (IsOnline())
       {
        SendMultiNetXtXmlMessage(SF_EXTCMD_JOIN_BUDDY_ROOM,
                                 SF_EXTCMD_JOIN_BUDDY_ROOM_PARAM_ROOM_SFID,roomSFId.ToString());
       }
     }

    public void SendJoinRoomInvitationResponse (MNJoinRoomInvitationParams invitationParams,
                                                bool                       accept)
     {
      if (IsOnline())
       {
        if (accept)
         {
          ReqJoinBuddyRoom(invitationParams.RoomSFId);
         }
        else
         {
          SendPrivateMessage("\tInvite reject for room:" + invitationParams.RoomSFId.ToString(),
                             invitationParams.FromUserSFId);
         }
       }
     }

    public void ReqJoinRandomRoom (string gameSetId)
     {
      if (IsOnline())
       {
        SendMultiNetXtXmlMessage(SF_EXTCMD_JOIN_RANDOM_ROOM,
                                 SF_EXTCMD_PARAM_GAMESET_ID,gameSetId);
       }
     }

    public void ReqCreateBuddyRoom (MNBuddyRoomParams buddyRoomParams)
     {
      if (IsOnline())
       {
        SendMultiNetXtXmlMessage(SF_EXTCMD_CREATE_BUDDY_ROOM,
                                 SF_EXTCMD_CREATE_BUDDY_ROOM_PARAM_ROOM_NAME,buddyRoomParams.RoomName,
                                 SF_EXTCMD_PARAM_GAMESET_ID,buddyRoomParams.GameSetId.ToString(),
                                 SF_EXTCMD_CREATE_BUDDY_ROOM_PARAM_TO_USERID_LIST,buddyRoomParams.ToUserIdList,
                                 SF_EXTCMD_CREATE_BUDDY_ROOM_PARAM_TO_USERSFID_LIST,buddyRoomParams.ToUserSFIdList,
                                 SF_EXTCMD_CREATE_BUDDY_ROOM_PARAM_MSG_TEXT,buddyRoomParams.InviteText);
       }
     }

    public void ReqStartBuddyRoomGame ()
     {
      if (status == MNConst.MN_IN_GAME_WAIT)
       {
        SendMultiNetXtXmlMessage(SF_EXTCMD_START_BUDDY_ROOM_GAME);
       }
      else
       {
        DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_START_BUDDY_ROOM_GAME,
                              MNI18n.GetLocalizedString
                               ("Room not ready",
                                MNI18n.MESSAGE_CODE_ROOM_IS_NOT_READY_TO_START_A_GAME_ERROR));
       }
     }

    public void ReqStopRoomGame ()
     {
      if (status == MNConst.MN_IN_GAME_PLAY)
       {
        SendMultiNetXtXmlMessage(SF_EXTCMD_STOP_ROOM_GAME);
       }
     }

    public void ReqCurrentGameResults ()
     {
      if (IsInGameRoom())
       {
        SendMultiNetXtXmlMessage(SF_EXTCMD_CURR_GAME_RESULTS);
       }
     }

    public void ReqSetUserStatus (int userStatus)
     {
      if (IsInGameRoom())
       {
        if (userStatus == MNConst.MN_USER_PLAYER || userStatus == MNConst.MN_USER_CHATER)
         {
          SendMultiNetXtXmlMessage(SF_EXTCMD_SET_USER_STATUS,
                                   SF_EXTCMD_SET_USER_STATUS_PARAM_USER_STATUS,userStatus.ToString());
         }
        else
         {
          DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_SET_USER_STATUS,
                                MNI18n.GetLocalizedString
                                 ("invalid player status value",
                                  MNI18n.MESSAGE_CODE_INVALID_PLAYER_STATUS_VALUE_ERROR));
         }
       }
     }

    public void StartGameWithParams (MNGameParams gameParams)
     {
      DoStartGameWithParamsEventHandler handler = DoStartGameWithParams;

      if (handler != null)
       {
        handler(gameParams);
       }
     }

    private void StartGameWithParamsFromActiveRoom ()
     {
      Room   activeRoom    = smartFoxFacade.smartFox.GetActiveRoom();
      int?   gameSetId     = GetRoomVariableAsInt(activeRoom,SF_GAME_ROOM_VAR_NAME_GAMESET_ID);
      string gameSetParams = null;
      int?   gameSeed      = GetRoomVariableAsInt(activeRoom,SF_GAME_ROOM_VAR_NAME_GAME_SEED);

      try
       {
        gameSetParams = activeRoom.GetVariable(SF_GAME_ROOM_VAR_NAME_GAMESET_PARAM) as string;
       }
      catch (KeyNotFoundException)
       {
       }

      if (gameSetId != null && gameSetParams != null && gameSeed != null)
       {
        MNGameParams gameParams =
         new MNGameParams(gameSetId.Value,gameSetParams,"",
                          gameSeed.Value,MNGameParams.MN_PLAYMODEL_MULTIPLAY);

        Dictionary<string,Object> roomVars = activeRoom.GetVariables();

        foreach (var Variable in roomVars)
         {
          if (Variable.Key.StartsWith(SF_GAMESET_PLAY_PARAM_VAR_NAME_PREFIX))
           {
            string value = Variable.Value as string;

            if (value != null)
             {
              gameParams.AddGameSetPlayParam
               (Variable.Key.Substring(SF_GAMESET_PLAY_PARAM_VAR_NAME_PREFIX_LEN),value);
             }
           }
         }

        StartGameWithParams(gameParams);
       }
     }

    public void FinishGameWithResult (MNGameResult gameResult)
     {
      if (IsUserLoggedIn())
       {
        if (IsOnline())
         {
          if ((status == MNConst.MN_IN_GAME_PLAY || status == MNConst.MN_IN_GAME_END) &&
              (userStatus == MNConst.MN_USER_PLAYER))
           {
            SendMultiNetXtXmlMessage(SF_EXTCMD_FINISH_GAME_IN_ROOM,
                                     SF_EXTCMD_FINISH_PARAM_SCORE,gameResult.Score.ToString(),
                                     SF_EXTCMD_FINISH_PARAM_OUT_TIME,"-1");
           }
          else
           {
            SendMultiNetXtXmlMessage(SF_EXTCMD_FINISH_GAME_PLAIN,
                                     SF_EXTCMD_FINISH_PARAM_SCORE,gameResult.Score.ToString(),
                                     SF_EXTCMD_FINISH_PARAM_OUT_TIME,"-1",
                                     SF_EXTCMD_FINISH_PARAM_SCORE_POSTLINK_ID,gameResult.ScorePostLinkId,
                                     SF_EXTCMD_PARAM_GAMESET_ID,gameResult.GameSetId.ToString());

           }
         }
        else
         {
          MNOfflineScores.SaveScore
           (varStorage,userId,gameResult.GameSetId,gameResult.Score);
         }
       }

      GameFinishedWithResultEventHandler handler = GameFinishedWithResult;

      if (handler != null)
       {
        handler(gameResult);
       }
     }

    public void SchedulePostScoreOnLogin (MNGameResult gameResult)
     {
      pendingGameResult = gameResult;
     }

    public void CancelPostScoreOnLogin ()
     {
      pendingGameResult = null;
     }

    public void CancelGameWithParams (MNGameParams gameParams)
     {
      if (IsInGameRoom() && userStatus == MNConst.MN_USER_PLAYER)
       {
        ReqSetUserStatus(MNConst.MN_USER_CHATER);
       }
     }

    public void SetDefaultGameSetId (int gameSetId)
     {
      defaultGameSetId = gameSetId;

      DefaultGameSetIdChangedToEventHandler handler = DefaultGameSetIdChangedTo;

      if (handler != null)
       {
        handler(gameSetId);
       }
     }

    public int GetDefaultGameSetId ()
     {
      return defaultGameSetId;
     }

    public void LeaveRoom ()
     {
      if (IsInGameRoom())
       {
        SendMultiNetXtXmlMessage(SF_EXTCMD_LEAVE_ROOM);
       }
     }

    public SmartFoxClient GetSmartFox ()
     {
      return smartFoxFacade.smartFox;
     }

    private void SetNewStatus (int newStatus)
     {
      int oldStatus = status;

      status = newStatus;

      SessionStatusChangedEventHandler handler = SessionStatusChanged;

      if (handler != null)
       {
        handler(newStatus,oldStatus);
       }
     }

    #region MNSmartFoxFacade event handlers

    private void OnSmartFoxFacadePreLoginSucceeded (long userId, string userName, string SId, int lobbyRoomId, string userAuthSign)
     {
      bool userChanged = this.userId != userId;

      string tempUserName;
      long   tempUserId;

      if (MNUtils.ParseMNUserName(userName,out tempUserName,out tempUserId))
       {
        this.userName = tempUserName;
       }
      else
       {
        this.userName = userName;
       }

      this.userId      = userId;
      this.userSId     = SId;
      this.lobbyRoomId = lobbyRoomId;

      if (userAuthSign != null && userAuthSign.Length > 0)
       {
        if (smartFoxFacade.GetLoginInfoLogin() == LOGIN_MODEL_GUEST_USER_LOGIN)
         {
          smartFoxFacade.UpdateLoginInfo
           (userId.ToString(),
            MakeStructuredPasswordFromParams
             (LOGIN_MODEL_AUTH_SIGN,gameSecret,userAuthSign,true));
         }

        MNUserCredentials.UpdateCredentials
         (varStorage,new MNUserCredentials(userId,this.userName,userAuthSign,DateTime.Now,null));

        DispatchDevUsersInfoChanged();

        varStorage.WriteToFile();
       }

      if (userChanged)
       {
        DispatchUserChangedEvent(userId);
       }
     }

    private void OnSmartFoxFacadeLoginSucceeded    ()
     {
      SetNewStatus(MNConst.MN_LOGGEDIN);
     }

    private void OnSmartFoxFacadeLoginFailed (string errorMessage)
     {
      SetNewStatus(MNConst.MN_OFFLINE);

      DispatchLoginFailed(errorMessage);
     }

    private void OnSmartFoxFacadeConnectionLost ()
     {
      userSId     = null;
      lobbyRoomId = MNSession.MN_LOBBY_ROOM_ID_UNDEFINED;

      if (status != MNConst.MN_OFFLINE)
       {
        SetNewStatus(MNConst.MN_OFFLINE);
       }

      if (OFFLINE_MODE_DISABLED)
       {
        if (userId != MNConst.MN_USER_ID_UNDEFINED)
         {
          userId   = MNConst.MN_USER_ID_UNDEFINED;
          userName = null;

          DispatchUserChangedEvent(userId);
         }
       }
     }

    private void OnSmartFoxFacadeConfigLoadStarted ()
     {
      ConfigLoadStartedEventHandler handler = ConfigLoadStarted;

      if (handler != null)
       {
        handler();
       }
     }

    private void OnSmartFoxFacadeConfigLoaded ()
     {
      useInstallIdInsteadOfUDID = smartFoxFacade.configData.useInstallIdInsteadOfUDID != 0;

      MNTrackingSystem trackingSystem = GetTrackingSystem();

      if (smartFoxFacade.configData.launchTrackerUrl != null)
       {
        trackingSystem.TrackLaunch(smartFoxFacade.configData.launchTrackerUrl,this);
       }

      if (smartFoxFacade.configData.installTrackerUrl != null)
       {
        trackingSystem.TrackInstallWithUrlTemplate(smartFoxFacade.configData.installTrackerUrl,this);
       }

      if (smartFoxFacade.configData.shutdownTrackerUrl != null)
       {
        trackingSystem.SetShutdownUrlTemplate(smartFoxFacade.configData.shutdownTrackerUrl);
       }

      if (smartFoxFacade.configData.beaconTrackerUrl != null)
       {
        trackingSystem.SetBeaconUrlTemplate(smartFoxFacade.configData.beaconTrackerUrl);
       }

      socNetSessionFB.SetAppId(smartFoxFacade.configData.facebookAppId);

      webServerUrl = smartFoxFacade.configData.webServerUrl;

      WebFrontURLReadyEventHandler webFrontURLReadyEventHandler = WebFrontURLReady;

      if (webFrontURLReadyEventHandler != null)
       {
        webFrontURLReadyEventHandler(webServerUrl);
       }

      ConfigLoadedEventHandler configLoadedEventHandler = ConfigLoaded;

      if (configLoadedEventHandler != null)
       {
        configLoadedEventHandler();
       }

      if      (fastResumeEnabled && smartFoxFacade.configData.tryFastResumeMode == 2)
       {
        fastResumeEnabled = false;

        LoginAuto();
       }
      else if (fastResumeEnabled && smartFoxFacade.configData.tryFastResumeMode != 0)
       {
        fastResumeEnabled = false;

        LoginWithStoredCredentials();
       }
     }

    private void OnSmartFoxFacadeConfigLoadFailed (string error)
     {
      DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_LOAD_CONFIG,error);
     }
    #endregion

    #region tracking system event handlers
    private void OnAppBeaconResponseReceived (MNAppBeaconResponse response)
     {
      AppBeaconResponseReceivedEventHandler handler = AppBeaconResponseReceived;

      if (handler != null)
       {
        handler(response);
       }
     }
    #endregion

    #region Variable storage methods
    public void VarStorageSetValue (string name, string value)
     {
      varStorage.SetValue(name,value);

      DispatchDevUsersInfoChanged();

      varStorage.WriteToFile();
     }

    public string VarStorageGetValueForVariable (string name)
     {
      return varStorage.GetValue(name);
     }

    public Dictionary<string,string> VarStorageGetValuesByMasks (string[] masks)
     {
      return varStorage.GetVariablesByMasks(masks);
     }

    public void VarStorageRemoveVariablesByMask (string mask)
     {
      varStorage.RemoveVariablesByMask(mask);

      DispatchDevUsersInfoChanged();

      varStorage.WriteToFile();
     }

    public void VarStorageRemoveVariablesByMasks (string[] masks)
     {
      varStorage.RemoveVariablesByMasks(masks);

      DispatchDevUsersInfoChanged();

      varStorage.WriteToFile();
     }

    public MNVarStorage GetVarStorage ()
     {
      return varStorage;
     }
    #endregion

    internal long GetLaunchTime ()
     {
      return launchTime;
     }

    internal string GetLaunchId ()
     {
      return launchId;
     }

    internal string GetInstallId ()
     {
      return installId;
     }

    public string GetUniqueAppId ()
     {
      if (useInstallIdInsteadOfUDID)
       {
        return installId;
       }
      else
       {
        return MNPlatformWinPhone.GetUniqueDeviceIdentifier();
       }
     }

    internal MNTrackingSystem GetTrackingSystem ()
     {
      lock (thisLock)
       {
        if (trackingSystem == null)
         {
          trackingSystem = new MNTrackingSystem(this);

          trackingSystem.AppBeaconResponseReceived += OnAppBeaconResponseReceived;
         }

        return trackingSystem;
       }
     }

    internal Dictionary<string,string> GetAppConfigVars ()
     {
      return appConfigVars;
     }

    internal MNConfigData GetConfigData ()
     {
      return smartFoxFacade.configData;
     }

    internal void SetWebShopReady (bool webShopIsReady)
     {
      this.webShopIsReady = webShopIsReady;

      VShopReadyStatusChangedEventHandler handler = VShopReadyStatusChanged;

      if (handler  != null)
       {
        handler(webShopIsReady);
       }
     }

    public bool IsWebShopReady ()
     {
      return webShopIsReady;
     }

    public MNGameVocabulary GetGameVocabulary ()
     {
      return gameVocabulary;
     }

    #region helper internal methods
    private static bool IsStatusValid (int status)
     {
      return status == MNConst.MN_OFFLINE       || status == MNConst.MN_CONNECTING   ||
             status == MNConst.MN_LOGGEDIN      || status == MNConst.MN_IN_GAME_WAIT ||
             status == MNConst.MN_IN_GAME_START || status == MNConst.MN_IN_GAME_PLAY ||
             status == MNConst.MN_IN_GAME_END;
     }

    private int? GetSFVariableValueAsInt (object value)
     {
      if (value == null)
       {
        return null;
       }

      if (value is string)
       {
        return MNUtils.ParseInt((string)value);
       }

      if (value is double)
       {
        return (int)(double)value;
       }

      return null;
     }

    private int? GetUserVariableAsInteger (string name)
     {
      Room   room  = smartFoxFacade.smartFox.GetActiveRoom();
      User   user  = room.GetUser(smartFoxFacade.smartFox.myUserId);

      try
       {
        object value = user.GetVariable(name);

        return GetSFVariableValueAsInt(value);
       }
      catch (KeyNotFoundException)
       {
        return null;
       }
     }

    internal int? GetRoomVariableAsInt (Room room, string name)
     {
      try
       {
        return GetSFVariableValueAsInt(room.GetVariable(name));
       }
      catch (KeyNotFoundException)
       {
        return null;
       }
     }

    private int? GetRoomUserStatusVariable ()
     {
      return GetUserVariableAsInteger(SF_GAME_USER_VAR_NAME_USER_STATUS);
     }
    #endregion

    #region SmartFox event handlers

    private void OnSmartFoxMessage (string message, User sender, bool isPrivate)
     {
      MNUserInfo userInfo;

      if (sender != null)
       {
        userInfo = CreateUserInfoBySFIdAndName(sender.GetId(),sender.GetName());
       }
      else
       {
        userInfo = new MNUserInfo(MNConst.MN_USER_ID_UNDEFINED,
                                  MNConst.MN_USER_SFID_UNDEFINED,
                                  "",
                                  webServerUrl);
       }

      MNChatMessage chatMessage = new MNChatMessage
                                       (userInfo,
                                        message == null ? "" : message,
                                        isPrivate);

      if (isPrivate)
       {
        ChatPrivateMessageReceivedEventHandler handler = ChatPrivateMessageReceived;

        if (handler != null)
         {
          handler(chatMessage);
         }
       }
      else
       {
        ChatPublicMessageReceivedEventHandler handler = ChatPublicMessageReceived;

        if (handler != null)
         {
          handler(chatMessage);
         }
       }
     }

    private void OnSmartFoxPublicMessage (string message, User sender, int roomId)
     {
      OnSmartFoxMessage(message,sender,false);
     }

    private void OnSmartFoxPrivateMessage (string message, User sender, int roomId, int userSFId)
     {
      if (sender == null || sender.GetId() != smartFoxFacade.smartFox.myUserId)
       {
        OnSmartFoxMessage(message,sender,true);
       }
     }

    private void OnSmartFoxJoinRoom (Room room)
     {
      bool needStartGame = false;
      int  oldUserStatus = userStatus;

      roomExtraInfoReceived = false;

      if (status == MNConst.MN_LOGGEDIN &&
          lobbyRoomId != MNSession.MN_LOBBY_ROOM_ID_UNDEFINED &&
          room.GetId() != lobbyRoomId)
       {
        int? newStatusVal  = GetRoomVariableAsInt(room,SF_GAME_ROOM_VAR_NAME_GAME_STATUS);
        int? newUserStatus = GetRoomUserStatusVariable();

        if (newUserStatus != null)
         {
          userStatus = newUserStatus.Value;
         }

        if (newStatusVal != null)
         {
          int newStatus = newStatusVal.Value;

          if (IsStatusValid(newStatus))
           {
            SetNewStatus(newStatus);

            if (newStatus  == MNConst.MN_IN_GAME_PLAY &&
                userStatus == MNConst.MN_USER_PLAYER &&
                roomExtraInfoReceived)
             {
              needStartGame = true;
             }
           }
         }
       }
      else if (lobbyRoomId != MNSession.MN_LOBBY_ROOM_ID_UNDEFINED &&
               room.GetId() == lobbyRoomId)
       {
        SetNewStatus(MNConst.MN_LOGGEDIN);

        if (pendingGameResult != null)
         {
          FinishGameWithResult(pendingGameResult);

          pendingGameResult = null;
         }
       }

      if (userStatus != oldUserStatus)
       {
        DispatchRoomUserStatusChanged(userStatus);
       }

      if (needStartGame)
       {
        StartGameWithParamsFromActiveRoom();
       }
     }

    private void OnSmartFoxRoomVariablesUpdate (Room room, Dictionary<string,object> changedVars)
     {
      if (!IsInGameRoom())
       {
        return;
       }

      bool needStartGame  = false;
      bool needFinishGame = false;
      bool needCancelGame = false;

      foreach (var ChangedVar in changedVars)
       {
        if (ChangedVar.Key == SF_GAME_ROOM_VAR_NAME_GAME_STATUS)
         {
          int? newStatusValue = GetRoomVariableAsInt(room,SF_GAME_ROOM_VAR_NAME_GAME_STATUS);

          if (newStatusValue != null && IsStatusValid(newStatusValue.Value))
           {
            int newStatus = newStatusValue.Value;

            if (userStatus == MNConst.MN_USER_PLAYER)
             {
              if (newStatus == MNConst.MN_IN_GAME_PLAY && roomExtraInfoReceived)
               {
                needStartGame = true;
               }
              else if (newStatus == MNConst.MN_IN_GAME_END)
               {
                needFinishGame = true;
               }
              else if (status == MNConst.MN_IN_GAME_PLAY &&
                       newStatus == MNConst.MN_IN_GAME_WAIT)
               {
                needCancelGame = true;
               }
             }

            SetNewStatus(newStatus);
           }
         }
        else if (ChangedVar.Key == SF_GAME_ROOM_VAR_NAME_GAME_START_COUNTDOWN)
         {
          int? secondsLeft= GetRoomVariableAsInt(room,SF_GAME_ROOM_VAR_NAME_GAME_START_COUNTDOWN);

          if (secondsLeft != null)
           {
            DispatchGameStartCountdownTick(secondsLeft.Value);
           }
         }
       }

      if      (needStartGame)
       {
        StartGameWithParamsFromActiveRoom();
       }
      else if (needFinishGame)
       {
        DispatchDoFinishGame();
       }
      else if (needCancelGame)
       {
        DispatchDoCancelGame();
       }
     }

    private void OnSmartFoxUserVariablesUpdate (User user, Dictionary<string,object> changedVars)
     {
      if (user.GetId() != smartFoxFacade.smartFox.myUserId)
       {
        return;
       }

      if (changedVars.ContainsKey(SF_GAME_USER_VAR_NAME_USER_STATUS))
       {
        int  oldUserStatus = userStatus;
        int? newUserStatus = GetRoomUserStatusVariable();

        if (newUserStatus != null)
         {
          if (status == MNConst.MN_IN_GAME_PLAY)
           {
            if (userStatus == MNConst.MN_USER_CHATER &&
                newUserStatus == MNConst.MN_USER_PLAYER &&
                roomExtraInfoReceived)
             {
              StartGameWithParamsFromActiveRoom();
             }
            else if (userStatus == MNConst.MN_USER_PLAYER &&
                     newUserStatus == MNConst.MN_USER_CHATER)
             {
              DispatchDoCancelGame();
             }
           }

          userStatus = newUserStatus.Value;
         }
        else
         {
          userStatus = MNConst.MN_USER_STATUS_UNDEFINED;
         }

        if (oldUserStatus != userStatus)
         {
          DispatchRoomUserStatusChanged(userStatus);
         }
       }
     }

    private void OnSmartFoxUserEnterRoom (int roomId, User user)
     {
      MNUserInfo userInfo = CreateUserInfoBySFIdAndName(user.GetId(),user.GetName());

      DispatchRoomUserJoin(userInfo);
     }

    private void OnSmartFoxUserLeaveRoom (int roomId, int userSFId, string userName)
     {
      MNUserInfo userInfo = CreateUserInfoBySFIdAndName(userSFId,userName);

      DispatchRoomUserLeave(userInfo);
     }

    private void OnSmartFoxExtCurrGameResultsResponse (SFSObject data)
     {
      long[] userIdArray    = MNUtils.ParseCSLongs(data.GetString(SF_EXTRSP_CURR_GAME_RESULTS_PARAM_USERID_LIST));
      int[] userSFIdArray   = MNUtils.ParseCSInts(data.GetString(SF_EXTRSP_CURR_GAME_RESULTS_PARAM_USERSFID_LIST));
      int[] placeArray      = MNUtils.ParseCSInts(data.GetString(SF_EXTRSP_CURR_GAME_RESULTS_PARAM_USER_PLACE_LIST));
      long[] scoreArray     = MNUtils.ParseCSLongs(data.GetString(SF_EXTRSP_CURR_GAME_RESULTS_PARAM_USER_SCORE_LIST));
      int? resultIsFinal    = MNUtils.ParseInt(data.GetString(SF_EXTRSP_CURR_GAME_RESULTS_PARAM_RESULT_IS_FINAL));
      int? gameId           = MNUtils.ParseInt(data.GetString(SF_EXTRSP_CURR_GAME_RESULTS_PARAM_GAME_ID));
      int? gameSetId        = MNUtils.ParseInt(data.GetString(SF_EXTRSP_CURR_GAME_RESULTS_PARAM_GAMESET_ID));
      long? playRoundNumber = MNUtils.ParseLong(data.GetString(SF_EXTRSP_CURR_GAME_RESULTS_PARAM_PLAYROUND_NUMBER));

      if (userIdArray == null || userSFIdArray == null || placeArray == null ||
          scoreArray == null || resultIsFinal == null || gameId == null ||
          gameSetId == null || playRoundNumber == null)
       {
        return;
       }

      if (userIdArray.Length != userSFIdArray.Length ||
          userIdArray.Length != placeArray.Length ||
          userIdArray.Length != scoreArray.Length)
       {
        return;
       }

      Room activeRoom    = smartFoxFacade.smartFox.GetActiveRoom();
      MNUserInfo[] users = new MNUserInfo[userIdArray.Length];

      for (int index = 0; index < userSFIdArray.Length; index++)
       {
        User sfUser = activeRoom.GetUser(userSFIdArray[index]);
        String name = null;

        if (sfUser != null)
         {
          long tempUserId;

          if (!MNUtils.ParseMNUserName(sfUser.GetName(),out name, out tempUserId))
           {
            name = sfUser.GetName();
           }
         }

        users[index] = new MNUserInfo(userIdArray[index],
                                      userSFIdArray[index],
                                      name,
                                      webServerUrl);
       }

      MNCurrGameResults results =
       new MNCurrGameResults(gameId.Value,gameSetId.Value,
                             resultIsFinal != 0 ? true : false,
                             playRoundNumber.Value,
                             placeArray,scoreArray,users);

      CurrGameResultsReceivedEventHandler handler = CurrGameResultsReceived;

      if (handler != null)
       {
        handler(results);
       }
     }

    private void OnSmartFoxExtJoinRoomInvitation (SFSObject data)
     {
      int?   fromUserSFId  = MNUtils.ParseInt(data.GetString(SF_EXTCMD_JOIN_ROOM_INV_PARAM_FROM_USERSFID));
      string fromUserName  = data.GetString(SF_EXTCMD_JOIN_ROOM_INV_PARAM_FROM_USERNAME);
      int?   roomSFId      = MNUtils.ParseInt(data.GetString(SF_EXTCMD_JOIN_ROOM_INV_PARAM_ROOM_SFID));
      string roomName      = data.GetString(SF_EXTCMD_JOIN_ROOM_INV_PARAM_ROOM_NAME);
      int?   roomGameId    = MNUtils.ParseInt(data.GetString(SF_EXTCMD_JOIN_ROOM_INV_PARAM_ROOM_GAME_ID));
      int?   roomGameSetId = MNUtils.ParseInt(data.GetString(SF_EXTCMD_JOIN_ROOM_INV_PARAM_ROOM_GAMESET_ID));
      string messText      = data.GetString(SF_EXTCMD_JOIN_ROOM_INV_PARAM_MSG_TEXT);

      if (fromUserSFId != null && fromUserName != null && roomSFId != null &&
          roomName != null && roomGameId != null && roomGameSetId != null &&
          messText != null)
       {
        MNJoinRoomInvitationParams joinRoomInvitationParams =
         new MNJoinRoomInvitationParams(fromUserSFId.Value,fromUserName,roomSFId.Value,
                                        roomName,roomGameId.Value,roomGameSetId.Value,
                                        messText);

        JoinRoomInvitationReceivedEventHandler handler = JoinRoomInvitationReceived;

        if (handler != null)
         {
          handler(joinRoomInvitationParams);
         }
       }
     }

    private void OnSmartFoxExtInitRoomUserInfo ()
     {
      roomExtraInfoReceived = true;

      if (status     == MNConst.MN_IN_GAME_PLAY &&
          userStatus == MNConst.MN_USER_PLAYER)
       {
        StartGameWithParamsFromActiveRoom();
       }
     }

    private void OnSmartFoxExtError (SFSObject data)
     {
      string errorCall    = data.GetString(SF_EXTCMD_ERROR_PARAM_CALL);
      string errorMessage = data.GetString(SF_EXTCMD_ERROR_PARAM_ERROR_MSG);

      if (errorCall == null)
       {
        MNDebug.debug("errorCall is undefined in extension error message");

        return;
       }

      if (errorMessage == null)
       {
        MNDebug.debug("errorMessage is undefined in extension error message");

        return;
       }

      if (errorCall == SF_EXTCMD_JOIN_RANDOM_ROOM ||
          errorCall == SF_EXTCMD_JOIN_BUDDY_ROOM)
       {
        DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_JOIN_GAME_ROOM,errorMessage);
       }
      else if (errorCall == SF_EXTCMD_FINISH_GAME_IN_ROOM ||
               errorCall == SF_EXTCMD_FINISH_GAME_PLAIN)
       {
        DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_POST_GAME_RESULT,errorMessage);
       }
      else if (errorCall == SF_EXTCMD_CREATE_BUDDY_ROOM)
       {
        DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_CREATE_BUDDY_ROOM,errorMessage);
       }
      else if (errorCall == SF_EXTCMD_LEAVE_ROOM)
       {
        DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_LEAVE_ROOM,errorMessage);
       }
      else if (errorCall == SF_EXTCMD_START_BUDDY_ROOM_GAME)
       {
        DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_START_BUDDY_ROOM_GAME,errorMessage);
       }
      else if (errorCall == SF_EXTCMD_STOP_ROOM_GAME)
       {
        DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_STOP_ROOM_GAME,errorMessage);
       }
      else if (errorCall == SF_EXTCMD_SET_USER_STATUS)
       {
        DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_SET_USER_STATUS,errorMessage);
       }
      else
       {
        MNDebug.debug("unknown errorCall (" + errorCall + ") in extension error message");
       }
     }

    private void OnSmartFoxExtRawMessage (string message)
     {
      bool   isGameMessage   = false;
      bool   isPluginMessage = false;
      string gameMessage     = null;

      if      (message.StartsWith(EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX))
       {
        isGameMessage = true;
        gameMessage   = message.Substring(EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX_LEN);
       }
      else if (message.StartsWith(EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX2))
       {
        isGameMessage = true;
        gameMessage   = message.Substring(EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX2_LEN);
       }
      else if (message.StartsWith(EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX))
       {
        isPluginMessage = true;
        gameMessage     = message.Substring(EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX_LEN);
       }
      else if (message.StartsWith(EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX2))
       {
        isPluginMessage = true;
        gameMessage     = message.Substring(EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX2_LEN);
       }

      MNUserInfo senderInfo  = null;
      bool       validFormat = false;

      if (isGameMessage || isPluginMessage)
       {
        if (gameMessage.Length > 0)
         {
          char startChar = gameMessage[0];

          if      (startChar == '~') // there is no sender info
           {
            validFormat = true;
            gameMessage = gameMessage.Substring(1);
           }
          else if (startChar == '^') // sender info present in message
           {
            int endPos = gameMessage.IndexOf('~',1);

            if (endPos > 0)
             {
              int? senderSFId = MNUtils.ParseInt(gameMessage.Substring(1,endPos));

              if (senderSFId != null)
               {
                validFormat = true;

                senderInfo = CreateUserInfoBySFIdAndName
                              (senderSFId.Value,
                               smartFoxFacade.GetUserNameBySFId(senderSFId.Value));

                gameMessage = gameMessage.Substring(endPos + 1);
               }
             }
           }
         }
       }

      if (isGameMessage && validFormat)
       {
        gameMessage = MNUtils.StringUnEscapeSimple
                       (gameMessage,
                        smartFoxFacade.smartFox.GetRawProtocolSeparator()[0],
                        GAME_MESSAGE_ESCAPE_CHAR);

        GameMessageReceivedEventHandler handler = GameMessageReceived;

        if (handler != null)
         {
          handler(gameMessage,senderInfo);
         }
       }
      else if (isPluginMessage && validFormat)
       {
        int offset = gameMessage.IndexOf(PLUGIN_MESSAGE_PLUGIN_NAME_TERM_CHAR);

        if (offset >= 0)
         {
          string pluginName = gameMessage.Substring(0,offset);

          gameMessage = gameMessage.Substring(offset + 1);

          pluginName = MNUtils.StringUnEscapeSimple
                        (MNUtils.StringUnEscapeCharSimple
                          (pluginName,
                           PLUGIN_MESSAGE_PLUGIN_NAME_TERM_CHAR,
                           GAME_MESSAGE_ESCAPE_CHAR),
                         smartFoxFacade.smartFox.GetRawProtocolSeparator()[0],
                         GAME_MESSAGE_ESCAPE_CHAR);

          gameMessage = MNUtils.StringUnEscapeSimple
                         (MNUtils.StringUnEscapeCharSimple
                           (gameMessage,
                            PLUGIN_MESSAGE_PLUGIN_NAME_TERM_CHAR,
                            GAME_MESSAGE_ESCAPE_CHAR),
                          smartFoxFacade.smartFox.GetRawProtocolSeparator()[0],
                          GAME_MESSAGE_ESCAPE_CHAR);

          PluginMessageReceivedEventHandler handler = PluginMessageReceived;

          if (handler != null)
           {
            handler(pluginName,gameMessage,senderInfo);
           }
         }
       }
     }

    private void OnSmartFoxExtensionResponse (object data, string type)
     {
      if      (type == SmartFoxClient.XTMSG_TYPE_STR)
       {
        // Silverlight version of SmartFox API may pass several string messages as a single line
        // Current version passes str + "%nn" sequence, may be due a bug in message parser. This "nn" message will be
        // ignored by OnSmartFoxExtRawMessage since it does not have required prefix.
        string[] messages = ((string)data).Split(smartFoxFacade.smartFox.GetRawProtocolSeparator().ToCharArray());

        foreach (string message in messages)
         {
          OnSmartFoxExtRawMessage(message);
         }
       }
      else if (type == SmartFoxClient.XTMSG_TYPE_XML)
       {
        SFSObject cmdData = (SFSObject)data;
        string    cmd     = cmdData.GetString("_cmd");

        if (cmd != null)
         {
          if      (cmd == SF_EXTCMD_JOIN_ROOM_INVITATION)
           {
            OnSmartFoxExtJoinRoomInvitation(cmdData);
           }
          else if (cmd == SF_EXTCMD_CURR_GAME_RESULTS)
           {
            OnSmartFoxExtCurrGameResultsResponse(cmdData);
           }
          else if (cmd == SF_EXTCMD_INIT_ROOM_USER_INFO)
           {
            OnSmartFoxExtInitRoomUserInfo();
           }
          else if (cmd == SF_EXTCMD_ERROR)
           {
            OnSmartFoxExtError(cmdData);
           }
         }
       }
     }
    #endregion

    #region Facebook support

    public class SocNetFBEventArgs
     {
      public bool   Failed       { get; internal set; }
      public bool   Cancelled    { get; internal set; }
      public string ErrorMessage { get; internal set; }

      public SocNetFBEventArgs ()
       {
        Failed       = false;
        Cancelled    = false;
        ErrorMessage = null;
       }
     }

    public delegate void SocNetFBEventHandler (SocNetFBEventArgs args);

    public void SocNetFBConnect (SocNetFBEventHandler eventHandler, string[] permissions)
     {
      if (status != MNConst.MN_OFFLINE && status != MNConst.MN_LOGGEDIN)
       {
        SocNetFBEventArgs args = new SocNetFBEventArgs();
        args.Failed       = true;
        args.ErrorMessage = MNI18n.GetLocalizedString
                             ("You must not be in the gameplay to use Facebook connect",
                               MNI18n.MESSAGE_CODE_YOU_MUST_NOT_BE_IN_GAMEPLAY_TO_USE_FACEBOOK_CONNECT_ERROR);
        eventHandler(args);
       }

      socNetSessionFB.Authorize(permissions,(result) =>
       {
        SocNetFBEventArgs args = new SocNetFBEventArgs();

        if (result.Cancelled)
         {
          args.Cancelled = true;
         }
        else if (!result.Succeeded)
         {
          args.Failed       = true;
          args.ErrorMessage = result.ErrorMessage;
         }

         eventHandler(args);
       });
     }

    public void SocNetFBResume (SocNetFBEventHandler eventHandler)
     {
      if (status != MNConst.MN_OFFLINE && status != MNConst.MN_LOGGEDIN)
       {
        SocNetFBEventArgs args = new SocNetFBEventArgs();
        args.Failed       = true;
        args.ErrorMessage = MNI18n.GetLocalizedString
                             ("You must not be in the gameplay to use Facebook connect",
                               MNI18n.MESSAGE_CODE_YOU_MUST_NOT_BE_IN_GAMEPLAY_TO_USE_FACEBOOK_CONNECT_ERROR);
        eventHandler(args);
       }

      socNetSessionFB.Resume((result) =>
       {
        SocNetFBEventArgs args = new SocNetFBEventArgs();

        if (result.Cancelled)
         {
          args.Cancelled = true;
         }
        else if (!result.Succeeded)
         {
          args.Failed       = true;
          args.ErrorMessage = result.ErrorMessage;
         }

         eventHandler(args);
       });
     }

    public void SocNetFBLogout ()
     {
      socNetSessionFB.Logout();
     }

    public MNSocNetSessionFB GetSocNetSessionFB ()
     {
      return socNetSessionFB;
     }

    #endregion

    #region event dispatchers
    private void DispatchDevUsersInfoChanged ()
     {
      DevUsersInfoChangedEventHandler handler = DevUsersInfoChanged;

      if (handler != null)
       {
        handler();
       }
     }

    private void DispatchUserChangedEvent (long id)
     {
      UserChangedEventHandler handler = UserChanged;

      if (handler != null)
       {
        handler(id);
       }
     }

    private void DispatchRoomUserStatusChanged (int userStatus)
     {
      RoomUserStatusChangedEventHandler handler = RoomUserStatusChanged;

      if (handler != null)
       {
        handler(userStatus);
       }
     }

    private void DispatchGameStartCountdownTick (int secondsLeft)
     {
      GameStartCountdownTickEventHandler handler = GameStartCountdownTick;

      if (handler != null)
       {
        handler(secondsLeft);
       }
     }

    private void DispatchDoFinishGame ()
     {
      DoFinishGameEventHandler handler = DoFinishGame;

      if (handler != null)
       {
        handler();
       }
     }

    private void DispatchDoCancelGame ()
     {
      DoCancelGameEventHandler handler = DoCancelGame;

      if (handler != null)
       {
        handler();
       }
     }

    private void DispatchRoomUserJoin (MNUserInfo userInfo)
     {
      RoomUserJoinEventHandler handler = RoomUserJoin;

      if (handler != null)
       {
        handler(userInfo);
       }
     }

    private void DispatchRoomUserLeave (MNUserInfo userInfo)
     {
      RoomUserLeaveEventHandler handler = RoomUserLeave;

      if (handler != null)
       {
        handler(userInfo);
       }
     }

    private void DispatchErrorOccurred (int errorCode, string errorMessage)
     {
      ErrorOccurredEventHandler handler = ErrorOccurred;

      if (handler != null)
       {
        handler(new MNErrorInfo(errorCode,errorMessage));
       }
     }

    private void DispatchLoginFailed (string errorMessage)
     {
      DispatchErrorOccurred(MNErrorInfo.ACTION_CODE_LOGIN,errorMessage);
     }
    #endregion

    private int              gameId;
    private string           gameSecret;
    private MNSmartFoxFacade smartFoxFacade;
    private MNSocNetSessionFB socNetSessionFB;
    private MNVarStorage     varStorage;
    private string           webServerUrl;
    
    private int              status;
    private long             userId;
    private string           userName;
    private string           userSId;
    private int              lobbyRoomId;
    private int              userStatus;
    private bool             roomExtraInfoReceived;
    private int              defaultGameSetId;
    private MNGameResult     pendingGameResult;
    private bool             fastResumeEnabled;

    private MNGameVocabulary gameVocabulary;

    private string           launchParam;

    private long             launchTime;
    private string           launchId;
    private string           installId;
    private bool             useInstallIdInsteadOfUDID;
    private bool             webShopIsReady;

    private MNTrackingSystem                   trackingSystem;
    private Dictionary<string,string>          appConfigVars;
    private readonly Dictionary<string,string> appExtParams;

    private object                             thisLock = new object();

    public const string CLIENT_API_VERSION  = "1_7_1";

    private const string SMARTFOX_EXT_NAME = "MultiNetExtension";
    private const string SF_EXTCMD_JOIN_BUDDY_ROOM       = "joinBuddyRoom";
    private const string SF_EXTCMD_JOIN_RANDOM_ROOM      = "joinRandomRoom";
    private const string SF_EXTCMD_FINISH_GAME_IN_ROOM   = "finishGameInRoom";
    private const string SF_EXTCMD_FINISH_GAME_PLAIN     = "finishGamePlain";
    private const string SF_EXTCMD_CREATE_BUDDY_ROOM     = "createBuddyRoom";
    private const string SF_EXTCMD_START_BUDDY_ROOM_GAME = "startBuddyRoomGame";
    private const string SF_EXTCMD_STOP_ROOM_GAME        = "stopRoomGame";
    private const string SF_EXTCMD_JOIN_ROOM_INVITATION  = "joinRoomInvitation";
    private const string SF_EXTCMD_CURR_GAME_RESULTS     = "currGameResults";
    private const string SF_EXTCMD_INIT_ROOM_USER_INFO   = "initRoomUserInfo";
    private const string SF_EXTCMD_LEAVE_ROOM            = "leaveRoom";
    private const string SF_EXTCMD_SET_USER_STATUS       = "setUserStatus";
    private const string SF_EXTCMD_SEND_GAME_MESSAGE     = "sendRGM";
    private const string SF_EXTCMD_SEND_PLUGIN_MESSAGE   = "sendRPM";
    private const string SF_EXTCMD_ERROR                 = "MN_error";

    private const string SF_EXTCMD_PARAM_GAMESET_ID = "MN_gameset_id";

    public const int MN_ROOM_ID_UNDEFINED       = -1;
    public const int MN_LOBBY_ROOM_ID_UNDEFINED = MN_ROOM_ID_UNDEFINED;

    public const int MN_CREDENTIALS_WIPE_NONE = 0;
    public const int MN_CREDENTIALS_WIPE_USER = 1;
    public const int MN_CREDENTIALS_WIPE_ALL  = 2;

    private const string APP_COMMAND_SET_APP_PROPERTY_PREFIX = "set";
    private const string APP_PROPERTY_VAR_PATH_PREFIX = "prop.";
    private readonly int APP_COMMAND_SET_APP_PROPERTY_PREFIX_LEN = APP_COMMAND_SET_APP_PROPERTY_PREFIX.Length;

    private const string INSTALL_ID_VAR_NAME = "app.install.id";

    private const string SF_GAME_ROOM_VAR_NAME_GAME_STATUS = "MN_game_status";
    private const string SF_GAME_ROOM_VAR_NAME_GAMESET_ID = "MN_gameset_id";
    private const string SF_GAME_ROOM_VAR_NAME_GAMESET_PARAM = "MN_gameset_param";
    private const string SF_GAME_ROOM_VAR_NAME_GAME_START_COUNTDOWN = "MN_gamestart_countdown";
    private const string SF_GAME_ROOM_VAR_NAME_GAME_SEED = "MN_game_seed";

    private const string SF_EXTCMD_JOIN_BUDDY_ROOM_PARAM_ROOM_SFID = "MN_room_sfid";

    private const string SF_EXTCMD_FINISH_PARAM_SCORE = "MN_out_score";
    private const string SF_EXTCMD_FINISH_PARAM_OUT_TIME = "MN_out_time";
    private const string SF_EXTCMD_FINISH_PARAM_SCORE_POSTLINK_ID = "MN_game_scorepostlink_id";

    private const string SF_EXTCMD_CREATE_BUDDY_ROOM_PARAM_ROOM_NAME = "MN_room_name";
    private const string SF_EXTCMD_CREATE_BUDDY_ROOM_PARAM_TO_USERID_LIST = "MN_to_user_id_list";
    private const string SF_EXTCMD_CREATE_BUDDY_ROOM_PARAM_TO_USERSFID_LIST = "MN_to_user_sfid_list";
    private const string SF_EXTCMD_CREATE_BUDDY_ROOM_PARAM_MSG_TEXT = "MN_mess_text";

    private const string SF_EXTCMD_SET_USER_STATUS_PARAM_USER_STATUS = "MN_user_status";
 
    private const string SF_GAME_USER_VAR_NAME_USER_STATUS = "MN_user_status";

    private const string SF_EXTRSP_CURR_GAME_RESULTS_PARAM_USERID_LIST = "MN_play_user_id_list";
    private const string SF_EXTRSP_CURR_GAME_RESULTS_PARAM_USERSFID_LIST = "MN_play_user_sfid_list";
    private const string SF_EXTRSP_CURR_GAME_RESULTS_PARAM_USER_PLACE_LIST = "MN_play_user_place_list";
    private const string SF_EXTRSP_CURR_GAME_RESULTS_PARAM_USER_SCORE_LIST = "MN_play_user_score_list";
    private const string SF_EXTRSP_CURR_GAME_RESULTS_PARAM_RESULT_IS_FINAL = "MN_play_result_is_final";
    private const string SF_EXTRSP_CURR_GAME_RESULTS_PARAM_GAME_ID = "MN_game_id";
    private const string SF_EXTRSP_CURR_GAME_RESULTS_PARAM_GAMESET_ID = "MN_gameset_id";
    private const string SF_EXTRSP_CURR_GAME_RESULTS_PARAM_PLAYROUND_NUMBER = "MN_play_round_number";

    private const string SF_EXTCMD_JOIN_ROOM_INV_PARAM_FROM_USERSFID = "MN_from_user_sfid";
    private const string SF_EXTCMD_JOIN_ROOM_INV_PARAM_FROM_USERNAME = "MN_from_user_name";
    private const string SF_EXTCMD_JOIN_ROOM_INV_PARAM_ROOM_SFID = "MN_room_sfid";
    private const string SF_EXTCMD_JOIN_ROOM_INV_PARAM_ROOM_NAME = "MN_room_name";
    private const string SF_EXTCMD_JOIN_ROOM_INV_PARAM_ROOM_GAME_ID = "MN_room_game_id";
    private const string SF_EXTCMD_JOIN_ROOM_INV_PARAM_ROOM_GAMESET_ID = "MN_room_gameset_id";
    private const string SF_EXTCMD_JOIN_ROOM_INV_PARAM_MSG_TEXT = "MN_mess_text";

    private const string SF_EXTCMD_ERROR_PARAM_CALL = "MN_call";
    private const string SF_EXTCMD_ERROR_PARAM_ERROR_MSG = "MN_err_msg";

    private const string SF_GAMESET_PLAY_PARAM_VAR_NAME_PREFIX = "MN_gameset_play_param_";
    private readonly int SF_GAMESET_PLAY_PARAM_VAR_NAME_PREFIX_LEN = SF_GAMESET_PLAY_PARAM_VAR_NAME_PREFIX.Length;

    private const string LOGIN_MODEL_LOGIN_PLUS_PASSWORD   = "L";
    private const string LOGIN_MODEL_ID_PLUS_PASSWORD_HASH = "I";
    private const string LOGIN_MODEL_GUEST                 = "G";
    private const string LOGIN_MODEL_AUTH_SIGN             = "A";

    private const string LOGIN_MODEL_GUEST_USER_LOGIN = "*";

    private const string EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX  = "~MNRGM";
    private readonly int EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX_LEN = EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX.Length;
    private const string EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX2 =
                                 "~" + EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX;
    private readonly int EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX2_LEN = EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX2.Length;

    private const string EXTCMD_SEND_GAME_MESSAGE_RAW_PREFIX =
                                 EXTCMD_RECV_GAME_MESSAGE_RAW_PREFIX2 + "~";

    private const char GAME_MESSAGE_ESCAPE_CHAR = '~';

    private const string EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX = "~MNRPM";
    private readonly int EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX_LEN = EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX.Length;
    private const string EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX2 =
                                 "~" + EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX;
    private readonly int EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX2_LEN = EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX2.Length;

    private const string EXTCMD_SEND_PLUGIN_MESSAGE_RAW_PREFIX =
                                 EXTCMD_RECV_PLUGIN_MESSAGE_RAW_PREFIX2 + "~";

    private const char PLUGIN_MESSAGE_PLUGIN_NAME_TERM_CHAR = '^';

    private const string GAME_ZONE_NAME_PREFIX = "Game_";
    private static bool   OFFLINE_MODE_DISABLED = true;
   }
 }
