//
//  MNURLFileDownloader.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Net;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public class MNURLFileDownloader
   {
    public delegate void OnLoadSucceeded ();
    public delegate void OnLoadFailed    (int httpStatus, string message);

    public MNURLFileDownloader ()
     {
      webClient       = null;
      onLoadSucceeded = null;
      onLoadFailed    = null;
     }

    public void LoadUrl (string fileName, string url, Dictionary<string,string> postQueryParams,
                         OnLoadSucceeded onLoadSucceeded, OnLoadFailed onLoadFailed)
     {
      //FIXME: have to send POST query here

      LoadUrl(fileName,new Uri(url + "?" + MNUtils.HttpGetRequestBuildParamsString(postQueryParams)),onLoadSucceeded,onLoadFailed);
     }

    public void LoadUrl (string fileName, Uri url,
                         OnLoadSucceeded onLoadSucceeded, OnLoadFailed onLoadFailed)
     {
      if (IsLoading())
       {
        onLoadFailed(HttpStatusSystemError,"download in progress");

        return;
       }

      this.fileName        = fileName;
      this.onLoadSucceeded = onLoadSucceeded;
      this.onLoadFailed    = onLoadFailed;
      webClient            = new WebClient();

      webClient.OpenReadCompleted += OnOpenReadCompleted;
      webClient.OpenReadAsync(url);
     }

    public void Cancel ()
     {
      if (webClient != null)
       {
        webClient.CancelAsync();

        webClient = null;
       }
     }

    private void OnOpenReadCompleted (object sender, OpenReadCompletedEventArgs eventArgs)
     {
      webClient = null;

      if (eventArgs.Cancelled)
       {
        return;
       }

      if (eventArgs.Error == null)
       {
        try
         {
          MNPlatformWinPhone.CopyStreamToFile(fileName,eventArgs.Result);

          onLoadSucceeded();
         }
        catch (Exception e)
         {
          //FIXME: http status is not filled
          onLoadFailed(HttpStatusSystemError,e.ToString());
         }
       }
      else
       {
        //FIXME: http status is not filled
        onLoadFailed(HttpStatusSystemError,eventArgs.Error.Message);
       }
     }

    private bool IsLoading ()
     {
      return webClient != null;
     }

    private string          fileName;
    private WebClient       webClient;
    private OnLoadSucceeded onLoadSucceeded;
    private OnLoadFailed    onLoadFailed;

    private const int HttpStatusSystemError = -1;
   }
 }
