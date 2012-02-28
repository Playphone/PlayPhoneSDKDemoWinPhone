//
//  MNUIWebViewHttpReqQueue.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  class MNUIWebViewHttpReqQueue
   {
    public delegate void HttpReqSucceededEventHandler (string jsCode, int flags);
    public delegate void HttpReqFailedEventHandler    (string jsCode, int flags);
    
    public event HttpReqSucceededEventHandler HttpReqSucceeded;
    public event HttpReqFailedEventHandler    HttpReqFailed;

    public MNUIWebViewHttpReqQueue ()
     {
      requests = new List<Request>();
     }

    public void Shutdown ()
     {
      foreach (var request in requests)
       {
        request.Cancel();
       }

      requests.Clear();
     }

    public void AddRequest (string url, string postBody, string successJSCode, string failJSCode, int flags)
     {
      Request request = new Request(this,successJSCode,failJSCode,flags);

      requests.Add(request);

      request.Load(url,postBody);
     }

    private void HandleDataReady (Request request, string jsCode, int flags)
     {
      HttpReqSucceeded(jsCode,flags);

      requests.Remove(request);
     }

    private void HandleLoadFailed (Request request, string jsCode, int flags)
     {
      HttpReqFailed(jsCode,flags);

      requests.Remove(request);
     }

    private class Request
     {
      public Request (MNUIWebViewHttpReqQueue owner, string successJSCode, string failJSCode, int flags)
       {
        downloader = new MNURLStringDownloader();

        this.owner         = owner;
        this.successJSCode = successJSCode;
        this.failJSCode    = failJSCode;
        this.flags         = flags;
       }

      public void Load (string url, string postBody)
       {
        //FIXME: switch to POST request
        Uri uri = new Uri(url + '?' + postBody);

        downloader.LoadUrl(uri,OnDataReady,OnLoadFailed);
       }

      public void Cancel ()
       {
        downloader.Cancel();
       }

      private void OnDataReady (string data)
       {
        string jsCode = successJSCode.Replace("RESPONSE_TEXT",MNUtils.StringAsJSString(data));

        owner.HandleDataReady(this,jsCode,flags);
       }

      public void OnLoadFailed (int httpStatus, string message)
       {
        string jsCode = failJSCode.Replace("RESPONSE_ERROR_CODE",MNUtils.StringAsJSString(httpStatus.ToString())).
                         Replace("RESPONSE_ERROR_TEXT",MNUtils.StringAsJSString(message));

        owner.HandleLoadFailed(this,jsCode,flags);
       }

      private MNUIWebViewHttpReqQueue owner;
      private MNURLStringDownloader   downloader;
      private string                  successJSCode;
      private string                  failJSCode;
      private int                     flags;
     }

    private List<Request> requests;
   }
 }
