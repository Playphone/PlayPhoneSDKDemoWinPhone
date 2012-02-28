//
//  MNConfigData.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  class MNConfigData
   {
    public delegate void OnConfigDataLoaded     (MNConfigData configData);
    public delegate void OnConfigDataLoadFailed (string       errorMessage);

    public MNConfigData (Uri configUri)
     {
      this.configUri  = configUri;
      this.downloader = new MNURLTextDownloader();
     }

    public bool IsLoaded()
     {
      return loaded;
     }

    public void Clear()
     {
      loaded = false;

      smartFoxAddr          = null;
      smartFoxPort          = 0;
      blueBoxAddr           = null;
      blueBoxPort           = 0;
      smartConnect          = false;
      webServerUrl          = null;
      facebookAPIKey        = null;
      facebookAppId         = null;
      facebookSSOMode       = 0;
      launchTrackerUrl      = null;
      shutdownTrackerUrl    = null;
      beaconTrackerUrl      = null;
      gameVocabularyVersion = null;
      tryFastResumeMode     = 0;
     }

    public void Load (OnConfigDataLoaded onConfigDataLoaded, OnConfigDataLoadFailed onConfigDataLoadFailed)
     {
      Clear();

      this.onConfigDataLoaded     = onConfigDataLoaded;
      this.onConfigDataLoadFailed = onConfigDataLoadFailed;

      if (configUri != null)
       {
        downloader.LoadUrl(configUri,OnDownloaderDataReady,OnDownloaderLoadFailed);
       }
      else
       {
        onConfigDataLoadFailed("Configuration URL is not set");
       }
     }

    public void OnDownloaderDataReady(string[] text)
     {
      bool ok = false;
      Dictionary<string,string> parameters = ParseConfig(text);

      if (parameters != null)
       {
        try
         {
          smartFoxAddr          = parameters["SmartFoxServerAddr"];
          smartFoxPort          = int.Parse(parameters["SmartFoxServerPort"]);
          blueBoxAddr           = parameters["BlueBoxServerAddr"];
          blueBoxPort           = int.Parse(parameters["BlueBoxServerPort"]);
          smartConnect          = bool.Parse(parameters["BlueBoxSmartConnect"]);
          webServerUrl          = parameters["MultiNetWebServerURL"];
          facebookAPIKey        = parameters["FacebookApiKey"];
          facebookAppId         = parameters["FacebookAppId"];
          facebookSSOMode       = DictGetIntOpt(parameters,"FacebookSSOMode");
          launchTrackerUrl      = DictGetStringOpt(parameters,"LaunchTrackerURL");
          beaconTrackerUrl      = DictGetStringOpt(parameters,"BeaconTrackerURL");
          shutdownTrackerUrl    = DictGetStringOpt(parameters,"ShutdownTrackerURL");
          gameVocabularyVersion = DictGetStringOpt(parameters,"GameVocabularyVersion");
          tryFastResumeMode     = DictGetIntOpt(parameters,"TryFastResumeMode");

          ok = true;
         }
        catch (KeyNotFoundException)
         {
         }
        catch (FormatException)
         {
         }
        catch (OverflowException)
         {
         }
       }

      if (ok)
       {
        loaded = true;

        onConfigDataLoaded(this);
       }
      else
       {
        Clear();

        onConfigDataLoadFailed("Invalid configuration file format");
       }

      onConfigDataLoaded     = null;
      onConfigDataLoadFailed = null;
     }

    public void OnDownloaderLoadFailed(int httpStatus, string message)
     {
      onConfigDataLoadFailed(message);

      onConfigDataLoaded     = null;
      onConfigDataLoadFailed = null;
     }

    private static int DictGetIntOpt(Dictionary<string, string> dict, string key)
     {
      string data = DictGetStringOpt(dict,key);

      if (data != null)
       {
        try
         {
          return int.Parse(data);
         }
        catch (OverflowException)
         {
         }
        catch (FormatException)
         {
         }
       }

      return 0;
     }

    private static string DictGetStringOpt(Dictionary<string, string> dict, string key)
     {
      string data;

      if (dict.TryGetValue(key,out data))
       {
        return data;
       }
      else
       {
        return null;
       }
     }

    private static Dictionary<string,string> ParseConfig (string[] text)
     {
      Dictionary<string,string> result = new Dictionary<string,string>();
      int  index = 0;
      int  count = text.Length;
      bool ok    = true;

      while (index < count && ok)
       {
        string line = text[index].Trim();

        if (line.Length > 0)
         {
          int pos = line.IndexOf('=');

          if (pos >= 0)
           {
            result.Add(line.Substring(0,pos).Trim(),
                       line.Substring(pos + 1).Trim());
           }
          else
           {
            ok = false;
           }
         }

        index++;
       }

      return ok ? result : null;
     }

    public string smartFoxAddr          { get; private set; }
    public int    smartFoxPort          { get; private set; }
    public string blueBoxAddr           { get; private set; }
    public int    blueBoxPort           { get; private set; }
    public bool   smartConnect          { get; private set; }
    public string webServerUrl          { get; private set; }
    public string facebookAPIKey        { get; private set; }
    public string facebookAppId         { get; private set; }
    public int    facebookSSOMode       { get; private set; }
    public string launchTrackerUrl      { get; private set; }
    public string beaconTrackerUrl      { get; private set; }
    public string shutdownTrackerUrl    { get; private set; }
    public string gameVocabularyVersion { get; private set; }
    public int    tryFastResumeMode     { get; private set; }

    private readonly Uri           configUri;
    private bool                   loaded;
    private OnConfigDataLoaded     onConfigDataLoaded;
    private OnConfigDataLoadFailed onConfigDataLoadFailed;
    private MNURLTextDownloader    downloader;
   }
 }
