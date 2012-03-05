//
//  MNDirect.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using PlayPhone.MultiNet.Core;
using PlayPhone.MultiNet.Providers;

namespace PlayPhone.MultiNet
 {
  public static class MNDirect
   {
    public delegate void DoStartGameWithParamsEventHandler  (MNGameParams gameParams);
    public delegate void DoFinishGameEventHandler           ();
    public delegate void DoCancelGameEventHandler           ();
    public delegate void SessionStatusChangedEventHandler   (int newStatus);
    public delegate void DidReceiveGameMessageEventHandler  (string message, MNUserInfo sender);
    public delegate void ErrorOccurredEventHandler          (MNErrorInfo errorInfo);
    public delegate void ViewDoGoBackEventHandler           ();

    public static event DoStartGameWithParamsEventHandler DoStartGameWithParams;
    public static event DoFinishGameEventHandler          DoFinishGame;
    public static event DoCancelGameEventHandler          DoCancelGame;
    public static event SessionStatusChangedEventHandler  SessionStatusChanged;
    public static event DidReceiveGameMessageEventHandler DidReceiveGameMessage;
    public static event ErrorOccurredEventHandler         ErrorOccurred;
    public static event ViewDoGoBackEventHandler          ViewDoGoBack;

    public static void Init (int gameId, String gameSecret)
     {
      gameParams = null;

      session = new MNSession(gameId,gameSecret);

      InitProviders();

      MNDebug.todo("MNDirect.MNDirectSessionReady event is not supported");

      view = new MNUserProfileView();
      view.DoGoBack += OnUserProfileViewDoGoBack;
      view.BindToSession(session);

      session.DoStartGameWithParams += OnSessionDoStartGameWithParams;
      session.DoFinishGame          += OnSessionDoFinishGame;
      session.DoCancelGame          += OnSessionDoCancelGame;
      session.SessionStatusChanged  += OnSessionStatusChanged;
      session.GameMessageReceived   += OnSessionGameMessageReceived;
      session.ErrorOccurred         += OnSessionErrorOccurred;
     }

    public static string MakeGameSecretByComponents (uint secret1, uint secret2, uint secret3, uint secret4)
     {
      return MNUtils.MakeGameSecretByComponents(secret1,secret2,secret3,secret4);
     }

    public static void ShutdownSession ()
     {
      if (view != null)
       {
        view.Destroy();

        view = null;
       }

      ReleaseProviders();

      if (session != null)
       {
        session.DoStartGameWithParams -= OnSessionDoStartGameWithParams;
        session.DoFinishGame          -= OnSessionDoFinishGame;
        session.DoCancelGame          -= OnSessionDoCancelGame;
        session.SessionStatusChanged  -= OnSessionStatusChanged;
        session.GameMessageReceived   -= OnSessionGameMessageReceived;
        session.ErrorOccurred         -= OnSessionErrorOccurred;

        session.Shutdown();

        session = null;
       }
     }

    public static bool IsOnline ()
     {
      return session != null ? session.IsOnline() : false;
     }

    public static bool IsUserLoggedIn ()
     {
      return session != null ? session.IsUserLoggedIn() : false;
     }

    public static int GetSessionStatus ()
     {
      if (session != null)
       {
        return session.GetStatus();
       }
      else
       {
        return MNConst.MN_OFFLINE;
       }
     }

    public static void PostGameScore (long score)
     {
      if (session != null)
       {
        if (gameParams == null)
         {
          gameParams =
           new MNGameParams(session.GetDefaultGameSetId(),"","",0,
                            MNGameParams.MN_PLAYMODEL_SINGLEPLAY);
         }

        MNGameResult gameResult = new MNGameResult(gameParams);

        gameResult.Score = score;

        session.FinishGameWithResult(gameResult);

        gameParams = null;
       }
     }

    public static void PostGameScorePending (long score)
     {
      if (session != null)
       {
        if (gameParams == null)
         {
          gameParams =
           new MNGameParams(session.GetDefaultGameSetId(),"","",0,
                            MNGameParams.MN_PLAYMODEL_SINGLEPLAY);
         }

        MNGameResult gameResult = new MNGameResult(gameParams);

        gameResult.Score = score;

        session.SchedulePostScoreOnLogin(gameResult);

        gameParams = null;
       }
     }

    public static void CancelGame ()
     {
      if (session != null)
       {
        session.CancelPostScoreOnLogin();
        session.CancelGameWithParams(gameParams);

        gameParams = null;
       }
     }

    public static void SetDefaultGameSetId (int gameSetId)
     {
      if (session != null)
       {
        session.SetDefaultGameSetId(gameSetId);
       }
     }

    public static int GetDefaultGameSetId ()
     {
      if (session != null)
       {
        return session.GetDefaultGameSetId();
       }
      else
       {
        return MNGameParams.MN_GAMESET_ID_DEFAULT;
       }
     }

    public static void SendAppBeacon (string actionName, string beaconData)
     {
      if (session != null)
       {
        session.SendAppBeacon(actionName,beaconData);
       }
     }

    public static void ExecAppCommand(string name, string param)
     {
      if (session != null)
       {
        session.ExecAppCommand(name,param);
       }
     }

    public static void SendGameMessage (string message)
     {
      if (session != null)
       {
        session.SendGameMessage(message);
       }
     }

    public static MNSession GetSession ()
     {
      return session;
     }

    public static MNUserProfileView GetView ()
     {
      return view;
     }

    private static void InitProviders ()
     {
      ReleaseProviders();

      achievementsProvider    = new MNAchievementsProvider(session);
      clientRobotsProvider    = new MNClientRobotsProvider(session);
      gameCookiesProvider     = new MNGameCookiesProvider(session);
      gameRoomCookiesProvider = new MNGameRoomCookiesProvider(session);
      myHiScoresProvider      = new MNMyHiScoresProvider(session);
      playerListProvider      = new MNPlayerListProvider(session);
      scoreProgressProvider   = new MNScoreProgressProvider(session);
      vItemsProvider          = new MNVItemsProvider(session);
      vShopProvider           = new MNVShopProvider(session,vItemsProvider);
      gameSettingsProvider    = new MNGameSettingsProvider(session);
      serverInfoProvider      = new MNServerInfoProvider(session);
      wsProvider              = new MNWSProvider(session);
     }

    private static void ReleaseProviders ()
     {
      if (achievementsProvider != null)
       {
        achievementsProvider.Shutdown(); achievementsProvider = null;
       }

      if (clientRobotsProvider != null)
       {
        clientRobotsProvider.Shutdown(); clientRobotsProvider = null;
       }

      if (gameCookiesProvider != null)
       {
        gameCookiesProvider.Shutdown(); gameCookiesProvider = null;
       }

      if (gameRoomCookiesProvider != null)
       {
        gameRoomCookiesProvider.Shutdown(); gameRoomCookiesProvider = null;
       }

      if (myHiScoresProvider != null)
       {
        myHiScoresProvider.Shutdown(); myHiScoresProvider = null;
       }

      if (playerListProvider != null)
       {
        playerListProvider.Shutdown(); playerListProvider = null;
       }

      if (scoreProgressProvider != null)
       {
        scoreProgressProvider.Shutdown(); scoreProgressProvider = null;
       }

      if (vShopProvider != null)
       {
        vShopProvider.Shutdown(); vShopProvider = null;
       }

      if (vItemsProvider != null)
       {
        vItemsProvider.Shutdown(); vItemsProvider = null;
       }

      if (gameSettingsProvider != null)
       {
        gameSettingsProvider.Shutdown(); gameSettingsProvider = null;
       }

      if (serverInfoProvider != null)
       {
        serverInfoProvider.Shutdown(); serverInfoProvider = null;
       }

      if (wsProvider != null)
       {
        wsProvider.Shutdown(); wsProvider = null;
       }
     }

    public static MNAchievementsProvider GetAchievementsProvider ()
     {
      return achievementsProvider;
     }

    public static MNClientRobotsProvider GetClientRobotsProvider ()
     {
      return clientRobotsProvider;
     }

    public static MNGameCookiesProvider GetGameCookiesProvider ()
     {
      return gameCookiesProvider;
     }

    public static MNGameRoomCookiesProvider GetGameRoomCookiesProvider ()
     {
      return gameRoomCookiesProvider;
     }

    public static MNMyHiScoresProvider GetMyHiScoresProvider ()
     {
      return myHiScoresProvider;
     }

    public static MNPlayerListProvider GetPlayerListProvider ()
     {
      return playerListProvider;
     }

    public static MNScoreProgressProvider GetScoreProgressProvider ()
     {
      return scoreProgressProvider;
     }

    public static MNVItemsProvider     GetVItemsProvider ()
     {
      return vItemsProvider;
     }

    public static MNVShopProvider      GetVShopProvider ()
     {
      return vShopProvider;
     }

    public static MNGameSettingsProvider GetGameSettingsProvider ()
     {
      return gameSettingsProvider;
     }

    public static MNServerInfoProvider GetServerInfoProvider ()
     {
      return serverInfoProvider;
     }

    public static MNWSProvider GetWSProvider ()
     {
      return wsProvider;
     }

    private static void OnSessionDoStartGameWithParams (MNGameParams gameParams)
     {
      MNDirect.gameParams = gameParams;

      DoStartGameWithParamsEventHandler handler = DoStartGameWithParams;

      if (handler != null)
       {
        handler(gameParams);
       }
     }

    private static void OnSessionDoFinishGame ()
     {
      DoFinishGameEventHandler handler = DoFinishGame;

      if (handler != null)
       {
        handler();
       }
     }

    private static void OnSessionDoCancelGame ()
     {
      session.CancelPostScoreOnLogin();

      gameParams = null;

      DoCancelGameEventHandler handler = DoCancelGame;

      if (handler != null)
       {
        handler();
       }
     }

    private static void OnSessionStatusChanged (int newStatus, int oldStatus)
     {
      SessionStatusChangedEventHandler handler = SessionStatusChanged;

      if (handler != null)
       {
        handler(newStatus);
       }
     }

    private static void OnSessionGameMessageReceived (string message, MNUserInfo sender)
     {
      DidReceiveGameMessageEventHandler handler = DidReceiveGameMessage;

      if (handler != null)
       {
        handler(message,sender);
       }
     }

    private static void OnSessionErrorOccurred (MNErrorInfo errorInfo)
     {
      ErrorOccurredEventHandler handler = ErrorOccurred;

      if (handler != null)
       {
        handler(errorInfo);
       }
     }

    private static void OnUserProfileViewDoGoBack ()
     {
      session.CancelPostScoreOnLogin();

      ViewDoGoBackEventHandler handler = ViewDoGoBack;

      if (handler != null)
       {
        handler();
       }
     }

    private static MNSession         session;
    private static MNUserProfileView view;
    private static MNGameParams      gameParams;

    private static MNAchievementsProvider    achievementsProvider;
    private static MNClientRobotsProvider    clientRobotsProvider;
    private static MNGameCookiesProvider     gameCookiesProvider;
    private static MNGameRoomCookiesProvider gameRoomCookiesProvider;
    private static MNMyHiScoresProvider      myHiScoresProvider;
    private static MNPlayerListProvider      playerListProvider;
    private static MNScoreProgressProvider   scoreProgressProvider;
    private static MNVItemsProvider          vItemsProvider;
    private static MNVShopProvider           vShopProvider;
    private static MNGameSettingsProvider    gameSettingsProvider;
    private static MNServerInfoProvider      serverInfoProvider;
    private static MNWSProvider              wsProvider;
   }
 }
