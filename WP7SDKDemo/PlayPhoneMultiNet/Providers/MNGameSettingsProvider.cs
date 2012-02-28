//
//  MNGameSettingsProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Linq;
using System.Xml;
using System.IO;
using System.Collections.Generic;

using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNGameSettingsProvider
   {
    public delegate void GameSettingsListUpdatedEventHandler ();

    public event GameSettingsListUpdatedEventHandler GameSettingsListUpdated;

    public class GameSettingInfo
     {
      public int    Id                 { get; set; }
      public string Name               { get; set; }
      public string Params             { get; set; }
      public string SysParams          { get; set; }
      public bool   MultiplayerEnabled { get; set; }
      public bool   LeaderboardVisible { get; set; }

      public GameSettingInfo (int id, string name, string _params, string sysParams,
                              bool multiplayerEnabled, bool leaderboardVisible)
       {
        Id                 = id;
        Name               = name;
        Params             = _params;
        SysParams          = sysParams;
        MultiplayerEnabled = multiplayerEnabled;
        LeaderboardVisible = leaderboardVisible;
       }
     }

    public MNGameSettingsProvider (MNSession session)
     {
      this.session = session;

      session.GetGameVocabulary().DownloadFinished += OnVocabularyDownloadFinished;
     }

    public void Shutdown ()
     {
      session.GetGameVocabulary().DownloadFinished -= OnVocabularyDownloadFinished;
     }

    private List<GameSettingInfo> GetGameSettingListLow ()
     {
      bool ok = true;
      List<GameSettingInfo> gameSettings = new List<GameSettingInfo>();
      byte[] fileData = session.GetGameVocabulary().GetFileData(DATA_FILE_NAME);

      if (fileData == null)
       {
        return gameSettings;
       }

      try
       {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        MemoryStream      sourceStream   = new MemoryStream(fileData);

        readerSettings.DtdProcessing = DtdProcessing.Ignore;

        using (XmlReader reader = XmlReader.Create(sourceStream,readerSettings))
         {
          if (!MNXmlTools.SeekElementByPath(reader,GameSettingsEntriesXmlPath))
           {
            ok = false;

            LogWarning("cannot find \"GameSettings\" element in document");
           }

          if (ok)
           {
            List<Dictionary<string,string>> items = MNXmlTools.ParseItemList(reader,"entry");

            foreach (var itemData in items)
             {
              int? id = MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"id"));

              if (id != null)
               {
                string name      = MNUtils.DictReadValue(itemData,"name") ?? "";
                string _params   = MNUtils.DictReadValue(itemData,"params") ?? "";
                string sysParams = MNUtils.DictReadValue(itemData,"sysParams") ?? "";
                bool   multiplayerEnabled = parseBool(MNUtils.DictReadValue(itemData,"isMultiplayerEnabled"));
                bool   leaderboardVisible = parseBool(MNUtils.DictReadValue(itemData,"isLeaderboardVisible"));

                gameSettings.Add(new GameSettingInfo(id.Value,name,_params,sysParams,multiplayerEnabled,leaderboardVisible));
               }
              else
               {
                LogWarning("game settings data with invalid or absent game setting id ignored");
               }
             }
           }
         }
       }
      catch (Exception e)
       {
        ok = false;

        LogWarning("game settings data parsing failed (" + e.ToString() + ")");
       }

      if (!ok)
       {
        gameSettings.Clear();
       }

      return gameSettings;
     }

    public GameSettingInfo[] GetGameSettingsList ()
     {
      List<GameSettingInfo> gameSettings = GetGameSettingListLow();

      return gameSettings.ToArray();
     }

    public GameSettingInfo FindGameSettingById (int id)
     {
      return GetGameSettingListLow().First(value => value.Id == id);
     }

    public bool IsGameSettingListNeedUpdate ()
     {
      return session.GetGameVocabulary().VocabularyStatus > 0;
     }

    public void DoGameSettingListUpdate ()
     {
      if (session.GetGameVocabulary().VocabularyStatus !=
           MNGameVocabulary.MN_GV_UPDATE_STATUS_DOWNLOAD_IN_PROGRESS)
       {
        session.GetGameVocabulary().StartDownload();
       }
     }

    private void OnVocabularyDownloadFinished (int downloadStatus)
     {
      if (downloadStatus >= 0)
       {
        DispatchGameSettingsUpdatedEvent();
       }
     }

    private void DispatchGameSettingsUpdatedEvent ()
     {
      GameSettingsListUpdatedEventHandler handler = GameSettingsListUpdated;

      if (handler != null)
       {
        handler();
       }
     }

    private static bool parseBool (string s)
     {
      if (s == null)
       {
        return false;
       }

      return s == "true";
     }

    private static void LogWarning (string message)
     {
      MNDebug.warning("MNGameSettingsProvider: " + message);
     }

    private MNSession session;

    private const string   DATA_FILE_NAME = "MNGameSettingsProvider.xml";
    private static readonly string[] GameSettingsEntriesXmlPath = { "GameVocabulary", "MNGameSettingsProvider", "GameSettings" };
   }
 }
