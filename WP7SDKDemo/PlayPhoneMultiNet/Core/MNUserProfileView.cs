//
//  MNUserProfileView.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Resources;
using System.Windows.Navigation;
using System.Collections.Generic;
using System.IO;

using Microsoft.Phone.Controls;

namespace PlayPhone.MultiNet.Core
 {
  public class MNUserProfileView : MNDashboardPanel
   {
    public delegate void DoGoBackEventHandler ();
    public event DoGoBackEventHandler DoGoBack;

    public MNUserProfileView ()
     {
      Initialize();
     }

    private void Initialize ()
     {
      autoCancelGameOnGoBack = true;
      trustedHosts = new MNTrustedHosts();
      devUsersInfoSrcCache = null;

      loadNavBarExtRequest = false;

      trackedPluginsStorage = new MNStrMaskStorage();

      httpReqQueue = new MNUIWebViewHttpReqQueue();

      httpReqQueue.HttpReqSucceeded += OnHttpReqCompleted;
      httpReqQueue.HttpReqFailed    += OnHttpReqCompleted;

      webView = new WebBrowser();
      webView.IsScriptEnabled = true;
      webView.ScriptNotify += OnWebViewScriptNotify;

      Children.Add(webView);

      navBarViewReady = false;
      navBarView = new WebBrowser();
      navBarView.IsScriptEnabled = true;
      navBarView.ScriptNotify += OnWebViewScriptNotify;

      navBarView.Visibility = Visibility.Collapsed;

      Children.Add(navBarView);
 
      appHostCallHandlers = new Dictionary<string,AppHostCallHander>();

      appHostCallHandlers.Add("apphost_goback.php",OnAppHostCallGoBack);
      appHostCallHandlers.Add("apphost_logout.php",OnAppHostCallLogout);
      appHostCallHandlers.Add("apphost_navbar_hide.php",OnAppHostCallNavBarHide);
      appHostCallHandlers.Add("apphost_navbar_show.php",OnAppHostCallNavBarShow);
      appHostCallHandlers.Add("apphost_get_context.php",OnAppHostCallGetContext);
      appHostCallHandlers.Add("apphost_connect.php",OnAppHostCallConnect);
      appHostCallHandlers.Add("apphost_sn_facebook_login.php",OnAppHostCallFacebookLogin);
      appHostCallHandlers.Add("apphost_sn_facebook_resume.php",OnAppHostCallFacebookResume);
      appHostCallHandlers.Add("apphost_sn_facebook_logout.php",OnAppHostCallFacebookLogout);
      appHostCallHandlers.Add("apphost_sn_facebook_dialog_publish_show.php",OnAppHostCallFacebookDialogPublishShow);
      appHostCallHandlers.Add("apphost_sn_facebook_dialog_permission_req_show.php",OnAppHostCallFacebookDialogPermissionShow);
      appHostCallHandlers.Add("apphost_webview_reload.php",OnAppHostCallWebViewReload);
      appHostCallHandlers.Add("apphost_reconnect.php",OnAppHostCallReconnect);
      appHostCallHandlers.Add("apphost_get_room_userlist.php",OnAppHostCallGetRoomUserList);
      appHostCallHandlers.Add("apphost_get_game_results.php",OnAppHostCallGetGameResults);
      appHostCallHandlers.Add("apphost_chatmess.php",OnAppHostCallChatMess);
      appHostCallHandlers.Add("apphost_sendmess.php",OnAppHostCallSendMess);
      appHostCallHandlers.Add("apphost_joinbuddyroom.php",OnAppHostCallJoinBuddyRoom);
      appHostCallHandlers.Add("apphost_joinautoroom.php",OnAppHostCallJoinAutoRoom);
      appHostCallHandlers.Add("apphost_playgame.php",OnAppHostCallPlayGame);
      appHostCallHandlers.Add("apphost_newbuddyroom.php",OnAppHostCallNewBuddyRoom);
      appHostCallHandlers.Add("apphost_start_room_game.php",OnAppHostCallStartRoomGame);
      appHostCallHandlers.Add("apphost_leaveroom.php",OnAppHostCallLeaveRoom);
      appHostCallHandlers.Add("apphost_set_room_user_status.php",OnAppHostCallSetRoomUserStatus);
      appHostCallHandlers.Add("apphost_script_eval.php",OnAppHostCallScriptEval);
      appHostCallHandlers.Add("apphost_http_request.php",OnAppHostCallHttpRequest);
      appHostCallHandlers.Add("apphost_var_save.php",OnAppHostCallVarSave);
      appHostCallHandlers.Add("apphost_vars_get.php",OnAppHostCallVarsGet);
      appHostCallHandlers.Add("apphost_vars_clear.php",OnAppHostCallVarsClear);
      appHostCallHandlers.Add("apphost_config_vars_get.php",OnAppHostCallConfigVarsGet);
      appHostCallHandlers.Add("apphost_plugin_message_subscribe.php",OnAppHostCallPluginMessageSubscribe);
      appHostCallHandlers.Add("apphost_plugin_message_unsubscribe.php",OnAppHostCallPluginMessageUnSubscribe);
      appHostCallHandlers.Add("apphost_plugin_message_send.php",OnAppHostCallPluginMessageSend);
      appHostCallHandlers.Add("apphost_void.php",OnAppHostCallVoid);
      appHostCallHandlers.Add("apphost_set_host_param.php",OnAppHostCallSetHostParam);
      appHostCallHandlers.Add("apphost_exec_ui_command.php",OnAppHostCallExecUICommand);
      appHostCallHandlers.Add("apphost_navbar_cancel_url_load.php",OnAppHostCallNavBarCancelUrlLoad);
      appHostCallHandlers.Add("apphost_do_photo_import.php",OnAppHostCallDoPhotoImport);
      appHostCallHandlers.Add("apphost_set_game_results.php",OnAppHostCallSetGameResults);
      appHostCallHandlers.Add("apphost_add_source_domain.php",OnAppHostCallAddSourceDomain);
      appHostCallHandlers.Add("apphost_remove_source_domain.php",OnAppHostCallRemoveSourceDomain);
      appHostCallHandlers.Add("apphost_post_web_event.php",OnAppHostCallPostWebEvent);
      appHostCallHandlers.Add("apphost_get_user_ab_data.php",OnAppHostCallGetUserABData);
      appHostCallHandlers.Add("apphost_app_is_installed.php",OnAppHostCallAppIsInstalled);
      appHostCallHandlers.Add("apphost_app_try_launch.php",OnAppHostCallAppTryLaunch);
      appHostCallHandlers.Add("apphost_app_show_in_market.php",OnAppHostCallShowInMarket);

      errorPageLoaded = false;

      LoadBootPage();
     }

    private void WebViewLoadStartUrl ()
     {
      webServerUrl = session.GetWebFrontURL();

      if (webServerUrl != null)
       {
        Uri multiNetWebServerURL = null;

        try
         {
          multiNetWebServerURL = new Uri(webServerUrl);
         }
        catch (UriFormatException)
         {
          MNDebug.debug("MNUserProfileView: invalid web server url");
         }

        if (multiNetWebServerURL != null)
         {
          String baseHost = null;

          try
           {
            baseHost = multiNetWebServerURL.Host;
           }
          catch (InvalidOperationException)
           {
           }

          if (baseHost != null)
           {
            trustedHosts.AddHost(baseHost);
           }

          Uri startUri = new Uri(webServerUrl +
                                 (webServerUrl.StartsWith("file:") ? "/welcome.php.html" : "/welcome.php") +
                                 "?" +
                                 MNUtils.HttpGetRequestBuildParamsString(session.BuildDefaultQueryParamsDict(true)));

          errorPageLoaded = false;

          webView.Navigate(startUri);
         }
       }
     }

    public void BindToSession (MNSession session)
     {
      UnbindFromSession();

      trustedHosts.Clear();
      devUsersInfoSrcCache = null;

      this.session = session;

      session.WebFrontURLReady           += OnSessionWebFrontURLReady;
      session.SessionStatusChanged       += OnSessionStatusChanged;
      session.UserChanged                += OnSessionUserChanged;
      session.RoomUserStatusChanged      += OnSessionRoomUserStatusChanged;
      session.DefaultGameSetIdChangedTo  += OnSessionDefaultGameSetIdChangedTo;
      session.ChatPrivateMessageReceived += OnSessionChatPrivateMessageReceived;
      session.ChatPublicMessageReceived  += OnSessionChatPublicMessageReceived;
      session.PluginMessageReceived      += OnSessionPluginMessageReceived;
      session.JoinRoomInvitationReceived += OnSessionJoinRoomInvitationReceived;
      session.GameStartCountdownTick     += OnSessionGameStartCountdownTick;
      session.ErrorOccurred              += OnSessionErrorOccurred;
      session.GameFinishedWithResult     += OnSessionGameFinishedWithResult;
      session.RoomUserJoin               += OnSessionRoomUserJoin;
      session.RoomUserLeave              += OnSessionRoomUserLeave;
      session.DevUsersInfoChanged        += OnSessionDevUsersInfoChanged;
      session.CurrGameResultsReceived    += OnSessionCurrGameResultsReceived;
      session.ExecAppCommandReceived     += OnSessionExecAppCommandReceived;
      session.WebEventReceived           += OnSessionWebEventReceived;
      session.SysEventReceived           += OnSessionSysEventReceived;

      WebViewLoadStartUrl();
     }

    public void UnbindFromSession ()
     {
      if (session != null)
       {
        session.WebFrontURLReady           -= OnSessionWebFrontURLReady;
        session.SessionStatusChanged       -= OnSessionStatusChanged;
        session.UserChanged                -= OnSessionUserChanged;
        session.RoomUserStatusChanged      -= OnSessionRoomUserStatusChanged;
        session.DefaultGameSetIdChangedTo  -= OnSessionDefaultGameSetIdChangedTo;
        session.ChatPrivateMessageReceived -= OnSessionChatPrivateMessageReceived;
        session.ChatPublicMessageReceived  -= OnSessionChatPublicMessageReceived;
        session.PluginMessageReceived      -= OnSessionPluginMessageReceived;
        session.JoinRoomInvitationReceived -= OnSessionJoinRoomInvitationReceived;
        session.GameStartCountdownTick     -= OnSessionGameStartCountdownTick;
        session.ErrorOccurred              -= OnSessionErrorOccurred;
        session.GameFinishedWithResult     -= OnSessionGameFinishedWithResult;
        session.RoomUserJoin               -= OnSessionRoomUserJoin;
        session.RoomUserLeave              -= OnSessionRoomUserLeave;
        session.DevUsersInfoChanged        -= OnSessionDevUsersInfoChanged;
        session.ExecAppCommandReceived     -= OnSessionExecAppCommandReceived;
        session.WebEventReceived           -= OnSessionWebEventReceived;
        session.SysEventReceived           -= OnSessionSysEventReceived;

        session = null;
       }
     }

    public void Destroy ()
     {
      httpReqQueue.Shutdown();

      UnbindFromSession();
     }

    public void SetAutoCancelGameOnGoBack (bool enable)
     {
      autoCancelGameOnGoBack = enable;
     }

    public bool GetAutoCancelGameOnGoBack ()
     {
      return autoCancelGameOnGoBack;
     }

    public void SetContextCallWaitLoad (bool enable)
     {
      contextCallWaitLoad = enable;
     }

    public bool GetContextCallWaitLoad ()
     {
      return contextCallWaitLoad;
     }

    private bool IsWebViewLocationAtLocalFile (WebBrowser browser)
     {
      Uri source = browser.Source;

      return source != null && source.Scheme == URI_SCHEME_FILE;
     }

    private bool IsHostTrusted (string hostName)
     {
      return trustedHosts.IsTrusted(hostName);
     }

    private bool IsWebViewLocationAtTrustedHost (WebBrowser browser)
     {
      Uri source = browser.Source;

      if (source != null)
       {
        string scheme = source.Scheme;

        if (scheme != URI_SCHEME_HTTP && scheme != URI_SCHEME_HTTPS)
         {
          return false;
         }

        return IsHostTrusted(browser.Source.Host);
       }
      else
       {
        return false;
       }
     }

    private bool IsWebViewLocationTrusted (WebBrowser browser)
     {
      return IsWebViewLocationAtLocalFile(browser) ||
             IsWebViewLocationAtTrustedHost(browser);
     }

    private void EvalJS (WebBrowser webBrowser, string jsCode)
     {
      try
       {
        webBrowser.InvokeScript("eval",jsCode);
       }
      catch (SystemException)
       {
       }
     }

    private void CallJS (string javaScriptCode)
     {
      CallJS(javaScriptCode,false);
     }

    private void CallJS (string javaScriptCode, bool forceFlag)
     {
      CallJS(javaScriptCode,forceFlag,true,true);
     }

    private void CallJS (string javaScriptCode, bool forceFlag, bool callWebView, bool callNavBar)
     {
      if (callWebView && (forceFlag || IsWebViewLocationTrusted(webView)))
       {
        EvalJS(webView,javaScriptCode);
       }

      if (callNavBar && (forceFlag || IsWebViewLocationTrusted(navBarView)))
       {
        if (navBarViewReady)
         {
          EvalJS(navBarView,javaScriptCode);
         }
       }
     }

    #region MNUIWebViewHttpReqQueue event handlers
    private void OnHttpReqCompleted (string jsCode, int flags)
     {
      CallJS(jsCode,
             false,
             (flags & HTTPREQ_FLAG_EVAL_IN_MAINWEBVIEW_MASK) != 0,
             (flags & HTTPREQ_FLAG_EVAL_IN_NAVBARWEBVIEW_MASK) != 0);
     }
    #endregion

    #region MNSession event handlers
    private void OnSessionStatusChanged (int newStatus, int oldStatus)
     {
      if (oldStatus == MNConst.MN_OFFLINE && errorPageLoaded)
       {
        WebViewLoadStartUrl();
       }
      else
       {
        ScheduleUpdateContext();
       }
     }

    private void OnSessionUserChanged (long userId)
     {
      ScheduleUpdateContext();
     }

    private void OnSessionRoomUserStatusChanged (int userStatus)
     {
      string newStatus = userStatus == MNConst.MN_USER_STATUS_UNDEFINED ? null : userStatus.ToString();

      CallJS("MN_UpdateRoomUserStatus(" + MNUtils.StringAsJSString(newStatus) + ")");
     }

    private void OnSessionDefaultGameSetIdChangedTo (int gameSetId)
     {
      CallJS("UpdateDefaultGameSetId(" + gameSetId.ToString() + ")");
     }

    private void ScheduleChatMessageNotification (string jsFuncName, MNChatMessage chatMessage)
     {
      string userInfo = string.Format("new MN_SF_UserInfo({0},{1},'{2}')",
                                      chatMessage.Sender.UserSFId,
                                      MNUtils.StringAsJSString(chatMessage.Sender.UserName),
                                      chatMessage.Sender.UserId);

      string jsCode = String.Format("{0}({1},{2},{3})",
                                    jsFuncName,
                                    chatMessage.Sender.UserSFId,
                                    userInfo,
                                    MNUtils.StringAsJSString(chatMessage.Message));

      CallJS(jsCode);
     }

    private void OnSessionChatPublicMessageReceived (MNChatMessage chatMessage)
     {
      ScheduleChatMessageNotification("MN_InChatPublicMessage",chatMessage);
     }

    private void OnSessionChatPrivateMessageReceived (MNChatMessage chatMessage)
     {
      ScheduleChatMessageNotification("MN_InChatPrivateMessage",chatMessage);
     }

    public void OnSessionPluginMessageReceived (string     pluginName,
                                                  string     message,
                                                  MNUserInfo sender)
     {
      if (trackedPluginsStorage.CheckString(pluginName))
       {
        string userInfo;

        if (sender != null)
         {
          userInfo = String.Format("new MN_SF_UserInfo({0},{1},'{2}')",
                                   sender.UserSFId,
                                   MNUtils.StringAsJSString(sender.UserName),
                                   sender.UserId);
         }
        else
         {
          userInfo = "null";
         }

        string jsCode = String.Format("MN_RecvPluginMessage({0},{1},{2})",
                                      MNUtils.StringAsJSString(pluginName),
                                      MNUtils.StringAsJSString(message),
                                      userInfo);

        CallJS(jsCode);
       }
     }

    public void OnSessionJoinRoomInvitationReceived (MNJoinRoomInvitationParams invParams)
     {
      long   userId;
      string userName;

      if (!MNUtils.ParseMNUserName(invParams.FromUserName, out userName, out userId))
       {
        userId   = MNConst.MN_USER_ID_UNDEFINED;
        userName = invParams.FromUserName;
       }

      string javaScriptSrc = String.Format("MN_InJoinRoomInvitation({0}," +
                                           "new MN_SF_UserInfo({1},{2},'{3}')," +
                                           "new MN_SF_InviteRoom({4},{5},{6},{7}),{8})",
                                           invParams.FromUserSFId,
                                           invParams.FromUserSFId,
                                           MNUtils.StringAsJSString(userName),
                                           userId,
                                           invParams.RoomSFId,
                                           MNUtils.StringAsJSString(invParams.RoomName),
                                           invParams.RoomGameId,
                                           invParams.RoomGameSetId,
                                           MNUtils.StringAsJSString(invParams.InviteText));

      CallJS(javaScriptSrc);
     }

    public void OnSessionGameStartCountdownTick (int secondsLeft)
     {
      CallJS("MN_InGameStartCountdown(" + secondsLeft.ToString() + ")");
     }

    public void OnSessionCurrGameResultsReceived (MNCurrGameResults gameResults)
     {
      StringBuilder jsCode =
       new StringBuilder("MN_InCurrGameResults(new Array(");

      MNStringJoiner infoStr = new MNStringJoiner(",");

      for (int index = 0; index < gameResults.Users.Length; index++)
       {
        MNUserInfo userInfo = gameResults.Users[index];

        infoStr.Join
         (String.Format
           ("new MN_UserGameResult({0},new MN_SF_UserInfo({1},{2},'{3}'),{4},{5})",
            userInfo.UserSFId,
            userInfo.UserSFId,
            MNUtils.StringAsJSString(userInfo.UserName),
            userInfo.UserId,
            gameResults.UserPlaces[index],
            gameResults.UserScores[index]));
       }

      jsCode.Append(infoStr.ToString());
      jsCode.Append("),");
      jsCode.Append(gameResults.FinalResult ? "1" : "0");
      jsCode.Append(')');

      CallJS(jsCode.ToString());
     }

    private void OnSessionErrorOccurred (MNErrorInfo errorInfo)
     {
      if (errorInfo.ActionCode == MNErrorInfo.ACTION_CODE_LOAD_CONFIG &&
          webServerUrl == null)
       {
        LoadErrorMessagePage(MNI18n.GetLocalizedString
                              ("Internet connection is not available",
                               MNI18n.MESSAGE_CODE_INTERNET_CONNECTION_NOT_AVAILABLE_ERROR));
       }
      else
       {
        CallSetErrorMessage(errorInfo.ErrorMessage,errorInfo.ActionCode);
       }
     }

    public void OnSessionGameFinishedWithResult (MNGameResult gameResult)
     {
      CallJS(String.Format("MN_InFinishGameNotify({0},{1},{2})",
                           gameResult.Score,
                           MNUtils.StringAsJSString(gameResult.ScorePostLinkId),
                           gameResult.GameSetId));
     }

    public void OnSessionRoomUserJoin (MNUserInfo userInfo)
     {
      CallJS
       (String.Format("MN_InRoomUserJoin({0},new MN_SF_UserInfo({1},{2},'{3}'))",
                      userInfo.UserSFId,
                      userInfo.UserSFId,
                      MNUtils.StringAsJSString(userInfo.UserName),
                      userInfo.UserId));
     }

    public void OnSessionRoomUserLeave (MNUserInfo userInfo)
     {
      CallJS
       (String.Format("MN_InRoomUserLeave({0},new MN_SF_UserInfo({1},{2},'{3}'))",
                      userInfo.UserSFId,
                      userInfo.UserSFId,
                      MNUtils.StringAsJSString(userInfo.UserName),
                      userInfo.UserId));
     }

    private void OnSessionDevUsersInfoChanged ()
     {
      devUsersInfoSrcCache = null;
     }

    private void OnSessionWebFrontURLReady (string webFrontUrl)
     {
      if (session != null && webServerUrl == null)
       {
        WebViewLoadStartUrl();
       }
     }

    public void OnSessionExecAppCommandReceived (string cmdName, string cmdParam)
     {
      StringBuilder jsCode = new StringBuilder("MN_RecvAppCommand(");

      jsCode.Append(MNUtils.StringAsJSString(cmdName));
      jsCode.Append(',');
      jsCode.Append(MNUtils.StringAsJSString(cmdParam));
      jsCode.Append(')');

      CallJS(jsCode.ToString());
     }

    public void OnSessionWebEventReceived (string eventName, string eventParam, string callbackId)
     {
      if (eventName == "web.getUserAddressBook")
       {
        NotImpl("web.getUserAddressBook web event");
       }
     }

    public void OnSessionSysEventReceived (string eventName, string eventParam, string callbackId)
     {
      StringBuilder jsCode = new StringBuilder("MN_HandleSysEvent({ 'eventName' : ");

      jsCode.Append(MNUtils.StringAsJSString(eventName));
      jsCode.Append(", 'eventParam' : ");
      jsCode.Append(MNUtils.StringAsJSString(eventParam));
      jsCode.Append(", 'callbackId' : ");
      jsCode.Append(MNUtils.StringAsJSString(callbackId));
      jsCode.Append("})");

      CallJS(jsCode.ToString());
     }
    #endregion

    private string getDeviceUsersInfoJSSrc ()
     {
      if (devUsersInfoSrcCache == null)
       {
        StringBuilder devUsersInfoSrc = new StringBuilder();
        MNUserCredentials[] devUsersInfoArray = MNUserCredentials.LoadAllCredentials(session.GetVarStorage());

        for (int index = 0; index < devUsersInfoArray.Length; index++)
         {
          MNUserCredentials devUserInfo = devUsersInfoArray[index];

          if (index > 0)
           {
            devUsersInfoSrc.Append(',');
           }

          devUsersInfoSrc.Append("new MN_DevUserInfo('");
          devUsersInfoSrc.Append(devUserInfo.userId.ToString());
          devUsersInfoSrc.Append("',");
          devUsersInfoSrc.Append(MNUtils.StringAsJSString(devUserInfo.userName));
          devUsersInfoSrc.Append(',');
          devUsersInfoSrc.Append(MNUtils.StringAsJSString(devUserInfo.userAuthSign));
          devUsersInfoSrc.Append(",'");
          devUsersInfoSrc.Append(MNUtils.GetUnitTime(devUserInfo.lastLoginTime.Value).ToString());
          devUsersInfoSrc.Append("',");
          devUsersInfoSrc.Append(MNUtils.StringAsJSString(devUserInfo.userAuxInfoText));
          devUsersInfoSrc.Append(')');
         }

        devUsersInfoSrcCache = devUsersInfoSrc.ToString();
       }

      return devUsersInfoSrcCache;
     }

    private void CallUpdateContext (bool setMode)
     {
      int  status = session.GetStatus();
      long userId = session.GetMyUserId();

      string roomId = session.IsInGameRoom() ? session.GetCurrentRoomId().ToString() : "null";

      int roomUserStatus = session.GetRoomUserStatus();

      MNSocNetSessionFB fbSession = session.GetSocNetSessionFB();
      string            fbContext;

      if (fbSession.IsAuthorized())
       {
        fbContext = "new MN_SNContextFacebook('-1',''," +
                     MNUtils.StringAsJSString(fbSession.AccessToken) +
                    ",1)"; // user id is unknown, no secret, credentials are stored
       }
      else
       {
        fbContext = "null";
       }

      StringBuilder javaScriptSrc = new StringBuilder();

      javaScriptSrc.Append(setMode ? "MN_SetContext" : "MN_UpdateContext");
      javaScriptSrc.Append("(new MN_Context(");
      javaScriptSrc.Append(                 status.ToString()); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                 session.GetGameId().ToString()); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                 userId == MNConst.MN_USER_ID_UNDEFINED ? "null" : userId.ToString()); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                 MNUtils.StringAsJSString(session.GetMyUserName())); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                 MNUtils.StringAsJSString(session.GetMySId())); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                 MNPlatformWinPhone.GetDeviceType().ToString()); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                 MNUtils.StringAsJSString(MNUtils.StringGetMD5String(MNPlatformWinPhone.GetUniqueDeviceIdentifier()))); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                 "new Array("); javaScriptSrc.Append(getDeviceUsersInfoJSSrc()); javaScriptSrc.Append("),");

      //javaScriptSrc.Append(                 "new Array(new MN_SNSessionInfo(1,0,null)),");
      javaScriptSrc.Append(                 "new Array(new MN_SNSessionInfo(1,");
      javaScriptSrc.Append(                 fbSession.IsAuthorized() ? "10," : "0,");
      javaScriptSrc.Append(                 fbContext); javaScriptSrc.Append(")),");

      javaScriptSrc.Append(                 session.GetDefaultGameSetId().ToString()); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                 MNUtils.StringAsJSString(session.GetApplicationStartParam())); javaScriptSrc.Append("),");
      javaScriptSrc.Append(                 "new MN_RoomContext(");
      javaScriptSrc.Append(                                      roomId); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                                      session.GetRoomGameSetId().ToString()); javaScriptSrc.Append(',');
      javaScriptSrc.Append(                                      roomUserStatus == MNConst.MN_USER_STATUS_UNDEFINED ? "null" : roomUserStatus.ToString());
      javaScriptSrc.Append(                                      ")");

      if (setMode)
       {
        javaScriptSrc.Append(")");
       }
      else
       {
        javaScriptSrc.Append(",null,null)");
       }

      if (IsWebViewLocationTrusted(webView))
       {
        EvalJS(webView,javaScriptSrc.ToString());
       }

      if (!setMode && navBarViewReady)
       {
        if (IsWebViewLocationTrusted(navBarView))
         {
          EvalJS(navBarView,javaScriptSrc.ToString());
         }
       }
     }

    private void callSetErrorMessage (String message)
     {
      CallSetErrorMessage(message,MNErrorInfo.ACTION_CODE_UNDEFINED);
     }

    private void CallSetErrorMessage (string message, int actionCode)
     {
      CallJS("MN_SetErrorMessage(" +
              MNUtils.StringAsJSString(message) +
              ",new MN_ErrorContext(" +
              actionCode.ToString() +
              "))");
     }

    private void ScheduleUpdateContext ()
     {
      CallUpdateContext(false);
     }

    private void OnWebViewScriptNotify (object Sender, NotifyEventArgs args)
     {
      Uri requestUrl = null;

      if (Sender == navBarView)
       {
        navBarViewReady = true;
       }

      if (session == null || webServerUrl == null)
       {
        return;
       }

      try
       {
        requestUrl = new Uri(new Uri(webServerUrl),args.Value);
       }
      catch (UriFormatException)
       {
        return;
       }

      string path = requestUrl.GetComponents(UriComponents.Path,UriFormat.Unescaped);

      if (path.StartsWith("apphost_"))
       {
        if (!IsWebViewLocationTrusted((WebBrowser)Sender))
         {
          return;
         }

        string query                          = requestUrl.Query;
        Dictionary<string,string> queryParams = null;

        if (query.StartsWith("?"))
         {
          queryParams = MNUtils.HttpGetRequestParseParams(query.Substring(1));
         }
        else
         {
          //FIXME: use single static readonly instance of empty dict instead of re-creating it each time
          queryParams = new Dictionary<string,string>();
         }

        string reqInOutArg = null;

        if (queryParams.TryGetValue("apphost_req_in_arg",out reqInOutArg))
         {
          CallAppRequestGuard((WebBrowser)Sender,reqInOutArg,false);
         }

        if (!session.PreprocessAppHostCall(new MNAppHostCallInfo(path,queryParams)))
         {
          AppHostCallHander handler;

          if (appHostCallHandlers.TryGetValue(path,out handler))
           {
            handler(queryParams);
           }
          else
           {
            MNDebug.debug("unsupported appHost call " + path + " ignored");
           }
         }

        if (queryParams.TryGetValue("apphost_req_out_arg",out reqInOutArg))
         {
          CallAppRequestGuard((WebBrowser)Sender,reqInOutArg,true);
         }
       }
     }

    private void CallAppRequestGuard (WebBrowser webView, string param, bool isPostGuard)
     {
      EvalJS(webView,(isPostGuard ? "MN_AppHostReqOut(" : "MN_AppHostReqIn(") +
                     MNUtils.StringAsJSString(param) +
                     ")");
     }

    #region AppHost call handlers
    private void OnAppHostCallGoBack (Dictionary<string,string> queryParams)
     {
      if (autoCancelGameOnGoBack && session != null && session.IsOnline())
       {
        session.CancelGameWithParams(null);
       }

      DoGoBackEventHandler handler = DoGoBack;

      if (handler != null)
       {
        handler();
       }
     }

    private void OnAppHostCallNavBarHide (Dictionary<string,string> queryParams)
     {
      if (navBarView.Visibility == Visibility.Visible)
       {
        navBarView.Visibility = Visibility.Collapsed;
       }
     }

    private void OnAppHostCallNavBarShow (Dictionary<string,string> queryParams)
     {
      string navBarUrl = GetQueryParamSafe(queryParams,"navbar_url");

      if (navBarUrl != null)
       {
        int? newHeightParam = MNUtils.ParseInt(GetQueryParamSafe(queryParams,"navbar_height"));

        if (newHeightParam == null || newHeightParam < 0)
         {
          newHeightParam = NAV_BAR_DEFAULT_HEIGHT;
         }

        try
         {
          navBarView.Navigate(new Uri(navBarUrl));
         }
        catch (UriFormatException)
         {
          MNDebug.debug("navbar_url is malformed in navbar_show apphost call");
         }

        newHeightParam = (int)(newHeightParam.Value * NAV_BAR_SCALE_FACTOR + 0.5);

        navBarView.Visibility = Visibility.Visible;
        navBarView.Height     = (int)newHeightParam;
       }
      else
       {
        MNDebug.debug("navbar_url is not set in navbar_show apphost call");
       }
     }

    private void OnAppHostCallConnect (Dictionary<string,string> queryParams)
     {
      string mode = GetQueryParamSafe(queryParams,"mode");

      if (mode != null)
       {
        if (mode == "login_multinet_by_user_id_and_phash")
         {
          long?  userId        = MNUtils.ParseLong(GetQueryParamSafe(queryParams,"user_id"));
          string passwordHash  = GetQueryParamSafe(queryParams,"user_password_hash");
          string devSetHomeStr = GetQueryParamSafe(queryParams,"user_dev_set_home");

          if (userId != null && passwordHash != null)
           {
            bool devSetHome;

            devSetHome = devSetHomeStr != null && devSetHomeStr == "1";

            session.LoginWithUserIdAndPasswordHash((long)userId,passwordHash,devSetHome);
           }
          else
           {
            MNDebug.debug("invalid parameters in login apphost call (user_id_and_phash)");
           }
         }
        else if (mode == "login_multinet_user_id_and_auth_sign")
         {
          long?  userId   = MNUtils.ParseLong(GetQueryParamSafe(queryParams,"user_id"));
          string authSign = GetQueryParamSafe(queryParams,"user_auth_sign");

          if (userId != null && authSign != null)
           {
            session.LoginWithUserIdAndAuthSign((long)userId,authSign);
           }
          else
           {
            MNDebug.debug("invalid parameters in login apphost call (user_id_and_auth_sign)");
           }
         }
        else if (mode == "login_multinet_user_id_and_auth_sign_offline")
         {
          long?  userId        = MNUtils.ParseLong(GetQueryParamSafe(queryParams,"user_id"));
          string authSign      = GetQueryParamSafe(queryParams,"user_auth_sign");

          if (userId != null && authSign != null)
           {
            session.LoginOfflineWithUserIdAndAuthSign((long)userId,authSign);
           }
          else
           {
            MNDebug.debug("invalid parameters in login apphost call (user_id_and_auth_sign offline)");
           }
         }
        else if (mode == "login_multinet_auto")
         {
          session.LoginAuto();
         }
        else if (mode == "login_multinet")
         {
          string userLogin     = GetQueryParamSafe(queryParams,"user_login");
          string userPassword  = GetQueryParamSafe(queryParams,"user_password");
          string devSetHomeStr = GetQueryParamSafe(queryParams,"user_dev_set_home");

          if (userLogin != null && userPassword != null)
           {
            bool devSetHome;

            devSetHome = devSetHomeStr != null && devSetHomeStr == "1";

            session.LoginWithUserLoginAndPassword(userLogin,userPassword,devSetHome);
           }
          else
           {
            MNDebug.debug("invalid parameters in login apphost call (login_multinet)");
           }
         }
        else if (mode == "login_multinet_signup_offline")
         {
          session.SignupOffline();
         }
        else
         {
          MNDebug.debug("invalid mode " + mode + " in login apphost call");
         }
       }
      else
       {
        MNDebug.debug("undefined mode in login apphost call");
       }
     }

    private void OnAppHostCallLogout (Dictionary<string,string> queryParams)
     {
      int? wipeHome = MNUtils.ParseInt(GetQueryParamSafe(queryParams,"user_dev_wipe_home"));

      if (wipeHome == null)
       {
        wipeHome = MNSession.MN_CREDENTIALS_WIPE_NONE;
       }

      session.LogoutAndWipeUserCredentialsByMode(wipeHome.Value);

      devUsersInfoSrcCache = null;
     }

    private void OnAppHostCallWebViewReload (Dictionary<string,string> queryParams)
     {
      Uri destUri      = null;
      string webViewUrl = GetQueryParamSafe(queryParams,"webview_url");

      if (webViewUrl != null)
       {
        try
         {
          destUri = new Uri(webViewUrl);
         }
        catch (UriFormatException)
         {
         }
       }

      if (destUri != null)
       {
        webView.Navigate(destUri);
       }
      else
       {
        MNDebug.warning("webview_url is invalid or not set in webview_reload request");
       }
     }

    private void OnAppHostCallReconnect (Dictionary<string,string> queryParams)
     {
      if (session != null)
       {
        if (session.GetStatus() == MNConst.MN_OFFLINE &&
            session.IsReLoginPossible())
         {
          session.ReLogin();
         }
        else
         {
          WebViewLoadStartUrl();
         }
       }
     }

    private void OnAppHostCallGetRoomUserList (Dictionary<string,string> queryParams)
     {
      if (session == null || !session.IsOnline())
       {
        return;
       }

      StringBuilder  jsCode = new StringBuilder("MN_InRoomUserList(new Array(");
      MNStringJoiner userListStr = new MNStringJoiner(",");
      MNUserInfo[] userList = session.GetRoomUserList();

      foreach (MNUserInfo userInfo in userList)
       {
        userListStr.Join(String.Format("new MN_SF_UserInfo({0},{1},'{2}')",
                                       userInfo.UserSFId,
                                       MNUtils.StringAsJSString
                                        (userInfo.UserName),
                                       userInfo.UserId));
       }

      jsCode.Append(userListStr.ToString());
      jsCode.Append("));");

      CallJS(jsCode.ToString());
     }

    private void OnAppHostCallGetGameResults (Dictionary<string,string> queryParams)
     {
      if (session != null)
       {
        session.ReqCurrentGameResults();
       }
     }

    private void OnAppHostCallChatMess (Dictionary<string,string> queryParams)
     {
      if (session == null || !session.IsOnline())
       {
        return;
       }

      string message =  GetQueryParamSafe(queryParams,"mess_text");

      if (message != null)
       {
        session.SendChatMessage(message);
       }
      else
       {
        MNDebug.warning("message is not set in chatmess request");
       }
     }

    private void OnAppHostCallSendMess (Dictionary<string,string> queryParams)
     {
      if (session == null || !session.IsOnline())
       {
        return;
       }

      string message    = GetQueryParamSafe(queryParams,"mess_text");
      int?   toUserSFId = MNUtils.ParseInt(GetQueryParamSafe(queryParams,"to_user_sfid"));

      if (message == null)
       {
        MNDebug.warning("message is not set in sendmess request");
       }
      else if (toUserSFId == null)
       {
        MNDebug.warning("to_user_sfid is not set or invalid in sendmess request");
       }
      else
       {
        session.SendPrivateMessage(message,toUserSFId.Value);
       }
     }

    private void OnAppHostCallNewBuddyRoom (Dictionary<string,string> queryParams)
     {
      if (session == null)
       {
        return;
       }

      string roomName       = GetQueryParamSafe(queryParams,"room_name");
      int?   gameSetId      = MNUtils.ParseInt(GetQueryParamSafe(queryParams,"gameset_id"));
      string toUserIdList   = GetQueryParamSafe(queryParams,"to_user_id_list");
      string toUserSFIdList = GetQueryParamSafe(queryParams,"to_user_sfid_list");
      string inviteText     = GetQueryParamSafe(queryParams,"mess_text");

      if (roomName != null && gameSetId != null && toUserIdList != null &&
           toUserSFIdList != null && inviteText != null)
       {
        MNBuddyRoomParams buddyRoomParams =
         new MNBuddyRoomParams(roomName,gameSetId.Value,toUserIdList,toUserSFIdList,inviteText);

        session.ReqCreateBuddyRoom(buddyRoomParams);
       }
      else
       {
        MNDebug.warning("not enough parameters in newbuddyroom request");
       }
     }

    private void OnAppHostCallStartRoomGame (Dictionary<string,string> queryParams)
     {
      if (session == null || session.GetStatus() != MNConst.MN_IN_GAME_WAIT)
       {
        callSetErrorMessage(MNI18n.GetLocalizedString("Room not ready",MNI18n.MESSAGE_CODE_ROOM_IS_NOT_READY_TO_START_A_GAME_ERROR));
       }
      else
       {
        session.ReqStartBuddyRoomGame();
       }
     }

    private void OnAppHostCallJoinBuddyRoom (Dictionary<string,string> queryParams)
     {
      if (session == null || !session.IsOnline())
       {
        return;
       }

      int? roomSFId = MNUtils.ParseInt(GetQueryParamSafe(queryParams,"room_sfid"));

      if (roomSFId != null)
       {
        session.ReqJoinBuddyRoom(roomSFId.Value);
       }
      else
       {
        MNDebug.warning("room_sfid is not set or invalid in join buddy room request");
       }
     }

    private void OnAppHostCallJoinAutoRoom (Dictionary<string,string> queryParams)
     {
      if (session == null || !session.IsOnline())
       {
        callSetErrorMessage(MNI18n.GetLocalizedString("you must be in lobby room to join",MNI18n.MESSAGE_CODE_MUST_BE_IN_LOBBY_ROOM_TO_JOIN_RANDOM_ROOM_ERROR));

        return;
       }

      string gameSetId = GetQueryParamSafe(queryParams,"gameset_id");

      if (gameSetId != null)
       {
        session.ReqJoinRandomRoom(gameSetId);
       }
      else
       {
        MNDebug.warning("gameset_id is not set in join auto-room request");
       }
     }

    private void OnAppHostCallPlayGame (Dictionary<string,string> queryParams)
     {
      if (session == null)
       {
        return;
       }

      int?   gameSetId       = MNUtils.ParseInt(GetQueryParamSafe(queryParams,"gameset_id"));
      string gameSetParams   = GetQueryParamSafe(queryParams,"gameset_params");
      string scorePostLinkId = GetQueryParamSafe(queryParams,"game_scorepostlink_id");
      int?   gameSeed        = MNUtils.ParseInt(GetQueryParamSafe(queryParams,"game_seed"));

      if (gameSetId != null && gameSetParams != null &&
          scorePostLinkId != null && gameSeed != null)
       {
        MNGameParams gameParams =
         new MNGameParams(gameSetId.Value,gameSetParams,
                          scorePostLinkId,
                          gameSeed.Value,
                          scorePostLinkId.Length > 0 ?
                           MNGameParams.MN_PLAYMODEL_SINGLEPLAY_NET :
                           MNGameParams.MN_PLAYMODEL_SINGLEPLAY);

        foreach (var param in queryParams)
         {
          if (param.Key.StartsWith(GAMESET_PLAY_PARAM_PREFIX))
           {
            gameParams.AddGameSetPlayParam
             (param.Key.Substring(GAMESET_PLAY_PARAM_PREFIX_LEN),param.Value);
           }
         }

        session.StartGameWithParams(gameParams);
       }
      else
       {
        MNDebug.warning("one of the 'play game' request parameters is not set or invalid");
       }
     }

    private void OnAppHostCallLeaveRoom (Dictionary<string,string> queryParams)
     {
      if (session == null)
       {
        return;
       }

      session.LeaveRoom();
     }

    private void OnAppHostCallSetRoomUserStatus (Dictionary<string,string> queryParams)
     {
      if (session == null || !session.IsOnline())
       {
        return;
       }

      int? userStatus = MNUtils.ParseInt(GetQueryParamSafe(queryParams,"mn_user_status"));

      if (userStatus != null)
       {
        session.ReqSetUserStatus(userStatus.Value);
       }
      else
       {
        MNDebug.warning("mn_user_status is not set in 'set user status' request");
       }
     }

    private void OnAppHostCallPluginMessageSubscribe (Dictionary<string,string> queryParams)
     {
      string mask = GetQueryParamSafe(queryParams,"plugin_mask");

      if (mask != null)
       {
        trackedPluginsStorage.AddMask(mask);
       }
     }

    private void OnAppHostCallPluginMessageUnSubscribe (Dictionary<string,string> queryParams)
     {
      string mask = GetQueryParamSafe(queryParams,"plugin_mask");

      if (mask != null)
       {
        trackedPluginsStorage.RemoveMask(mask);
       }
     }

    private void OnAppHostCallPluginMessageSend (Dictionary<string,string> queryParams)
     {
      if (session == null || !session.IsOnline())
       {
        return;
       }

      string pluginName    = GetQueryParamSafe(queryParams,"plugin_name");
      string pluginMessage = GetQueryParamSafe(queryParams,"plugin_message");

      if (pluginName != null && pluginMessage != null)
       {
        session.SendPluginMessage(pluginName,pluginMessage);
       }
     }

    private void OnAppHostCallSetGameResults (Dictionary<string,string> queryParams)
     {
      if (session == null)
       {
        return;
       }

      long?  score = MNUtils.ParseLong(GetQueryParamSafe(queryParams,"score"));
      string scorePostLinkId = GetQueryParamSafe(queryParams,"score_post_link_id");
      int?   gameSetId       = MNUtils.ParseInt(GetQueryParamSafe(queryParams,"gameset_id"));

      if (score != null && gameSetId != null)
       {
        MNGameResult gameResult = new MNGameResult(null);

        gameResult.Score           = score.Value;
        gameResult.ScorePostLinkId = scorePostLinkId;
        gameResult.GameSetId       = gameSetId.Value;

        session.FinishGameWithResult(gameResult);
       }
      else
       {
        MNDebug.warning("score or game_set_id parameter is invalid or not set in 'set_game_results' request");
       }
     }

    private void OnAppHostCallAddSourceDomain (Dictionary<string,string> queryParams)
     {
      string domainName = GetQueryParamSafe(queryParams,"domain_name");

      if (domainName != null)
       {
        trustedHosts.AddHost(domainName);
       }
      else
       {
        MNDebug.warning("domain name parameter is not set in 'add_source_domain' request");
       }
     }

    private void OnAppHostCallRemoveSourceDomain (Dictionary<string,string> queryParams)
     {
      string domainName = GetQueryParamSafe(queryParams,"domain_name");

      if (domainName != null)
       {
        trustedHosts.RemoveHost(domainName);
       }
      else
       {
        MNDebug.warning("domain name parameter is not set in 'remove_source_domain' request");
       }
     }

    private void OnAppHostCallAppIsInstalled (Dictionary<string,string> queryParams)
     {
      string appPackageName = GetQueryParamSafe(queryParams,"app_install_bundle_id");

      if (appPackageName != null)
       {
        NotImpl("app_is_installed query");

        bool isInstalled = false;

        CallJS("MN_AppCheckInstalledCallback(" +
               MNUtils.StringAsJSString(appPackageName) +
               "," + (isInstalled ? "true" : "false") + ")");
       }
      else
       {
        MNDebug.warning("app_install_bundle_id parameter is not set in 'app_is_installed' request");
       }
     }

    private void OnAppHostCallAppTryLaunch (Dictionary<string,string> queryParams)
     {
      string appPackageName = GetQueryParamSafe(queryParams,"app_launch_bundle_id");
      string launchParam    = GetQueryParamSafe(queryParams,"app_launch_param");

      if (appPackageName != null && launchParam != null)
       {
        NotImpl("app launch request");

        bool ok = false;

        CallJS("MN_AppLaunchCallback(" +
               MNUtils.StringAsJSString(appPackageName) +
               "," + (ok ?  "true" : "false") + ")");
       }
      else
       {
        MNDebug.debug("app_launch_bundle_id or app_launch_param parameter is not set in 'app_try_launch' request");
       }
     }

    private void OnAppHostCallShowInMarket (Dictionary<string,string> queryParams)
     {
      string marketUrl = GetQueryParamSafe(queryParams,"app_market_url");

      if (marketUrl != null)
       {
        NotImpl("app_show_in_market request");
       }
      else
       {
        MNDebug.warning("app_market_url parameter is not set in 'app_show_in_market' request");
       }
     }

    private void OnAppHostCallSetHostParam (Dictionary<string,string> queryParams)
     {
      string contextCallWaitLoadParam = GetQueryParamSafe(queryParams,"context_call_wait_load");
      string webShopIsReadyParam      = GetQueryParamSafe(queryParams,"vshop_is_ready");

      if (contextCallWaitLoadParam != null)
       {
        SetContextCallWaitLoad(contextCallWaitLoadParam != "0");
       }

      if (webShopIsReadyParam != null)
       {
        session.SetWebShopReady(webShopIsReadyParam != "0");
       }
     }

    private void  OnAppHostCallExecUICommand (Dictionary<string,string> queryParams)
     {
      if (session != null)
       {
        string cmdNameParam  = GetQueryParamSafe(queryParams,"command_name");
        string cmdParamParam = GetQueryParamSafe(queryParams,"command_param");

        if (cmdNameParam != null)
         {
          session.ExecUICommand(cmdNameParam,cmdParamParam);
         }
        else
         {
          MNDebug.warning("'command_name' parameter is not set in 'exec_ui_command' request");
         }
       }
     }

    private void OnAppHostCallPostWebEvent (Dictionary<string,string> queryParams)
     {
      if (session != null)
       {
        string eventName  = GetQueryParamSafe(queryParams,"event_name");
        string eventParam = GetQueryParamSafe(queryParams,"event_param");
        string callbackId = GetQueryParamSafe(queryParams,"callback_id");

        if (eventName != null)
         {
          session.ProcessWebEvent(eventName,eventParam,callbackId);
         }
        else
         {
          MNDebug.warning("'event_name' parameter is not set in 'post_web_event' request");
         }
       }
     }

    private void OnAppHostCallScriptEval (Dictionary<string,string> queryParams)
     {
      string jsCode = GetQueryParamSafe(queryParams,"jscript_eval");

      if (jsCode != null)
       {
        string forceEval = GetQueryParamSafe(queryParams,"force_eval");
        bool   forceFlag = forceEval != null && forceEval == "1";

        CallJS(jsCode,forceFlag);
       }
     }

    private void OnAppHostCallHttpRequest (Dictionary<string,string> queryParams)
     {
      string reqUrl     = GetQueryParamSafe(queryParams,"req_url");
      string postParams = GetQueryParamSafe(queryParams,"req_post_params");
      string okJSCode   = GetQueryParamSafe(queryParams,"req_ok_eval");
      string failJSCode = GetQueryParamSafe(queryParams,"req_fail_eval");
      long?  flags      = MNUtils.ParseLong(GetQueryParamSafe(queryParams,"req_flags"));

      if (reqUrl != null && okJSCode != null && failJSCode != null && flags != null)
       {
        if (flags >= 0)
         {
          httpReqQueue.AddRequest(reqUrl,postParams,okJSCode,failJSCode,(int)flags);
         }
       }
     }

    private void LoginFacebook (bool isLogin, string successJS, string cancelJS, string[] permissions)
     {
      if (session == null)
       {
        return;
       }

      MNSession.SocNetFBEventHandler eventHandler = (args) =>
       {
        if (args.Cancelled)
         {
          if (cancelJS != null)
           {
            CallJS(cancelJS);
           }
         }
        else if (args.Failed)
         {
          CallJS(string.Format("MN_SetSNContextFacebook(null,{0})",
                               MNUtils.StringAsJSString(args.ErrorMessage)));
         }
        else
         {
          CallJS
           (string.Format("MN_SetSNContextFacebook(new MN_SNContextFacebook('{0}',{1},{2},{3}),null)",
                          -1, // user id is unavailable
                          "''", // session key is not applicable
                          MNUtils.StringAsJSString(session.GetSocNetSessionFB().AccessToken),
                          1)); // user store credentials (access token));

          if (successJS != null)
           {
            CallJS(successJS);
           }
         }
       };

      if (isLogin)
       {
        session.SocNetFBConnect(eventHandler,permissions);
       }
      else
       {
        session.SocNetFBResume(eventHandler);
       }
     }

    private void OnAppHostCallFacebookLogin (Dictionary<string,string> queryParams)
     {
      string   loginSuccessJS = GetQueryParamSafe(queryParams,"mn_callback");
      string   loginCancelJS  = GetQueryParamSafe(queryParams,"mn_cancel");
      string   permissionList = GetQueryParamSafe(queryParams,"permission");
      string[] permissions    = permissionList == null ? null : permissionList.Split(',');

      LoginFacebook(true,loginSuccessJS,loginCancelJS,permissions);
     }

    private void OnAppHostCallFacebookResume (Dictionary<string,string> queryParams)
     {
      LoginFacebook(false,null,null,null);
     }

    private void OnAppHostCallFacebookLogout (Dictionary<string,string> queryParams)
     {
      if (session != null)
       {
        session.SocNetFBLogout();
       }
     }

    private void OnAppHostCallFacebookDialogPublishShow (Dictionary<string,string> queryParams)
     {
      string messagePrompt = GetQueryParamSafe(queryParams,"message_prompt");
      string attachment    = GetQueryParamSafe(queryParams,"attachment");
      string actionLinks   = GetQueryParamSafe(queryParams,"action_links");
      string targetId      = GetQueryParamSafe(queryParams,"target_id");
      string successJS     = GetQueryParamSafe(queryParams,"mn_callback");
      string cancelJS      = GetQueryParamSafe(queryParams,"mn_cancel");

      if (messagePrompt == null || attachment == null || actionLinks == null ||
          targetId == null || successJS == null || cancelJS == null)
       {
        MNDebug.debug("one or more parameters are not available in 'facebook publish' request");

        return;
       }

      if (session == null)
       {
        return;
       }
      
      session.GetSocNetSessionFB().ShowStreamDialog(messagePrompt,attachment,targetId,actionLinks,(result) =>
       {
        if (result.Cancelled)
         {
          CallJS(cancelJS);
         }
        else if (result.Succeeded)
         {
          CallJS(successJS);
         }
        else
         {
          CallSetErrorMessage(result.ErrorMessage,MNErrorInfo.ACTION_CODE_UNDEFINED);
         }
       });
     }

    private void OnAppHostCallFacebookDialogPermissionShow (Dictionary<string,string> queryParams)
     {
      string permission = GetQueryParamSafe(queryParams,"permission");
      string successJS  = GetQueryParamSafe(queryParams,"mn_callback");
      string cancelJS   = GetQueryParamSafe(queryParams,"mn_cancel");

      if (permission == null || successJS == null || cancelJS == null)
       {
        MNDebug.debug("one or more parameters are not available in 'facebook permissions' request");

        return;
       }

      if (session == null)
       {
        return;
       }

      session.GetSocNetSessionFB().ShowPermissionDialog(permission,(result) =>
       {
        if (result.Cancelled)
         {
          if (cancelJS != null)
           {
            CallJS(cancelJS);
           }
         }
        else if (result.Succeeded)
         {
          if (successJS != null)
           {
            CallJS(successJS);
           }
         }
        else
         {
          CallSetErrorMessage(result.ErrorMessage,MNErrorInfo.ACTION_CODE_UNDEFINED);
         }
       });
     }

    private void OnAppHostCallVarSave (Dictionary<string,string> queryParams)
     {
      if (session != null)
       {
        string name  = GetQueryParamSafe(queryParams,"var_name");
        string value = GetQueryParamSafe(queryParams,"var_value");

        if (name != null)
         {
          session.VarStorageSetValue(name,value);
         }
       }
     }

    private void OnAppHostCallVarsClear (Dictionary<string,string> queryParams)
     {
      if (session != null)
       {
        string list = GetQueryParamSafe(queryParams,"var_name_list");

        if (list != null)
         {
          session.VarStorageRemoveVariablesByMasks(list.Split(','));
         }
       }
     }

    private string MakeConfigVarsList (Dictionary<string,string> vars)
     {
      bool          first      = true;
      StringBuilder varListSrc = new StringBuilder();

      foreach (var entry in vars)
       {
        if (first)
         {
          first = false;
         }
        else
         {
          varListSrc.Append(',');
         }

        varListSrc.Append("new MN_HostVar(");
        varListSrc.Append(MNUtils.StringAsJSString(entry.Key));
        varListSrc.Append(',');
        varListSrc.Append(MNUtils.StringAsJSString(entry.Value));
        varListSrc.Append(')');
       }

      return varListSrc.ToString();
     }

    private void OnAppHostCallVarsGet (Dictionary<string,string> queryParams)
     {
      string varList = GetQueryParamSafe(queryParams,"var_name_list");

      if (varList != null)
       {
        CallJS("MN_HostVarUpdate(new Array(" +
               MakeConfigVarsList(session.VarStorageGetValuesByMasks(varList.Split(','))) +
               "))");
       }
     }

    private void OnAppHostCallConfigVarsGet (Dictionary<string,string> queryParams)
     {
      if (session == null)
       {
        return;
       }

      Todo("add tracking vars to MN_ConfigVarUpdate call");

      CallJS("MN_ConfigVarUpdate(new Array(),new Array(" +
             MakeConfigVarsList
              (session.GetAppConfigVars()) +
            "))");
     }

    private void OnAppHostCallDoPhotoImport (Dictionary<string,string> queryParams)
     {
      NotImpl("do_photo_import apphost call");
     }

    private void OnAppHostCallGetUserABData (Dictionary<string,string> queryParams)
     {
      NotImpl("get_user_ab_data apphost call");
     }

    private void OnAppHostCallVoid (Dictionary<string,string> queryParams)
     {
     }

    private void OnAppHostCallNavBarCancelUrlLoad (Dictionary<string,string> queryParams)
     {
      loadNavBarExtRequest = false;
     }

    private void OnAppHostCallGetContext (Dictionary<string,string> queryParams)
     {
      ScheduleUpdateContext();
     }
    #endregion

    private void OnNavBarNavigating (object sender, NavigatingEventArgs args)
     {
      if (!IsHostTrusted(args.Uri.Host))
       {
        loadNavBarExtRequest = true;

        EvalJS(navBarView,"MN_NavBarCheckLoadUrl(" +
                          MNUtils.StringAsJSString(args.Uri.ToString()) +
                          ",null)");

        if (!loadNavBarExtRequest)
         {
          args.Cancel = true;
         }
       }
     }

    private void LoadBootPage ()
     {
      string pageContent = null;
      Uri    pageUri     = new Uri(BOOT_PAGE_FILE_NAME,UriKind.Relative);

      try
       {
        StreamResourceInfo sourceStreamInfo = Application.GetResourceStream(pageUri);

        if (sourceStreamInfo != null)
         {
          StreamReader reader = new StreamReader(sourceStreamInfo.Stream);

          pageContent = reader.ReadToEnd();
         }
       }
      catch (IOException)
       {
       }

      if (pageContent != null)
       {
        webView.Base = pageUri.ToString();
        webView.NavigateToString(pageContent);
       }
     }

    private const string ERROR_PAGE_TEMPLATE_EMBEDDED =
     "<html>" +
      "<head>" +
        "<title>MultiNet: Error</title>" +
      "</head>" +
      "<body onclick=\"location.assign('apphost_goback.php');\">" +
       "<div align=\"center\" style=\"padding:20px;padding-top:150px;padding-bottom:150px;color:red\" " +
       "     onclick=\";\" " + // Empty event handler is needed to workaround browser bug (onclick not fired on body if no handler defined here)
       ">" +
        "<b>MultiNet: Error</b><br/>" +
        "<script>document.write({0});</script>" +
       "</div>" +
      "</body>" +
     "</html>";

    private void LoadErrorMessagePage (string errorMessage)
     {
      string errorPageTemplate = null;
      Uri    pageUri           = new Uri(ERROR_PAGE_FILE_NAME,UriKind.Relative);

      try
       {
        StreamResourceInfo sourceStreamInfo = Application.GetResourceStream(pageUri);

        if (sourceStreamInfo != null)
         {
          StreamReader reader = new StreamReader(sourceStreamInfo.Stream);

          errorPageTemplate = reader.ReadToEnd();
         }
       }
      catch (IOException)
       {
       }

      if (errorPageTemplate == null)
       {
        errorPageTemplate = ERROR_PAGE_TEMPLATE_EMBEDDED;
       }

      string pageContent = errorPageTemplate.Replace
                            ("{0}",MNUtils.StringAsJSString(errorMessage));

      if (navBarView.Visibility == Visibility.Visible)
       {
        navBarView.Visibility = Visibility.Collapsed;
       }

      errorPageLoaded = true;

      webView.Base = pageUri.ToString();
      webView.NavigateToString(pageContent);
     }

    private static string GetQueryParamSafe (Dictionary<string,string> queryParams, string name)
     {
      string value;

      if (queryParams.TryGetValue(name,out value))
       {
        return value;
       }
      else
       {
        return null;
       }
     }

    private static void Todo (string message)
     {
      //FIXME: remove this method in release

      MNDebug.todo("MNUserProfileView: " + message);
     }

    private static void NotImpl (string feature)
     {
      MNDebug.NotImpl("MNUserProfileView: " + feature);
     }

    private delegate void AppHostCallHander (Dictionary<string,string> appHostParams);

    private WebBrowser       webView;
    private WebBrowser       navBarView;
    private bool             navBarViewReady;
    private string           webServerUrl;
    private MNSession        session;
    private MNTrustedHosts   trustedHosts;
    private bool             autoCancelGameOnGoBack;
    private bool             contextCallWaitLoad;
    private bool             errorPageLoaded;
    private MNStrMaskStorage trackedPluginsStorage;
    private string           devUsersInfoSrcCache;
    private bool             loadNavBarExtRequest;

    private MNUIWebViewHttpReqQueue httpReqQueue;

    private Dictionary<string,AppHostCallHander> appHostCallHandlers;

    private const int    NAV_BAR_DEFAULT_HEIGHT = 49;
    private const double NAV_BAR_SCALE_FACTOR   = 480.0 / 320.0; // device width / webview width

    private const string URI_SCHEME_FILE  = "file";
    private const string URI_SCHEME_HTTP  = "http";
    private const string URI_SCHEME_HTTPS = "https";

    //FIXME: get base dir from platform
    private const string BOOT_PAGE_FILE_NAME  = "Assets/multinet_boot.html";
    private const string ERROR_PAGE_FILE_NAME = "Assets/multinet_http_error.html";

    private const string GAMESET_PLAY_PARAM_PREFIX = "gameset_play_param_";
    private readonly int GAMESET_PLAY_PARAM_PREFIX_LEN = GAMESET_PLAY_PARAM_PREFIX.Length;

    private const int HTTPREQ_FLAG_EVAL_IN_MAINWEBVIEW_MASK   = 0x0001;
    private const int HTTPREQ_FLAG_EVAL_IN_NAVBARWEBVIEW_MASK = 0x0002;
   }
 }
