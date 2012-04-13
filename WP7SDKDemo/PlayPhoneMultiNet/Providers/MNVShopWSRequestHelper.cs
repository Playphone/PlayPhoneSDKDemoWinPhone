//
//  MNVShopWSRequestHelper.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Xml;
using System.Collections.Generic;

using PlayPhone.MultiNet.Core;
using System.IO;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNVShopWSRequestHelper
   {
    public interface IEventHandler
     {
      bool VShopShouldParseResponse        (long   userId);
      void VShopPostVItemTransaction       (long   srvTransactionId,
                                            long   cliTransactionId,
                                            string itemsToAddStr,
                                            bool   vshopTransactionEnabled);
      void VShopPostVShopTransactionFailed (long   cliTransactionId,
                                            int    errorCode,
                                            string errorMessage);
      void VShopFinishTransaction          (string transactionId);
      void VShopWSRequestFailed            (long   cliTransactionId,
                                            int    errorCode,
                                            string errorMessage);
     }

    public MNVShopWSRequestHelper (MNSession session, IEventHandler eventHandler)
     {
      this.session      = session;
      this.eventHandler = eventHandler;

      requests = new HttpRequestSet();
      requests.RequestCompleted += OnRequestCompleted;
     }

    public void Shutdown ()
     {
      CancelAllRequests();
     }

    public void CancelAllRequests ()
     {
      requests.CancelAll();
     }

    public void SendWSRequest (string url, Dictionary<string,string> postQueryParams, long cliTransactionId)
     {
      MNSession session = this.session;
      string    userSId = session.GetMySId();

      if (userSId == null)
       {
        eventHandler.VShopWSRequestFailed
         (cliTransactionId,
          MNVShopProvider.CheckoutVShopPackFailInfo.ERROR_CODE_NETWORK_ERROR,
          "user is not logged in");

        return;
       }

      postQueryParams["ctx_game_id"]    = session.GetGameId().ToString();
      postQueryParams["ctx_gameset_id"] = session.GetDefaultGameSetId().ToString();
      postQueryParams["ctx_user_id"]    = session.GetMyUserId().ToString();
      postQueryParams["ctx_user_sid"]   = userSId;
      postQueryParams["ctx_dev_type"]   = MNPlatformWinPhone.GetDeviceType().ToString();
      postQueryParams["ctx_dev_id"]     = MNUtils.StringGetMD5String(session.GetUniqueAppId());
      postQueryParams["ctx_client_ver"] = MNSession.CLIENT_API_VERSION;

      requests.SendRequestAsync(url,postQueryParams,cliTransactionId);
     }

    private Dictionary<string,string> ReadXmlCommandData (XmlReader reader)
     {
      Dictionary<string,string> result = new Dictionary<string,string>();

      MNXmlTools.ReaderReadToContent(reader);

      while (reader.NodeType != XmlNodeType.EndElement)
       {
        if (reader.NodeType == XmlNodeType.Element)
         {
          string key   = reader.Name;
          string value = reader.ReadInnerXml().Trim();

          result[key] = value;
         }

        MNXmlTools.ReaderReadToContent(reader);
       }

      return result;
     }

    private void ProcessFinishTransactionCmd (XmlReader reader)
     {
      Dictionary<string,string> data = ReadXmlCommandData(reader);

      string srcTransactionId = MNUtils.DictReadValue(data,"srcTransactionId");

      if (srcTransactionId != null)
       {
        eventHandler.VShopFinishTransaction(srcTransactionId);
       }
      else
       {
        LogWarning("src transaction id not found in 'postFinishTransaction' command");
       }
     }

    private void ProcessPostVItemTransactionCmd (XmlReader reader)
     {
      Dictionary<string,string> data = ReadXmlCommandData(reader);

      long?  cliTransactionId = MNUtils.ParseLong(MNUtils.DictReadValue(data,"clientTransactionId"));
      long?  srvTransactionId = MNUtils.ParseLong(MNUtils.DictReadValue(data,"serverTransactionId"));
      string itemsToAdd       = MNUtils.DictReadValue(data,"itemsToAdd");
      bool   vShopTransactionEnabled = (MNUtils.DictReadValue(data,"itemsToAdd") ?? "") != "0";

      if (cliTransactionId != null && srvTransactionId != null && itemsToAdd != null)
       {
        eventHandler.VShopPostVItemTransaction
         (srvTransactionId.Value,cliTransactionId.Value,itemsToAdd,vShopTransactionEnabled);
       }
      else
       {
        LogWarning("invalid parameters in 'postVItemTransaction' command");
       }
     }

    private void ProcessPostSysEventCmd (XmlReader reader)
     {
      Dictionary<string,string> data = ReadXmlCommandData(reader);

      string eventName  = MNUtils.DictReadValue(data,"eventName");
      string eventParam = MNUtils.DictReadValue(data,"eventParam");
      string callbackId = MNUtils.DictReadValue(data,"callbackId");

      if (eventName != null)
       {
        session.PostSysEvent(eventName,eventParam ?? "",callbackId);
       }
      else
       {
        LogWarning("event name is absent in 'postSysEvent' command");
       }
     }

    private void ProcessPostPluginMessageCmd (XmlReader reader)
     {
      Dictionary<string,string> data = ReadXmlCommandData(reader);

      string pluginName = MNUtils.DictReadValue(data,"pluginName");
      string message    = MNUtils.DictReadValue(data,"pluginMessage");

      if (pluginName != null)
       {
        session.SendPluginMessage(pluginName,message ?? "");
       }
      else
       {
        LogWarning("plugin name is absent in 'postPluginMessage' command");
       }
     }

    private void ProcessCallVShopTransactionFailCmd (XmlReader reader)
     {
      Dictionary<string,string> data = ReadXmlCommandData(reader);

      int?   errorCode = MNUtils.ParseInt(MNUtils.DictReadValue(data,"errorCode"));
      string errorMessage = MNUtils.DictReadValue(data,"errorMessage");
      long?  cliTransactionId = MNUtils.ParseLong(MNUtils.DictReadValue(data,"clientTransactionId"));

      if (errorCode != null && errorMessage != null && cliTransactionId != null)
       {
        eventHandler.VShopPostVShopTransactionFailed(cliTransactionId.Value,errorCode.Value,errorMessage);
       }
      else
       {
        LogWarning("invalid or absent parameter in 'callVShopTransactionFail' command");
       }
     }

    private void ProcessWSResponse (string responseStr, long cliTransactionId)
     {
      XmlReaderSettings readerSettings = new XmlReaderSettings();

      readerSettings.DtdProcessing = DtdProcessing.Ignore;

      string errorMessage = null;
      int    errorCode    = MNVShopProvider.CheckoutVShopPackFailInfo.ERROR_CODE_UNDEFINED;

      try
       {
        using (XmlReader reader = XmlReader.Create(new StringReader(responseStr),readerSettings))
         {
          MNXmlTools.ReaderReadToContent(reader);

          if (reader.NodeType == XmlNodeType.Element && reader.Name == "responseData")
           {
            MNXmlTools.ReaderReadToContent(reader);

            if      (reader.NodeType == XmlNodeType.Element && reader.Name == "ctxUserId")
             {
              long? userId = MNUtils.ParseLong(reader.ReadInnerXml().Trim());

              if (userId != null)
               {
                if (eventHandler.VShopShouldParseResponse(userId.Value))
                 {
                  MNXmlTools.ReaderReadToContent(reader);

                  while (reader.NodeType != XmlNodeType.EndElement)
                   {
                    if (reader.NodeType == XmlNodeType.Element)
                     {
                      string tagName = reader.Name;

                      if (tagName == "finishTransaction")
                       {
                        ProcessFinishTransactionCmd(reader);
                       }
                      else if (tagName == "postVItemTransaction")
                       {
                        ProcessPostVItemTransactionCmd(reader);
                       }
                      else if (tagName == "postSysEvent")
                       {
                        ProcessPostSysEventCmd(reader);
                       }
                      else if (tagName == "postPluginMessage")
                       {
                        ProcessPostPluginMessageCmd(reader);
                       }
                      else if (tagName == "callVShopTransactionFail")
                       {
                        ProcessCallVShopTransactionFailCmd(reader);
                       }
                      else
                       {
                        LogWarning("invalid command in purchase ws request");
                       }
                     }

                    MNXmlTools.ReaderReadToContent(reader);
                   }
                 }
               }
              else
               {
                errorMessage = "response contains invalid user id value";
                errorCode    = MNVShopProvider.CheckoutVShopPackFailInfo.ERROR_CODE_XML_STRUCTURE_ERROR;
               }
             }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "errorMessage")
             {
              errorCode    = MNUtils.ParseInt(reader.GetAttribute("code")) ?? WS_FAILED_ERROR_CODE;
              errorMessage = reader.ReadInnerXml().Trim();
             }
            else
             {
              errorMessage = "response contains neither 'ctxUserId' nor 'errorMessage' element";
              errorCode    = MNVShopProvider.CheckoutVShopPackFailInfo.ERROR_CODE_XML_STRUCTURE_ERROR;
             }
           }
          else
           {
            errorMessage = "invalid document element in response";
            errorCode    = MNVShopProvider.CheckoutVShopPackFailInfo.ERROR_CODE_XML_STRUCTURE_ERROR;
           }
         }
       }
      catch (Exception e)
       {
        errorMessage = e.ToString();
        errorCode    = MNVShopProvider.CheckoutVShopPackFailInfo.ERROR_CODE_XML_PARSE_ERROR;
       }

      if (errorMessage != null)
       {
        eventHandler.VShopWSRequestFailed(cliTransactionId,errorCode,errorMessage);
       }
     }

    private void OnRequestCompleted (object sender, HttpRequestSet.RequestCompletedEventArgs args)
     {
      long cliTransactionId = (args.UserState as long?) ?? MNVItemsProvider.TRANSACTION_ID_UNDEFINED;

      if (args.Succeeded)
       {
        ProcessWSResponse(args.Data,cliTransactionId);
       }
      else
       {
        eventHandler.VShopWSRequestFailed
         (cliTransactionId,MNVShopProvider.CheckoutVShopPackFailInfo.ERROR_CODE_NETWORK_ERROR,args.Error.Message);
       }
     }

    private class HttpRequestSet
     {
      public class RequestCompletedEventArgs
       {
        public class ErrorInfo
         {
          public int    HttpStatus { get; private set; }
          public string Message    { get; private set; }

          public ErrorInfo (int httpStatus, string message)
           {
            HttpStatus = httpStatus;
            Message    = message;
           }
         }

        public object    UserState { get; private set; }
        public string    Data      { get; private set; }
        public ErrorInfo Error     { get; private set; }

        public bool Succeeded
         {
          get
           {
            return Error == null;
           }
         }

        public RequestCompletedEventArgs (string data, object userState)
         {
          UserState = userState;
          Data      = data;
          Error     = null;
         }

        public RequestCompletedEventArgs (ErrorInfo error, object userState)
         {
          UserState = userState;
          Data      = null;
          Error     = error;
         }
       }

      public delegate void RequestCompletedEventHandler (object sender, RequestCompletedEventArgs args);
      public event         RequestCompletedEventHandler RequestCompleted;

      public HttpRequestSet ()
       {
        requests = new List<RequestInfo>();
       }

      public void SendRequestAsync (string url, Dictionary<string,string> postQueryParams, object userState)
       {
        MNURLStringDownloader downloader = new MNURLStringDownloader();

        lock (requestsLock)
         {
          requests.Add(new RequestInfo(downloader,userState));
         }

        downloader.LoadUrl
         (url,
          postQueryParams,
          (string data) =>
           {
            OnDownloaderDataReady(downloader,data);
           },
          (int httpStatus, string errorMessage) =>
           {
            OnDownloaderLoadFailed(downloader,httpStatus,errorMessage);
           });
       }

      public void CancelAll ()
       {
        lock (requestsLock)
         {
          while (requests.Count > 0)
           {
            RequestInfo request = requests[requests.Count - 1];

            requests.RemoveAt(requests.Count - 1);

            request.Downloader.Cancel();
           }
         }
       }

      private int FindRequest (MNURLStringDownloader downloader)
       {
        for (int i = 0; i < requests.Count; i++)
         {
          if (requests[i].Downloader == downloader)
           {
            return i;
           }
         }

        return -1;
       }

      private void OnDownloaderDataReady (MNURLStringDownloader downloader, string data)
       {
        lock (requestsLock)
         {
          int    reqIndex  = FindRequest(downloader);
          object userState = reqIndex < 0 ? null : requests[reqIndex].UserState;

          DispatchRequestCompletedEvent(new RequestCompletedEventArgs(data,userState));

          if (reqIndex >= 0)
           {
            requests.RemoveAt(reqIndex);
           }
         }
       }

      private void OnDownloaderLoadFailed (MNURLStringDownloader downloader, int httpStatus, string message)
       {
        lock (requestsLock)
         {
          int    reqIndex  = FindRequest(downloader);
          object userState = reqIndex < 0 ? null : requests[reqIndex].UserState;

          DispatchRequestCompletedEvent
           (new RequestCompletedEventArgs
                 (new RequestCompletedEventArgs.ErrorInfo(httpStatus,message),userState));

          if (reqIndex >= 0)
           {
            requests.RemoveAt(reqIndex);
           }
         }
       }

      private void DispatchRequestCompletedEvent (RequestCompletedEventArgs args)
       {
        RequestCompletedEventHandler handler = RequestCompleted;

        if (handler != null)
         {
          handler(this,args);
         }
       }

      private class RequestInfo
       {
        public MNURLStringDownloader Downloader { get; private set; }
        public object                UserState  { get; private set; }

        public RequestInfo (MNURLStringDownloader downloader,
                            object                userState)
         {
          Downloader = downloader;
          UserState  = userState;
         }
       }

      private List<RequestInfo> requests;
      private object            requestsLock = new object();
     }

    private void LogWarning (string message)
     {
      MNDebug.warning("MNVShopWSRequestHelper: " + message);
     }

    private MNSession      session;
    private IEventHandler  eventHandler;
    private HttpRequestSet requests;

    private const int WS_FAILED_ERROR_CODE = 100;
   }
 }
