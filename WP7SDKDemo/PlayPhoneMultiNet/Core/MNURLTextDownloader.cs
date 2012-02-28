//
//  MNURLTextDownloader.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public class MNURLTextDownloader : MNURLDownloader
   {
    public delegate void OnDataReady  (string[] text);

    public void LoadUrl (string                    url,
                         Dictionary<string,string> postQueryParams,
                         OnDataReady               onDataReady,
                         OnLoadFailed              onLoadFailed)
     {
      this.onDataReady = onDataReady;

      LoadUrl(url,postQueryParams,onLoadFailed);
     }


    public void LoadUrl (Uri url, OnDataReady onDataReady, OnLoadFailed onLoadFailed)
     {
      this.onDataReady = onDataReady;

      LoadUrl(url,onLoadFailed);
     }

    override protected void OnDownloadStringSucceeded (string data)
     {
      onDataReady(data.Split(LnCharArray));
     }

    private OnDataReady onDataReady;
    private static readonly char[] LnCharArray = new char[] { '\n' };
   }
 }

