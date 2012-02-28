//
//  MNWSProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

using PlayPhone.MultiNet.Core;
using PlayPhone.MultiNet.Core.WS;
using PlayPhone.MultiNet.Core.WS.Data;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNWSProvider
   {
    public MNWSProvider (MNSession session)
     {
      this.session = session;
     }

    public void Shutdown ()
     {
      session = null;
     }

    public MNWSLoader Send (MNWSInfoRequest request)
     {
      return Send(new MNWSInfoRequest[] { request });
     }

    public MNWSLoader Send (MNWSInfoRequest[] requests)
     {
      MNWSRequestSender  sender  = new MNWSRequestSender(session);
      MNWSRequestContent content = PrepareContent(requests);

      return new MNWSLoader
                  (sender.SendWSRequestSmartAuth
                    (content,
                     (response) =>
                      {
                       foreach (var request in requests)
                        {
                         request.HandleRequestCompleted(response);
                        }
                      },
                     (error) =>
                      {
                       foreach (var request in requests)
                        {
                         request.HandleRequestError(error);
                        }
                      }));
     }

    private static MNWSRequestContent PrepareContent (MNWSInfoRequest[] requests)
     {
      MNWSRequestContent content = new MNWSRequestContent();

      foreach (var request in requests)
       {
        request.AddContent(content);
       }

      return content;
     }

    private MNSession session;
   }

  public class MNWSLoader
   {
    public void Cancel ()
     {
      request.Cancel();
     }

    internal MNWSLoader (IMNWSRequest request)
     {
      this.request = request;
     }

    private IMNWSRequest request;
   }

  public abstract class MNWSInfoRequest
   {
    public class RequestResult
     {
      public bool   HadError     { get; internal set; }
      public string ErrorMessage { get; internal set; }

      protected RequestResult ()
       {
        HadError     = false;
        ErrorMessage = null;
       }

      internal void SetError (string errorMessage)
       {
        HadError     = true;
        ErrorMessage = errorMessage;
       }
     }

    internal abstract void AddContent             (MNWSRequestContent content);
    internal abstract void HandleRequestCompleted (MNWSResponse       response);
    internal abstract void HandleRequestError     (MNWSRequestError   error);
   }

  #region MNWSInfoRequestAnyGame
  public class MNWSInfoRequestAnyGame : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestAnyGame (int gameId, OnCompleted onCompleted)
     {
      this.gameId      = gameId;
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddAnyGame(gameId);
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      DispatchResult(new RequestResult((MNWSAnyGameItem)response.GetDataForBlock(blockName)));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSAnyGameItem DataEntry { get; internal set; }

      public MNWSAnyGameItem GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSAnyGameItem data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private readonly int gameId;
    private string       blockName;
    private OnCompleted  onCompleted;
   }
  #endregion

  #region MNWSInfoRequestAnyUser
  public class MNWSInfoRequestAnyUser : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestAnyUser (long userId, OnCompleted onCompleted)
     {
      this.userId      = userId;
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddAnyUser(userId);
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      DispatchResult(new RequestResult((MNWSAnyUserItem)response.GetDataForBlock(blockName)));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSAnyUserItem DataEntry { get; internal set; }

      public MNWSAnyUserItem GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSAnyUserItem data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private readonly long userId;
    private string        blockName;
    private OnCompleted   onCompleted;
   }
  #endregion

  #region MNWSInfoRequestAnyUserGameCookies
  public class MNWSInfoRequestAnyUserGameCookies : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestAnyUserGameCookies (long[] userIdList, int[] cookieKeyList, OnCompleted onCompleted)
     {
      this.userIdList    = userIdList;
      this.cookieKeyList = cookieKeyList;
      this.onCompleted   = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddAnyUserGameCookies(userIdList,cookieKeyList);
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      List<MNWSUserGameCookie> cookiesList = (List<MNWSUserGameCookie>)response.GetDataForBlock(blockName);

      DispatchResult(new RequestResult(cookiesList.ToArray()));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSUserGameCookie[] DataEntry { get; internal set; }

      public MNWSUserGameCookie[] GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSUserGameCookie[] data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private readonly long[] userIdList;
    private readonly int[]  cookieKeyList;
    private string          blockName;
    private OnCompleted     onCompleted;
   }
  #endregion

  #region MNWSInfoRequestCurrentUserInfo
  public class MNWSInfoRequestCurrentUserInfo : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestCurrentUserInfo (OnCompleted onCompleted)
     {
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddCurrentUserInfo();
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      DispatchResult(new RequestResult((MNWSCurrentUserInfo)response.GetDataForBlock(blockName)));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSCurrentUserInfo DataEntry { get; internal set; }

      public MNWSCurrentUserInfo GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSCurrentUserInfo data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private string      blockName;
    private OnCompleted onCompleted;
   }
  #endregion

  #region MNWSInfoRequestCurrGameRoomList
  public class MNWSInfoRequestCurrGameRoomList : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestCurrGameRoomList (OnCompleted onCompleted)
     {
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddCurrGameRoomList();
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      List<MNWSRoomListItem> roomList = (List<MNWSRoomListItem>)response.GetDataForBlock(blockName);

      DispatchResult(new RequestResult(roomList.ToArray()));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSRoomListItem[] DataEntry { get; internal set; }

      public MNWSRoomListItem[] GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSRoomListItem[] data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private string          blockName;
    private OnCompleted     onCompleted;
   }
  #endregion

  #region MNWSInfoRequestCurrGameRoomUserList
  public class MNWSInfoRequestCurrGameRoomUserList : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestCurrGameRoomUserList (int roomSFId, OnCompleted onCompleted)
     {
      this.roomSFId    = roomSFId;
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddCurrGameRoomUserList(roomSFId);
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      List<MNWSRoomUserInfoItem> userList = (List<MNWSRoomUserInfoItem>)response.GetDataForBlock(blockName);

      DispatchResult(new RequestResult(userList.ToArray()));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSRoomUserInfoItem[] DataEntry { get; internal set; }

      public MNWSRoomUserInfoItem[] GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSRoomUserInfoItem[] data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private int         roomSFId;
    private string      blockName;
    private OnCompleted onCompleted;
   }
  #endregion

  #region MNWSInfoRequestCurrUserBuddyList
  public class MNWSInfoRequestCurrUserBuddyList : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestCurrUserBuddyList (OnCompleted onCompleted)
     {
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddCurrUserBuddyList();
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      List<MNWSBuddyListItem> buddyList = (List<MNWSBuddyListItem>)response.GetDataForBlock(blockName);

      DispatchResult(new RequestResult(buddyList.ToArray()));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSBuddyListItem[] DataEntry { get; internal set; }

      public MNWSBuddyListItem[] GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSBuddyListItem[] data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private string          blockName;
    private OnCompleted     onCompleted;
   }
  #endregion

  #region MNWSInfoRequestCurrUserSubscriptionStatus
  public class MNWSInfoRequestCurrUserSubscriptionStatus : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestCurrUserSubscriptionStatus (OnCompleted onCompleted)
     {
      this.socNetId    = MNWSRequestContent.SN_ID_PLAYPHONE;
      this.onCompleted = onCompleted;
     }

    protected MNWSInfoRequestCurrUserSubscriptionStatus (int socNetId, OnCompleted onCompleted)
     {
      this.socNetId    = socNetId;
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddCurrUserSubscriptionStatus(socNetId);
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      DispatchResult(new RequestResult((MNWSCurrUserSubscriptionStatus)response.GetDataForBlock(blockName)));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSCurrUserSubscriptionStatus DataEntry { get; internal set; }

      public MNWSCurrUserSubscriptionStatus GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSCurrUserSubscriptionStatus data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private readonly int socNetId;
    private string       blockName;
    private OnCompleted  onCompleted;
   }
  #endregion

  #region MNWSInfoRequestLeaderboard
  public class MNWSInfoRequestLeaderboard : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public const int LEADERBOARD_PERIOD_ALL_TIME   = MNWSRequestContent.LEADERBOARD_PERIOD_ALL_TIME;
    public const int LEADERBOARD_PERIOD_THIS_WEEK  = MNWSRequestContent.LEADERBOARD_PERIOD_THIS_WEEK;
    public const int LEADERBOARD_PERIOD_THIS_MONTH = MNWSRequestContent.LEADERBOARD_PERIOD_THIS_MONTH;

    public const int LEADERBOARD_SCOPE_GLOBAL = MNWSRequestContent.LEADERBOARD_SCOPE_GLOBAL;
    public const int LEADERBOARD_SCOPE_LOCAL  = MNWSRequestContent.LEADERBOARD_SCOPE_LOCAL;

    public class LeaderboardModeCurrentUser : LeaderboardMode
     {
      public LeaderboardModeCurrentUser (int scope, int period)
       {
        this.scope  = scope;
        this.period = period;
       }

      internal override string AddContent (MNWSRequestContent content)
       {
        return content.AddCurrUserLeaderboard(scope,period);
       }

      private readonly int scope;
      private readonly int period;
     }

    public class LeaderboardModeAnyGameGlobal : LeaderboardMode
     {
      public LeaderboardModeAnyGameGlobal (int gameId, int gameSetId, int period)
       {
        this.gameId    = gameId;
        this.gameSetId = gameSetId;
        this.period    = period;
       }

      internal override string AddContent (MNWSRequestContent content)
       {
        return content.AddAnyGameLeaderboardGlobal(gameId,gameSetId,period);
       }

      private readonly int gameId;
      private readonly int gameSetId;
      private readonly int period;
     }

    public class LeaderboardModeAnyUserAnyGameGlobal : LeaderboardMode
     {
      public LeaderboardModeAnyUserAnyGameGlobal (long userId, int gameId, int gameSetId, int period)
       {
        this.userId    = userId;
        this.gameId    = gameId;
        this.gameSetId = gameSetId;
        this.period    = period;
       }

      internal override string AddContent (MNWSRequestContent content)
       {
        return content.AddAnyUserAnyGameLeaderboardGlobal(userId,gameId,gameSetId,period);
       }

      private readonly long userId;
      private readonly int  gameId;
      private readonly int  gameSetId;
      private readonly int  period;
     }

    public class LeaderboardModeCurrUserAnyGameLocal : LeaderboardMode
     {
      public LeaderboardModeCurrUserAnyGameLocal (int gameId, int gameSetId, int period)
       {
        this.gameId    = gameId;
        this.gameSetId = gameSetId;
        this.period    = period;
       }

      internal override string AddContent (MNWSRequestContent content)
       {
        return content.AddCurrUserAnyGameLeaderboardLocal(gameId,gameSetId,period);
       }

      private readonly int  gameId;
      private readonly int  gameSetId;
      private readonly int  period;
     }

    public abstract class LeaderboardMode
     {
      internal abstract string AddContent (MNWSRequestContent content);
     }

    public MNWSInfoRequestLeaderboard (LeaderboardMode mode, OnCompleted onCompleted)
     {
      this.mode        = mode;
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = mode.AddContent(content);
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      List<MNWSLeaderboardListItem> leaderboard = (List<MNWSLeaderboardListItem>)response.GetDataForBlock(blockName);

      DispatchResult(new RequestResult(leaderboard.ToArray()));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSLeaderboardListItem[] DataEntry { get; internal set; }

      public MNWSLeaderboardListItem[] GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSLeaderboardListItem[] data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private LeaderboardMode mode;
    private string          blockName;
    private OnCompleted     onCompleted;
   }
  #endregion

  #region MNWSInfoRequestSessionSignedClientToken
  public class MNWSInfoRequestSessionSignedClientToken : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestSessionSignedClientToken (string payload, OnCompleted onCompleted)
     {
      this.payload     = payload;
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddGetSessionSignedClientToken(payload);
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      DispatchResult(new RequestResult((MNWSSessionSignedClientToken)response.GetDataForBlock(blockName)));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSSessionSignedClientToken DataEntry { get; internal set; }

      public MNWSSessionSignedClientToken GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSSessionSignedClientToken data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private readonly string payload;
    private string          blockName;
    private OnCompleted     onCompleted;
   }
  #endregion

  #region MNWSInfoRequestSystemGameNetStats
  public class MNWSInfoRequestSystemGameNetStats : MNWSInfoRequest
   {
    public delegate void OnCompleted (RequestResult result);

    public MNWSInfoRequestSystemGameNetStats (OnCompleted onCompleted)
     {
      this.onCompleted = onCompleted;
     }

    internal override void AddContent (MNWSRequestContent content)
     {
      blockName = content.AddSystemGameNetStats();
     }

    internal override void HandleRequestCompleted (MNWSResponse response)
     {
      DispatchResult(new RequestResult((MNWSSystemGameNetStats)response.GetDataForBlock(blockName)));
     }

    internal override void HandleRequestError (MNWSRequestError error)
     {
      DispatchResult(new RequestResult(error));
     }

    private void DispatchResult (RequestResult result)
     {
      if (onCompleted != null)
       {
        onCompleted(result);
       }
     }

    public new class RequestResult : MNWSInfoRequest.RequestResult
     {
      public MNWSSystemGameNetStats DataEntry { get; internal set; }

      public MNWSSystemGameNetStats GetDataEntry ()
       {
        return DataEntry;
       }

      internal RequestResult (MNWSSystemGameNetStats data)
       {
        DataEntry = data;
       }

      internal RequestResult (MNWSRequestError error)
       {
        SetError(error.Message);
       }
     }

    private string          blockName;
    private OnCompleted     onCompleted;
   }
  #endregion
 }
