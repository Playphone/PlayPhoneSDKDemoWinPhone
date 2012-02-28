//
//  MNAchievementsProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using PlayPhone.MultiNet;
using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNAchievementsProvider
   {
    public const uint ACHIEVEMENT_IS_SECRET_MASK = 0x0001;

    public delegate void GameAchievementListUpdatedEventHandler ();
    public delegate void PlayerAchievementUnlockedEventHandler  (int achievementId);

    public event GameAchievementListUpdatedEventHandler GameAchievementListUpdated;
    public event PlayerAchievementUnlockedEventHandler  PlayerAchievementUnlocked;

    public class GameAchievementInfo
     {
      public int    Id          { get; set; }
      public string Name        { get; set; }
      public uint   Flags       { get; set; }
      public string Description { get; set; }
      public string Params      { get; set; }
      public int    Points      { get; set; }

      public GameAchievementInfo (int id, string name, uint flags, string description, string _params, int points)
       {
        Id          = id;
        Name        = name;
        Flags       = flags;
        Description = description;
        Params      = _params;
        Points      = points;
       }
     }

    public class PlayerAchievementInfo
     {
      public int Id { get; set; }

      public PlayerAchievementInfo (int id)
       {
        Id = id;
       }
     }

    public MNAchievementsProvider (MNSession session)
     {
      this.session         = session;
      unlockedAchievements = new List<PlayerAchievementInfo>();

      FillUnlockedAchievementsArray(session);

      session.PluginMessageReceived += OnSessionPluginMessageReceived;
      session.UserChanged           += OnSessionUserChanged;

      session.GetGameVocabulary().DownloadFinished += OnVocabularyDownloadFinished;
     }

    public void Shutdown ()
     {
      session.GetGameVocabulary().DownloadFinished -= OnVocabularyDownloadFinished;

      session.PluginMessageReceived -= OnSessionPluginMessageReceived;
      session.UserChanged           -= OnSessionUserChanged;

      unlockedAchievements.Clear();
     }

    private List<GameAchievementInfo> GetGameAchievementsListLow ()
     {
      bool ok = true;
      List<GameAchievementInfo> achievements = new List<GameAchievementInfo>();
      byte[] fileData   = session.GetGameVocabulary().GetFileData(DATA_FILE_NAME);

      if (fileData == null)
       {
        return achievements;
       }

      try
       {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        MemoryStream      sourceStream   = new MemoryStream(fileData);

        readerSettings.DtdProcessing = DtdProcessing.Ignore;

        using (XmlReader reader = XmlReader.Create(sourceStream,readerSettings))
         {
          if (!MNXmlTools.SeekElementByPath(reader,AchievementsEntriesXmlPath))
           {
            ok = false;

            LogWarning("cannot find \"achievements\" element in document");
           }

          if (ok)
           {
            List<Dictionary<string,string>> items = MNXmlTools.ParseItemList(reader,"entry");

            foreach (var itemData in items)
             {
              int? id = MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"id"));

              if (id != null)
               {
                string name    = MNUtils.DictReadValue(itemData,"name") ?? "";
                uint   flags   = MNUtils.ParseUInt(MNUtils.DictReadValue(itemData,"flags")) ?? 0;
                string desc    = MNUtils.DictReadValue(itemData,"desc") ?? "";
                string _params = MNUtils.DictReadValue(itemData,"params") ?? "";
                int    points  = MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"points")) ?? 0;

                achievements.Add(new GameAchievementInfo(id.Value,name,flags,desc,_params,points));
               }
              else
               {
                LogWarning("game achievement data with invalid or absent achievement id ignored");
               }
             }
           }
         }
       }
      catch (Exception e)
       {
        ok = false;

        LogWarning("game achievement data parsing failed (" + e.ToString() + ")");
       }

      if (!ok)
       {
        achievements.Clear();
       }

      return achievements;
     }

    public GameAchievementInfo[] GetGameAchievementsList ()
     {
      List<GameAchievementInfo> achievements = GetGameAchievementsListLow();

      return achievements.ToArray();
     }

    public GameAchievementInfo FindGameAchievementById (int id)
     {
      return GetGameAchievementsListLow().First(value => value.Id == id);
     }

    public bool IsGameAchievementListNeedUpdate ()
     {
      return session.GetGameVocabulary().VocabularyStatus > 0;
     }

    public void DoGameAchievementListUpdate ()
     {
      if (session.GetGameVocabulary().VocabularyStatus !=
           MNGameVocabulary.MN_GV_UPDATE_STATUS_DOWNLOAD_IN_PROGRESS)
       {
        session.GetGameVocabulary().StartDownload();
       }
     }

    public bool IsPlayerAchievementUnlocked (int id)
     {
      return unlockedAchievements.Any(value => value.Id == id);
     }

    private bool AddUniqueAchievement (int id)
     {
      if (!IsPlayerAchievementUnlocked(id))
       {
        unlockedAchievements.Add(new PlayerAchievementInfo(id));

        return true;
       }
      else
       {
        return false;
       }
     }

    public void UnlockPlayerAchievement (int id)
     {
      if (session.IsUserLoggedIn())
       {
        AddUniqueAchievement(id);

        if (session.IsOnline())
         {
          session.SendPluginMessage(PROVIDER_NAME,"A" + id.ToString());
         }
        else
         {
          session.VarStorageSetValue
           (GetOfflineUnlockedAchievementVarName(session.GetMyUserId(),id),
            MNUtils.GetUnixTime().ToString());

          DispatchPlayerAchievementUnlockedEvent(id);
         }
       }
     }

    public string GetAchievementImageURL (int id)
     {
      string webServerUrl = session.GetWebServerURL();

      if (webServerUrl != null)
       {
        StringBuilder builder = new StringBuilder(webServerUrl);

        builder.Append("/data_game_achievement_image.php?game_id=");
        builder.Append(session.GetGameId().ToString());
        builder.Append("&game_achievement_id=");
        builder.Append(id.ToString());

        return builder.ToString();
       }
      else
       {
        return null;
       }
     }

    public PlayerAchievementInfo[] GetPlayerAchievementsList ()
     {
      return unlockedAchievements.ToArray();
     }

    private static List<PlayerAchievementInfo> ParsePlayerAchievementsListMessage (string message)
     {
      string[] achievementsInfoArray = message.Split('\n');
      int      index  = 0;
      int      count  = achievementsInfoArray.Length;
      bool     ok = true;

      List<PlayerAchievementInfo> result = new List<PlayerAchievementInfo>();

      while (index < count && ok)
       {
        string info = achievementsInfoArray[index];
        int    id;

        if (info.Length > 0)
         {
          ok = int.TryParse(MessageGetFirstField(info),out id);

          if (ok)
           {
            result.Add(new PlayerAchievementInfo(id));
           }
         }

        index++;
       }

      return ok ? result : null;
     }

    private void ProcessUserAddAchievementMessage (string message)
     {
      int id;

      if (int.TryParse(MessageGetFirstField(message),out id))
       {
        if (AddUniqueAchievement(id))
         {
          session.VarStorageSetValue
           (GetServerUnlockedAchievementsVarName(session.GetMyUserId()),
            GetCommaSeparatedAchievemensList(unlockedAchievements));

          DispatchPlayerAchievementUnlockedEvent(id);
         }
       }
     }

    private void OnSessionPluginMessageReceived (string     pluginName,
                                                 string     message,
                                                 MNUserInfo sender)
     {
      if (sender != null || pluginName != PROVIDER_NAME)
       {
        return;
       }

      if (message.Length == 0)
       {
        return;
       }

      char   cmd  = message[0];
      String data = message.Substring(MESSAGE_CMD_PREFIX_LEN);

      if      (cmd == 'g')
       {
        // ignore this message, it was used before 1.4.0 to get server
        // data version
       }
      else if (cmd == 'p')
       {
        List<PlayerAchievementInfo> newPlayerAchievements =
         ParsePlayerAchievementsListMessage(data);

        if (newPlayerAchievements != null)
         {
          foreach (var achievement in newPlayerAchievements)
           {
            AddUniqueAchievement(achievement.Id);
           }

          session.VarStorageSetValue
           (GetServerUnlockedAchievementsVarName(session.GetMyUserId()),
            GetCommaSeparatedAchievemensList(unlockedAchievements));
         }
       }
      else if (cmd == 'a')
       {
        ProcessUserAddAchievementMessage(data);
       }
     }

    private void OnSessionUserChanged (long userId)
     {
      if (userId == MNConst.MN_USER_ID_UNDEFINED)
       {
        unlockedAchievements.Clear();
       }
      else
       {
        FillUnlockedAchievementsArray(session);
       }
     }

    private void OnVocabularyDownloadFinished (int downloadStatus)
     {
      if (downloadStatus >= 0)
       {
        DispatchGameAchievementsUpdatedEvent();
       }
     }

    private static string MessageGetFirstField (string message)
     {
      int separatorIndex = message.IndexOf(MESSAGE_FIELD_SEPARATOR);

      if (separatorIndex < 0)
       {
        return message;
       }
      else
       {
        return message.Substring(0,separatorIndex);
       }
     }

    private static string GetServerUnlockedAchievementsVarName (long userId)
     {
      return "offline." + userId.ToString() + ".achievement_saved_list";
     }

    private static string GetOfflineUnlockedAchievementVarName (long userId, int achievementId)
     {
      return "offline." + userId.ToString() + ".achievement_pending." +
              achievementId.ToString() + ".date";
     }

    private static string GetCommaSeparatedAchievemensList (List<PlayerAchievementInfo> achievements)
     {
      MNStringJoiner joiner = new MNStringJoiner(",");

      int count = achievements.Count;

      for (int index = 0; index < count; index++)
       {
        joiner.Join(achievements[index].Id.ToString());
       }

      return joiner.ToString();
     }

    private void FillUnlockedAchievementsArray (MNSession session)
     {
      unlockedAchievements.Clear();

      long userId = session.GetMyUserId();

      if (userId == MNConst.MN_USER_ID_UNDEFINED)
       {
        return;
       }

      /* load achievements confirmed by server */

      string   serverAchievementsList = session.VarStorageGetValueForVariable
                                         (GetServerUnlockedAchievementsVarName(userId));
      string[] serverAchievements = serverAchievementsList == null ?
                                     null :
                                     serverAchievementsList.Split(',');

      if (serverAchievements != null)
       {
        for (int index = 0; index < serverAchievements.Length; index++)
         {
          int id;

          if (int.TryParse(serverAchievements[index],out id))
           {
            AddUniqueAchievement(id);
           }
         }
       }

      /* load achievements unlocked in offline mode */

      string[] masks = { "offline." + userId.ToString() + ".achievement.pending.*" };
      Dictionary<string,string> offlineAchievements = session.VarStorageGetValuesByMasks(masks);
      string[] varNameComponents;

      foreach (var varData in offlineAchievements)
       {
        varNameComponents = varData.Key.Split(',');

        if (varNameComponents.Length > 3)
         {
          int id;

          if (int.TryParse(varNameComponents[3],out id))
           {
            AddUniqueAchievement(id);
           }
         }
       }
     }

    private void DispatchGameAchievementsUpdatedEvent ()
     {
      GameAchievementListUpdatedEventHandler handler = GameAchievementListUpdated;

      if (handler != null)
       {
        handler();
       }
     }

    private void DispatchPlayerAchievementUnlockedEvent (int id)
     {
      PlayerAchievementUnlockedEventHandler handler = PlayerAchievementUnlocked;

      if (handler != null)
       {
        handler(id);
       }
     }

    private static void LogWarning (string message)
     {
      MNDebug.warning("MNAchievementsProvider: " + message);
     }

    private MNSession                   session;
    private List<PlayerAchievementInfo> unlockedAchievements;

    private const string   PROVIDER_NAME = "com.playphone.mn.at";
    private const int      MESSAGE_CMD_PREFIX_LEN   = 1;
    private const char     MESSAGE_FIELD_SEPARATOR = '\t';
    private const string   DATA_FILE_NAME = "MNAchievementsProvider.xml";
    private static readonly string[] AchievementsEntriesXmlPath = { "GameVocabulary", "MNAchievementsProvider", "Achievements" };
   }
 }
