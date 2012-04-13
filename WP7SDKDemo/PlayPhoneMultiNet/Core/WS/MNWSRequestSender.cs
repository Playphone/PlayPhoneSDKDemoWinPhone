//
//  MNWSRequestSender.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.IO;
using PlayPhone.MultiNet.Core.WS.Parser;
using PlayPhone.MultiNet.Core.WS.Data;

namespace PlayPhone.MultiNet.Core.WS
 {
  public class MNWSRequestSender
   {
    public delegate void RequestCompletedEventHandler (MNWSResponse     response);
    public delegate void RequestErrorEventHandler     (MNWSRequestError error);

    public MNWSRequestSender (MNSession session)
     {
      this.session = session;

      parsers = new Dictionary<string,IMNWSXmlDataParser>();

      SetupStdParser();
     }

    public IMNWSRequest SendWSRequest (MNWSRequestContent           content,
                                       RequestCompletedEventHandler requestCompleted,
                                       RequestErrorEventHandler     requestError)
     {
      return SendWSRequest(content,requestCompleted,requestError,false,false);
     }

    public IMNWSRequest SendWSRequestAuthorized
                                      (MNWSRequestContent           content,
                                       RequestCompletedEventHandler requestCompleted,
                                       RequestErrorEventHandler     requestError)
     {
      return SendWSRequest(content,requestCompleted,requestError,true,false);
     }

    public IMNWSRequest SendWSRequestSmartAuth
                                      (MNWSRequestContent           content,
                                       RequestCompletedEventHandler requestCompleted,
                                       RequestErrorEventHandler     requestError)
     {
      return SendWSRequest(content,requestCompleted,requestError,true,true);
     }

    public IMNWSRequest SendWSRequest (MNWSRequestContent           content,
                                       RequestCompletedEventHandler requestCompleted,
                                       RequestErrorEventHandler     requestError,
                                       bool                         authorized,
                                       bool                         smartAuth)
     {
      string webServerUrl = session != null ? session.GetWebFrontURL() : null;

      if (webServerUrl == null)
       {
        if (requestError != null)
         {
          requestError(new MNWSRequestError(MNWSRequestError.TRANSPORT_ERROR,
                                            "request cannot be sent (server url is undefined)"));

          return null;
         }
       }

      string userSId = session.GetMySId();

      if (smartAuth)
       {
        authorized = userSId != null;
       }

      if (authorized && userSId == null)
       {
        if (requestError != null)
         {
          requestError(new MNWSRequestError(MNWSRequestError.PARAMETERS_ERROR,
                                            "authorized request cannot be sent if user is not logged in"));

          return null;
         }
       }

      StringBuilder requestUrl = new StringBuilder(webServerUrl);

      requestUrl.Append('/');
      requestUrl.Append(WS_URL_PATH);

      Dictionary<string,string> wsParams = new Dictionary<string,string>();

      wsParams["ctx_game_id"]    = session.GetGameId().ToString();
      wsParams["ctx_gameset_id"] = session.GetDefaultGameSetId().ToString();
      wsParams["ctx_dev_type"]   = MNPlatformWinPhone.GetDeviceType().ToString();
      wsParams["ctx_dev_id"]     = MNUtils.StringGetMD5String(session.GetUniqueAppId());

      if (authorized)
       {
        wsParams["ctx_user_id"]  = session.GetMyUserId().ToString();
        wsParams["ctx_user_sid"] = userSId;
       }

      wsParams["info_list"] = content.GetRequestInfoListString();

      MNWSRequest request = new MNWSRequest(requestCompleted,requestError,content.GetMapping(),parsers);

      request.SendRequest(requestUrl.ToString(),wsParams);

      return request;
     }

    private void SetupStdParser()
     {
      IMNWSXmlDataParser buddyListParser =
       new MNWSXmlGenericItemListParser<MNWSBuddyListItem>("buddyItem");

      IMNWSXmlDataParser leaderboardListParser =
       new MNWSXmlGenericItemListParser<MNWSLeaderboardListItem>("leaderboardItem");

      IMNWSXmlDataParser gameRoomListParser =
       new MNWSXmlGenericItemListParser<MNWSRoomListItem>("roomInfoItem");

      IMNWSXmlDataParser gameRoomUserListParser =
       new MNWSXmlGenericItemListParser<MNWSRoomUserInfoItem>("roomUserInfoItem");

      IMNWSXmlDataParser userGameCookiesListParser =
       new MNWSXmlGenericItemListParser<MNWSUserGameCookie>("anyUserGameCookieItem");

      parsers["currentUserBuddyList"] = buddyListParser;
      parsers["anyUser"] = new MNWSXmlGenericItemParser<MNWSAnyUserItem>();
      parsers["anyGame"] = new MNWSXmlGenericItemParser<MNWSAnyGameItem>();
      parsers["currentUserLeaderboardGlobalThisWeek"]        = leaderboardListParser;
      parsers["currentUserLeaderboardGlobalThisMonth"]       = leaderboardListParser;
      parsers["currentUserLeaderboardGlobalAllTime"]         = leaderboardListParser;
      parsers["currentUserLeaderboardLocalThisWeek"]         = leaderboardListParser;
      parsers["currentUserLeaderboardLocalThisMonth"]        = leaderboardListParser;
      parsers["currentUserLeaderboardLocalAllTime"]          = leaderboardListParser;
      parsers["anyGameLeaderboardGlobalThisWeek"]            = leaderboardListParser;
      parsers["anyGameLeaderboardGlobalThisMonth"]           = leaderboardListParser;
      parsers["anyGameLeaderboardGlobalAllTime"]             = leaderboardListParser;
      parsers["anyUserAnyGameLeaderboardGlobalThisWeek"]     = leaderboardListParser;
      parsers["anyUserAnyGameLeaderboardGlobalThisMonth"]    = leaderboardListParser;
      parsers["anyUserAnyGameLeaderboardGlobalAllTime"]      = leaderboardListParser;
      parsers["currentUserAnyGameLeaderboardLocalThisWeek"]  = leaderboardListParser;
      parsers["currentUserAnyGameLeaderboardLocalThisMonth"] = leaderboardListParser;
      parsers["currentUserAnyGameLeaderboardLocalAllTime"]   = leaderboardListParser;
      parsers["currentUserSubscriptionStatus"] = new MNWSXmlGenericItemParser<MNWSCurrUserSubscriptionStatus>();
      parsers["currentUser"] = new MNWSXmlGenericItemParser<MNWSCurrentUserInfo>();
      parsers["currentGameRoomList"]     = gameRoomListParser;
      parsers["currentGameRoomUserList"] = gameRoomUserListParser;
      parsers["anyUserGameCookies"] = userGameCookiesListParser;
      parsers["systemGameNetStats"] = new MNWSXmlGenericItemParser<MNWSSystemGameNetStats>();
      parsers["getSessionSignedClientToken"] = new MNWSXmlGenericItemParser<MNWSSessionSignedClientToken>();
     }

    private readonly MNSession                    session;
    private Dictionary<string,IMNWSXmlDataParser> parsers;

    private const string WS_URL_PATH = "user_ajax_host.php";
   }

  class MNWSRequest : IMNWSRequest
   {
    public MNWSRequest (MNWSRequestSender.RequestCompletedEventHandler requestCompleted,
                        MNWSRequestSender.RequestErrorEventHandler     requestError,
                        Dictionary<string,string>                      mapping,
                        Dictionary<string,IMNWSXmlDataParser>          parsers)
     {
      downloader            = null;
      this.requestCompleted = requestCompleted;
      this.requestError     = requestError;
      this.nameMapping      = mapping;
      this.parsers          = parsers;
     }

    public void SendRequest (string url, Dictionary<string,string> postQueryParams)
     {
      downloader = new MNURLStringDownloader();
      downloader.LoadUrl(url,postQueryParams,OnDataReady,OnLoadFailed);
     }

    override public void Cancel ()
     {
      if (downloader != null)
       {
        downloader.Cancel();
       }

      CleanUp();
     }

    private void OnDataReady (string data)
     {
      if (requestCompleted != null)
       {
        HandleXmlResponse(data);
       }

      CleanUp();
     }

    private void OnLoadFailed (int httpStatus, string message)
     {
      if (requestError != null)
       {
        requestError(new MNWSRequestError
                          (MNWSRequestError.TRANSPORT_ERROR,message));
       }

      CleanUp();
     }

    private void CleanUp ()
     {
      downloader       = null;
      nameMapping      = null;
      requestCompleted = null;
      requestError     = null;
     }

    private void HandleXmlResponse (string xmlString)
     {
      XmlReaderSettings readerSettings = new XmlReaderSettings();

      readerSettings.DtdProcessing = DtdProcessing.Ignore;

      bool ok             = true;
      string errorMessage = null;
      int    errorDomain  = MNWSRequestError.PARSE_ERROR;

      using (XmlReader reader = XmlReader.Create(new StringReader(xmlString),readerSettings))
       {
        reader.Read();
        reader.MoveToContent();

        if (reader.NodeType == XmlNodeType.Element && reader.Name == "responseData")
         {
          reader.Read();
          reader.MoveToContent();
         }
        else
         {
          ok = false;
          errorMessage = "invalid document element";
         }

        if (ok)
         {
          if (reader.NodeType == XmlNodeType.Element && reader.Name == "errorMessage")
           {
            ok           = false;
            errorMessage = reader.ReadInnerXml().Trim();
            errorDomain  = MNWSRequestError.SERVER_ERROR;
           }
          else
           {
            MNWSResponse response = new MNWSResponse();

            while (reader.NodeType == XmlNodeType.Element)
             {
              string tagName    = reader.Name;
              string parserName = null;
             
              if (nameMapping != null)
               {
                try
                 {
                  parserName = nameMapping[tagName];
                 }
                catch (KeyNotFoundException)
                 {
                 }
               }

              if (parserName == null)
               {
                parserName = tagName;
               }

              IMNWSXmlDataParser parser = null;

              if (parsers.TryGetValue(parserName, out parser))
               {
                response.AddBlock(tagName,parser.ParseElement(reader));
               }
              else
               {
                response.AddBlock(tagName,reader.ReadInnerXml().Trim());
               }

              reader.Read();
              reader.MoveToContent();
             }

            requestCompleted(response);
           }
         }

        if (!ok && requestError != null)
         {
          requestError(new MNWSRequestError(errorDomain,errorMessage));
         }
       }
     }

    private MNURLStringDownloader                          downloader;
    private MNWSRequestSender.RequestCompletedEventHandler requestCompleted;
    private MNWSRequestSender.RequestErrorEventHandler     requestError;
    private Dictionary<string,string>                      nameMapping;
    private Dictionary<string,IMNWSXmlDataParser>          parsers;
   }
 }
