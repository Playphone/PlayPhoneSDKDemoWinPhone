//
//  MNVItemsProvider.cs
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

using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNVItemsProvider
   {
    public const long TRANSACTION_ID_UNDEFINED = 0;

    public const uint VITEM_IS_CURRENCY_MASK     = 0x0001;
    public const uint VITEM_IS_UNIQUE_MASK       = 0x0002;
    public const uint VITEM_IS_CONSUMABLE_MASK   = 0x0004;
    public const uint VITEM_ISSUE_ON_CLIENT_MASK = 0x0200;

    public delegate void VItemsListUpdatedEventHandler          ();
    public delegate void VItemsTransactionCompletedEventHandler (TransactionInfo  transaction);
    public delegate void VItemsTransactionFailedEventHandler    (TransactionError error);

    public event VItemsListUpdatedEventHandler          VItemsListUpdated;
    public event VItemsTransactionCompletedEventHandler VItemsTransactionCompleted;
    public event VItemsTransactionFailedEventHandler    VItemsTransactionFailed;

    public class GameVItemInfo
     {
      public int    Id          { get; set; }
      public string Name        { get; set; }
      public uint   Model       { get; set; }
      public string Description { get; set; }
      public string Params      { get; set; }

      public GameVItemInfo (int id, string name, uint model, string description, string _params)
       {
        Id          = id;
        Name        = name;
        Model       = model;
        Description = description;
        Params      = _params;
       }
     }

    public class PlayerVItemInfo
     {
      public int  Id    { get; set; }
      public long Count { get; set; }

      public PlayerVItemInfo (int id, long count)
       {
        Id    = id;
        Count = count;
       }
     }

    public class TransactionVItemInfo
     {
      public int  Id    { get; set; }
      public long Delta { get; set; }

      public TransactionVItemInfo (int id, long delta)
       {
        Id    = id;
        Delta = delta;
       }
     }

    public class TransactionInfo
     {
      public long                   ClientTransactionId { get; set; }
      public long                   ServerTransactionId { get; set; }
      public long                   CorrUserId          { get; set; }
      public TransactionVItemInfo[] VItems              { get; set; }

      public TransactionInfo (long clientTransactionId, long serverTransactionId,
                              long corrUserId, TransactionVItemInfo[] vItems)
       {
        ClientTransactionId = clientTransactionId;
        ServerTransactionId = serverTransactionId;
        CorrUserId          = corrUserId;
        VItems              = vItems;
       }
     }

    public class TransactionError
     {
      public long                   ClientTransactionId { get; set; }
      public long                   ServerTransactionId { get; set; }
      public long                   CorrUserId          { get; set; }
      public int                    FailReasonCode      { get; set; }
      public string                 ErrorMessage        { get; set; }

      public TransactionError (long clientTransactionId, long serverTransactionId,
                               long corrUserId, int failReasonCode, string errorMessage)
       {
        ClientTransactionId = clientTransactionId;
        ServerTransactionId = serverTransactionId;
        CorrUserId          = corrUserId;
        FailReasonCode      = failReasonCode;
        ErrorMessage        = errorMessage;
       }
     }

    public MNVItemsProvider (MNSession session)
     {
      this.session = session;

      playerVItemsOwnerId = MNConst.MN_USER_ID_UNDEFINED;
      playerVItems        = new List<PlayerVItemInfo>();

      clientTransactionId = GenerateInitialClientTransactionId();

      session.PluginMessageReceived += OnSessionPluginMessageReceived;
      session.WebEventReceived      += OnSessionWebEventReceived;
      session.UserChanged           += OnSessionUserChanged;

      session.GetGameVocabulary().DownloadFinished += OnVocabularyDownloadFinished;
     }

    public void Shutdown ()
     {
      session.PluginMessageReceived -= OnSessionPluginMessageReceived;
      session.WebEventReceived      -= OnSessionWebEventReceived;
      session.UserChanged           -= OnSessionUserChanged;

      session.GetGameVocabulary().DownloadFinished -= OnVocabularyDownloadFinished;

      playerVItems.Clear();
     }

    private List<GameVItemInfo> GetGameVItemsListLow ()
     {
      bool ok = true;
      List<GameVItemInfo> vItems = new List<GameVItemInfo>();
      byte[] fileData   = session.GetGameVocabulary().GetFileData(DATA_FILE_NAME);

      if (fileData == null)
       {
        return vItems;
       }

      try
       {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        MemoryStream      sourceStream   = new MemoryStream(fileData);

        readerSettings.DtdProcessing = DtdProcessing.Ignore;

        using (XmlReader reader = XmlReader.Create(sourceStream,readerSettings))
         {
          if (!MNXmlTools.SeekElementByPath(reader,GameVItemEntriesXmlPath))
           {
            ok = false;

            LogWarning("cannot find \"VItems\" element in document");
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
                uint   model   = MNUtils.ParseUInt(MNUtils.DictReadValue(itemData,"model")) ?? 0;
                string desc    = MNUtils.DictReadValue(itemData,"desc") ?? "";
                string _params = MNUtils.DictReadValue(itemData,"params") ?? "";

                vItems.Add(new GameVItemInfo(id.Value,name,model,desc,_params));
               }
              else
               {
                LogWarning("game vitem data with invalid or absent vitem id ignored");
               }
             }
           }
         }
       }
      catch (Exception e)
       {
        ok = false;

        LogWarning("game vitems data parsing failed (" + e.ToString() + ")");
       }

      if (!ok)
       {
        vItems.Clear();
       }

      return vItems;
     }

    public GameVItemInfo[] GetGameVItemsList ()
     {
      return GetGameVItemsListLow().ToArray();
     }

    public GameVItemInfo FindGameVItemById (int id)
     {
      return GetGameVItemsListLow().FirstOrDefault(value => value.Id == id);
     }

    public bool IsGameVItemsListNeedUpdate ()
     {
      return session.GetGameVocabulary().VocabularyStatus > 0;
     }

    public void DoGameVItemsListUpdate ()
     {
      if (session.GetGameVocabulary().VocabularyStatus !=
           MNGameVocabulary.MN_GV_UPDATE_STATUS_DOWNLOAD_IN_PROGRESS)
       {
        session.GetGameVocabulary().StartDownload();
       }
     }

    public void ReqAddPlayerVItem (int vItemId, long count, long clientTransactionId)
     {
      if (session.IsUserLoggedIn() && session.IsOnline())
       {
        session.SendPluginMessage
         (PROVIDER_NAME,"A" + clientTransactionId.ToString() + "\n" +
                        vItemId.ToString() + "\t" + count.ToString());
       }
     }

    public void ReqAddPlayerVItemTransaction (TransactionVItemInfo[] transactionVItems, long clientTransactionId)
     {
      if (session.IsUserLoggedIn() && session.IsOnline())
       {
        if (transactionVItems.Length > 0)
         {
          StringBuilder message = new StringBuilder("A" + clientTransactionId.ToString());

          foreach (var vItem in transactionVItems)
           {
            message.Append("\n");
            message.Append(vItem.Id.ToString());
            message.Append("\t");
            message.Append(vItem.Delta.ToString());
           }

          session.SendPluginMessage(PROVIDER_NAME,message.ToString());
         }
       }
     }

    public void ReqTransferPlayerVItem (int  vItemId, long count, long toPlayerId, long clientTransactionId)
     {
      if (session.IsUserLoggedIn() && session.IsOnline())
       {
        session.SendPluginMessage
         (PROVIDER_NAME,"T" + clientTransactionId.ToString() + "\t" + toPlayerId.ToString() + "\n" +
                         vItemId.ToString() + "\t" + count.ToString());
       }
     }

    public void ReqTransferPlayerVItemTransaction
                              (TransactionVItemInfo[] transactionVItems,
                               long                   toPlayerId,
                               long                   clientTransactionId)
     {
      if (session.IsUserLoggedIn() && session.IsOnline())
       {
        if (transactionVItems.Length > 0)
         {
          StringBuilder message = new StringBuilder("T" + clientTransactionId.ToString() + "\t" + toPlayerId.ToString());

          foreach (var vItem in transactionVItems)
           {
            message.Append("\n");
            message.Append(vItem.Id.ToString());
            message.Append("\t");
            message.Append(vItem.Delta.ToString());
           }

          session.SendPluginMessage(PROVIDER_NAME,message.ToString());
         }
       }
     }

    public PlayerVItemInfo[] GetPlayerVItemList ()
     {
      return playerVItems.ToArray();
     }

    public long GetPlayerVItemCountById (int vItemId)
     {
      PlayerVItemInfo vItem = SearchPlayerVItemInfoById(vItemId);

      return vItem != null ? vItem.Count : 0;
     }

    private PlayerVItemInfo SearchPlayerVItemInfoById (int vItemId)
     {
      return playerVItems.FirstOrDefault(value => value.Id == vItemId);
     }

    public string GetVItemImageURL (int id)
     {
      string webServerUrl = session.GetWebServerURL();

      if (webServerUrl != null)
       {
        StringBuilder builder = new StringBuilder(webServerUrl);

        builder.Append("/data_game_item_image.php?game_id=");
        builder.Append(session.GetGameId().ToString());
        builder.Append("&game_item_id=");
        builder.Append(id.ToString());

        return builder.ToString();
       }
      else
       {
        return null;
       }
     }

    private static object ParsePlayerVItemInfoField (string line, bool transactionVItemMode, char fieldSeparator)
     {
      int separatorPos = line.IndexOf(fieldSeparator);

      if (separatorPos <= 0)
       {
        return null;
       }

      int  vItemId;
      long count;

      if (!int.TryParse(line.Substring(0,separatorPos), out vItemId))
       {
        return null;
       }

      if (!long.TryParse(line.Substring(separatorPos + 1), out count))
       {
        return null;
       }

      if (transactionVItemMode)
       {
        return new TransactionVItemInfo(vItemId,count);
       }
      else
       {
        return new PlayerVItemInfo(vItemId,count);
       }
     }

    private static List<PlayerVItemInfo> ParsePlayerVItemsListMessage (string message)
     {
      string[] vItemsInfoArray = message.Split('\n');
      int      index  = 0;
      int      count  = vItemsInfoArray.Length;
      bool     ok = true;

      List<PlayerVItemInfo> result = new List<PlayerVItemInfo>();

      while (index < count && ok)
       {
        PlayerVItemInfo vItemInfo = (PlayerVItemInfo)ParsePlayerVItemInfoField(vItemsInfoArray[index],false,'\t');

        if (vItemInfo != null)
         {
          result.Add(vItemInfo);
         }
        else
         {
          ok = false;
         }

        index++;
       }

      if (!ok)
       {
        result.Clear();
       }

      return result;
     }

    private static List<TransactionVItemInfo> ParseTransactionVItemsListMessage (string message, char vItemsItemsSeparator, char vItemsFieldSeparator)
     {
      string[] vItemsInfoArray = message.Split(vItemsItemsSeparator);
      int      index  = 0;
      int      count  = vItemsInfoArray.Length;
      bool     ok = true;

      List<TransactionVItemInfo> result = new List<TransactionVItemInfo>();

      while (index < count && ok)
       {
        TransactionVItemInfo vItemInfo = (TransactionVItemInfo)ParsePlayerVItemInfoField(vItemsInfoArray[index],true,vItemsFieldSeparator);

        if (vItemInfo != null)
         {
          result.Add(vItemInfo);
         }
        else
         {
          ok = false;
         }

        index++;
       }

      return ok ? result : null;
     }

    private PlayerVItemInfo PlayerVItemInfoById (int vItemId)
     {
      PlayerVItemInfo vItem = SearchPlayerVItemInfoById(vItemId);

      if (vItem == null)
       {
        vItem = new PlayerVItemInfo(vItemId,0);

        playerVItems.Add(vItem);
       }

      return vItem;
     }

    private TransactionInfo ApplyTransaction (long srvTransactionId,
                                              long cliTransactionId,
                                              long corrUserId,
                                              TransactionVItemInfo[] vItems)
     {
      foreach (TransactionVItemInfo transactionVItemInfo in vItems)
       {
        PlayerVItemInfo playerVItemInfo = PlayerVItemInfoById(transactionVItemInfo.Id);

        playerVItemInfo.Count += transactionVItemInfo.Delta;
       }

      VItemsTransactionCompletedEventHandler handler = VItemsTransactionCompleted;

      TransactionInfo transactionInfo =
       new TransactionInfo(cliTransactionId,srvTransactionId,corrUserId,vItems);

      if (handler != null)
       {
        handler(transactionInfo);
       }

      return transactionInfo;
     }

    internal TransactionInfo ApplyTransaction (Dictionary<string,string> _params, char vItemsItemSeparator, char vItemsFieldSeparator)
     {
      long cliTransactionId = MNUtils.ParseLong(MNUtils.DictReadValue(_params,"client_transaction_id")) ?? 0;
      long srvTransactionId = MNUtils.ParseLong(MNUtils.DictReadValue(_params,"server_transaction_id")) ?? 0;
      long corrUserId       = MNUtils.ParseLong(MNUtils.DictReadValue(_params,"corr_user_id")) ?? 0;
      string itemsToAdd     = MNUtils.DictReadValue(_params,"items_to_add");

      if (cliTransactionId < 0 || srvTransactionId < 0)
       {
        return null;
       }

      List<TransactionVItemInfo> vItemChanges = ParseTransactionVItemsListMessage(itemsToAdd,vItemsItemSeparator,vItemsFieldSeparator);

      if (vItemChanges == null)
       {
        return null;
       }

      return ApplyTransaction(srvTransactionId,cliTransactionId,corrUserId,vItemChanges.ToArray());
     }

    private void ProcessAddVItemsMessage (String message)
     {
      int  headerLength;
      long srvTransactionId;
      long cliTransactionId;
      long corrUserId;

      if (!ParseMessageHeader(message, out srvTransactionId, out cliTransactionId, out corrUserId, out headerLength))
       {
        return;
       }

      List<TransactionVItemInfo> vItemChanges = ParseTransactionVItemsListMessage(message.Substring(headerLength),'\n','\t');

      if (vItemChanges == null)
       {
        return;
       }

      ApplyTransaction(srvTransactionId,
                       cliTransactionId,
                       corrUserId,
                       vItemChanges.ToArray());
     }

    private void processFailMessage (String message)
     {
      int  headerLength;
      long srvTransactionId;
      long cliTransactionId;
      long corrUserId;

      if (!ParseMessageHeader(message, out srvTransactionId, out cliTransactionId, out corrUserId, out headerLength))
       {
        return;
       }

      string errorInfoStr = message.Substring(headerLength);
      int    separatorPos = errorInfoStr.IndexOf(MESSAGE_FIELD_SEPARATOR);
      int    failReasonCode;

      if (int.TryParse(errorInfoStr.Substring(0,separatorPos),out failReasonCode))
       {
        VItemsTransactionFailedEventHandler handler = VItemsTransactionFailed;
        
        if (handler != null)
         {
          TransactionError transactionError =
           new TransactionError(cliTransactionId,
                                srvTransactionId,
                                corrUserId,
                                failReasonCode,
                                errorInfoStr.Substring(separatorPos + 1));

          handler(transactionError);
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
      string data = message.Substring(MESSAGE_CMD_PREFIX_LEN);

      if      (cmd == 'g')
       {
        // ignore this message, it was used before 1.4.0 to get server
        // data version
       }
      else if (cmd == 'p')
       {
        playerVItemsOwnerId = session.GetMyUserId();

        if (data.Length > 0)
         {
          playerVItems = ParsePlayerVItemsListMessage(data);
         }
        else
         {
          playerVItems = new List<PlayerVItemInfo>();
         }
       }
      else if (cmd == 'a')
       {
        ProcessAddVItemsMessage(data);
       }
      else if (cmd == 'f')
       {
        processFailMessage(data);
       }
     }

    private void OnSessionWebEventReceived (string eventName, string eventParam, string callbackId)
     {
      if (eventName == "web.onUserDoAddItems" && eventParam != null)
       {
        ApplyTransaction(MNUtils.HttpGetRequestParseParams(eventParam),'\n','\t');
       }
     }

    private void OnSessionUserChanged (long userId)
     {
      if (userId == MNConst.MN_USER_ID_UNDEFINED || userId != playerVItemsOwnerId)
       {
        playerVItemsOwnerId = userId;
        playerVItems.Clear();
       }
     }

    private void OnVocabularyDownloadFinished (int downloadStatus)
     {
      if (downloadStatus >= 0)
       {
        VItemsListUpdatedEventHandler handler = VItemsListUpdated;

        if (handler != null)
         {
          handler();
         }
       }
     }

    private static bool ParseMessageHeader (string message, out long srvTransactionId, out long cliTransactionId,
                                            out long corrUserId, out int headerLength)
     {
      cliTransactionId = TRANSACTION_ID_UNDEFINED;
      srvTransactionId = TRANSACTION_ID_UNDEFINED;
      corrUserId       = MNConst.MN_USER_ID_UNDEFINED;
      int headerEndPos = message.IndexOf('\n');
      headerLength     = headerEndPos + 1;

      if (headerEndPos < 0)
       {
        return false;
       }

      string[] fields = message.Substring(0,headerEndPos).Split('\t');

      if (fields.Length != 3)
       {
        return false;
       }

      return long.TryParse(fields[0],out cliTransactionId) &&
             long.TryParse(fields[1],out srvTransactionId) &&
             long.TryParse(fields[2],out corrUserId);
     }

    public long GetNewClientTransactionId ()
     {
      clientTransactionId++;

      return clientTransactionId;
     }

    private long GenerateInitialClientTransactionId ()
     {
      return DateTime.Now.Ticks;
     }

    private static void LogWarning (string message)
     {
      MNDebug.warning("MNVItemsProvider: " + message);
     }

    private MNSession             session;
    private long                  playerVItemsOwnerId;
    private List<PlayerVItemInfo> playerVItems;
    private long                  clientTransactionId;

    private const int  MESSAGE_CMD_PREFIX_LEN  = 1;
    private const char MESSAGE_FIELD_SEPARATOR = '\t';

    private const string PROVIDER_NAME  = "com.playphone.mn.vi";
    private const string DATA_FILE_NAME = "MNVItemsProvider.xml";
    private static readonly string[] GameVItemEntriesXmlPath = { "GameVocabulary", "MNVItemsProvider", "VItems" };
   }
 }
