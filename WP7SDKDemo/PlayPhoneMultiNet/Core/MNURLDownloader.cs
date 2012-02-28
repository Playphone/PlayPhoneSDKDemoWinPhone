//
//  MNURLDownloader.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Net;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public abstract class MNURLDownloader
   {
    public delegate void OnLoadFailed (int httpStatus, string message);

    public MNURLDownloader ()
     {
      webClient    = null;
      onLoadFailed = null;
     }

    public void LoadUrl (string url, Dictionary<string,string> postQueryParams, OnLoadFailed onLoadFailed)
     {
      //FIXME: have to send POST query here

      LoadUrl(new Uri(url + "?" + MNUtils.HttpGetRequestBuildParamsString(postQueryParams)),onLoadFailed);
     }

    public void LoadUrl (Uri url, OnLoadFailed onLoadFailed)
     {
      if (IsLoading())
       {
        onLoadFailed(HttpStatusSystemError,"download in progress");

        return;
       }

      this.onLoadFailed = onLoadFailed;
      webClient         = new WebClient();

      webClient.DownloadStringCompleted += OnDownloadStringCompleted;
      webClient.DownloadStringAsync(url);
     }

    public void Cancel ()
     {
      if (webClient != null)
       {
        webClient.CancelAsync();
        webClient = null;
       }
     }

    private void OnDownloadStringCompleted (object sender, DownloadStringCompletedEventArgs eventArgs)
     {
      webClient = null;

      if (eventArgs.Cancelled)
       {
        return;
       }

      if (eventArgs.Error == null)
       {
        OnDownloadStringSucceeded(eventArgs.Result);
       }
      else
       {
        //FIXME: http status is not filled
        onLoadFailed(HttpStatusSystemError,eventArgs.Error.Message);
       }
     }

    protected abstract void OnDownloadStringSucceeded (string data);

    private bool IsLoading ()
     {
      return webClient != null;
     }

    private WebClient    webClient;
    private OnLoadFailed onLoadFailed;

    private const int HttpStatusSystemError = -1;
   }
 }
