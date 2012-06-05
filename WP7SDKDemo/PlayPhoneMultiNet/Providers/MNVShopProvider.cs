//
//  MNVShopProvider.cs
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
  public class MNVShopProvider
   {
    public delegate void VShopInfoUpdatedEventHandler         ();
    public delegate void ShowDashboardEventHandler            ();
    public delegate void HideDashboardEventHandler            ();
    public delegate void CheckoutVShopPackSuccessEventHandler (CheckoutVShopPackSuccessInfo result);
    public delegate void CheckoutVShopPackFailEventHandler    (CheckoutVShopPackFailInfo    result);
    public delegate void VShopReadyStatusChangedEventHandler  (bool                        isVShopReady);

    public event VShopInfoUpdatedEventHandler         VShopInfoUpdated;
    public event ShowDashboardEventHandler            ShowDashboard;
    public event HideDashboardEventHandler            HideDashboard;
    public event CheckoutVShopPackSuccessEventHandler CheckoutVShopPackSuccess;
    public event CheckoutVShopPackFailEventHandler    CheckoutVShopPackFail;
    public event VShopReadyStatusChangedEventHandler  VShopReadyStatusChanged;

    public class CheckoutVShopPackSuccessInfo
     {
      public MNVItemsProvider.TransactionInfo Transaction { get; private set; }

      public CheckoutVShopPackSuccessInfo (MNVItemsProvider.TransactionInfo transaction)
       {
        Transaction = transaction;
       }
     }

    public class CheckoutVShopPackFailInfo
     {
      public int    ErrorCode           { get; private set; }
      public string ErrorMessage        { get; private set; }
      public long   ClientTransactionId { get; private set; }

      public const int ERROR_CODE_NO_ERROR            =    0;
      public const int ERROR_CODE_USER_CANCEL         = -999;
      public const int ERROR_CODE_UNDEFINED           = -998;
      public const int ERROR_CODE_XML_PARSE_ERROR     = -997;
      public const int ERROR_CODE_XML_STRUCTURE_ERROR = -996;
      public const int ERROR_CODE_NETWORK_ERROR       = -995;
      public const int ERROR_CODE_GENERIC             = -994;
      public const int ERROR_CODE_WF_NOT_READY        =  200;

      public CheckoutVShopPackFailInfo (int errorCode, string errorMessage, long cliTransactionId)
       {
        ErrorCode           = errorCode;
        ErrorMessage        = errorMessage;
        ClientTransactionId = cliTransactionId;
       }
     }

    public class VShopDeliveryInfo
     {
      public int  VItemId { get; set; }
      public long Amount  { get; set; }

      public VShopDeliveryInfo (int vItemId, long amount)
       {
        VItemId = vItemId;
        Amount  = amount;
       }
     }

    public class VShopPackInfo
     {
      public const int IS_HIDDEN_MASK     = 0x0001;
      public const int IS_HOLD_SALES_MASK = 0x0002;

      public int                 Id          { get; set; }
      public string              Name        { get; set; }
      public uint                Model       { get; set; }
      public string              Description { get; set; }
      public string              AppParams   { get; set; }
      public int                 SortPos     { get; set; }
      public int                 CategoryId  { get; set; }
      public VShopDeliveryInfo[] Delivery    { get; set; }
      public int                 PriceItemId { get; set; }
      public long                PriceValue  { get; set; }

      public VShopPackInfo (int id, string name)
       {
        Id          = id;
        Name        = name;
        Model       = 0;
        Description = "";
        AppParams   = "";
        SortPos     = 0;
        CategoryId  = 0;
        Delivery    = null;
        PriceItemId = 0;
        PriceValue  = 0;
       }
     }

    public class VShopCategoryInfo
     {
      public int    Id      { get; set; }
      public string Name    { get; set; }
      public int    SortPos { get; set; }

      public VShopCategoryInfo (int id, string name)
       {
        Id      = id;
        Name    = name;
        SortPos = 0;
       }
     }

    public MNVShopProvider (MNSession session, MNVItemsProvider vItemsProvider)
     {
      this.session        = session;
      this.vItemsProvider = vItemsProvider;

      requestHelper = new MNVShopWSRequestHelper(session,new RequestHelperEventHandler(this));

      session.GetGameVocabulary().DownloadFinished += OnVocabularyDownloadFinished;

      session.ExecUICommandReceived   += OnSessionExecUICommandReceived;
      session.VShopReadyStatusChanged += OnSessionVShopReadyStatusChanged;
     }

    public void Shutdown ()
     {
      session.ExecUICommandReceived   -= OnSessionExecUICommandReceived;
      session.VShopReadyStatusChanged -= OnSessionVShopReadyStatusChanged;

      requestHelper.Shutdown();

      session.GetGameVocabulary().DownloadFinished -= OnVocabularyDownloadFinished;
     }

    private List<VShopPackInfo> GetVShopPackListLow ()
     {
      bool ok = true;
      List<VShopPackInfo> packs = new List<VShopPackInfo>();
      byte[] fileData   = session.GetGameVocabulary().GetFileData(DATA_FILE_NAME);

      if (fileData == null)
       {
        return packs;
       }

      try
       {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        MemoryStream      sourceStream   = new MemoryStream(fileData);

        readerSettings.DtdProcessing = DtdProcessing.Ignore;

        using (XmlReader reader = XmlReader.Create(sourceStream,readerSettings))
         {
          if (!MNXmlTools.SeekElementByPath(reader,VShopPackListEntriesXmlPath))
           {
            ok = false;

            LogWarning("cannot find \"VShopPacks\" element in document");
           }

          if (ok)
           {
            List<Dictionary<string,string>> items = MNXmlTools.ParseItemList(reader,"entry");

            foreach (var itemData in items)
             {
              int? id = MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"id"));

              if (id != null)
               {
                string name = MNUtils.DictReadValue(itemData,"name") ?? "";

                VShopPackInfo packInfo = new VShopPackInfo(id.Value,name);

                packInfo.Model       = MNUtils.ParseUInt(MNUtils.DictReadValue(itemData,"model")) ?? 0;
                packInfo.Description = MNUtils.DictReadValue(itemData,"desc") ?? "";
                packInfo.AppParams   = MNUtils.DictReadValue(itemData,"params") ?? "";
                packInfo.SortPos     = MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"sortPos")) ?? 0;
                packInfo.CategoryId  = MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"categoryId")) ?? 0;
                packInfo.Delivery    = new VShopDeliveryInfo[1];
                packInfo.Delivery[0] = new VShopDeliveryInfo
                                            (MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"deliveryItemId")) ?? 0,
                                             MNUtils.ParseLong(MNUtils.DictReadValue(itemData,"deliveryItemAmount")) ?? 0);
                packInfo.PriceItemId = MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"priceItemId")) ?? -1;
                packInfo.PriceValue  = MNUtils.ParseLong(MNUtils.DictReadValue(itemData,"priceValue")) ?? 0;

                packs.Add(packInfo);
               }
              else
               {
                LogWarning("vshop pack with invalid or absent id ignored");
               }
             }
           }
         }
       }
      catch (Exception e)
       {
        ok = false;

        LogWarning("vshop packs data parsing failed (" + e.ToString() + ")");
       }

      if (!ok)
       {
        packs.Clear();
       }

      return packs;
     }

    private List<VShopCategoryInfo> GetVShopCategoryListLow ()
     {
      bool ok = true;
      List<VShopCategoryInfo> categories = new List<VShopCategoryInfo>();
      byte[] fileData = session.GetGameVocabulary().GetFileData(DATA_FILE_NAME);

      if (fileData == null)
       {
        return categories;
       }

      try
       {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        MemoryStream      sourceStream   = new MemoryStream(fileData);

        readerSettings.DtdProcessing = DtdProcessing.Ignore;

        using (XmlReader reader = XmlReader.Create(sourceStream,readerSettings))
         {
          if (!MNXmlTools.SeekElementByPath(reader,VShopCategoryListEntriesXmlPath))
           {
            ok = false;

            LogWarning("cannot find \"VShopCategories\" element in document");
           }

          if (ok)
           {
            List<Dictionary<string,string>> items = MNXmlTools.ParseItemList(reader,"entry");

            foreach (var itemData in items)
             {
              int? id = MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"id"));

              if (id != null)
               {
                string name = MNUtils.DictReadValue(itemData,"name") ?? "";

                VShopCategoryInfo categoryInfo = new VShopCategoryInfo(id.Value,name);

                categoryInfo.SortPos = MNUtils.ParseInt(MNUtils.DictReadValue(itemData,"sortPos")) ?? 0;

                categories.Add(categoryInfo);
               }
              else
               {
                LogWarning("vshop category with invalid or absent id ignored");
               }
             }
           }
         }
       }
      catch (Exception e)
       {
        ok = false;

        LogWarning("vshop categories data parsing failed (" + e.ToString() + ")");
       }

      if (!ok)
       {
        categories.Clear();
       }

      return categories;
     }

    public VShopPackInfo[] GetVShopPackList ()
     {
      return GetVShopPackListLow().ToArray();
     }

    public VShopCategoryInfo[] GetVShopCategoryList ()
     {
      return GetVShopCategoryListLow().ToArray();
     }

    public VShopPackInfo FindVShopPackById (int id)
     {
      return GetVShopPackListLow().FirstOrDefault(value => value.Id == id);
     }

    public VShopCategoryInfo FindVShopCategoryById (int id)
     {
      return GetVShopCategoryListLow().FirstOrDefault(value => value.Id == id);
     }

    public bool IsVShopInfoNeedUpdate ()
     {
      return session.GetGameVocabulary().VocabularyStatus > 0;
     }

    public void DoVShopInfoUpdate ()
     {
      if (session.GetGameVocabulary().VocabularyStatus !=
           MNGameVocabulary.MN_GV_UPDATE_STATUS_DOWNLOAD_IN_PROGRESS)
       {
        session.GetGameVocabulary().StartDownload();
       }
     }

    public string GetVShopPackImageURL (int id)
     {
      string webServerUrl = session.GetWebServerURL();

      if (webServerUrl != null)
       {
        StringBuilder builder = new StringBuilder(webServerUrl);

        builder.Append("/data_game_shoppack_image.php?game_id=");
        builder.Append(session.GetGameId().ToString());
        builder.Append("&gameshoppack_id=");
        builder.Append(id.ToString());

        return builder.ToString();
       }
      else
       {
        return null;
       }
     }

    public void ExecCheckoutVShopPacks (int[] packIdArray,
                                        int[] packCountArray,
                                        long  cliTransactionId)
     {
      if (session.IsWebShopReady())
       {
        session.ExecAppCommand
         ("jumpToBuyVShopPackRequestDialogSimple",
          "pack_id=" + string.Join(",",packIdArray) + "&" +
          "buy_count=" + string.Join(",",packCountArray) + "&" +
          "client_transaction_id=" + cliTransactionId.ToString());
       }
      else
       {
        string errorMessage = session.VarStorageGetValueForVariable
                               ("hook.ui.shop_not_ready_error_message");

        if (errorMessage == null)
         {
          errorMessage = MNI18n.GetLocalizedString
                          ("Purchase system is loading. Please rety later.",
                           MNI18n.MESSAGE_CODE_PURCHASE_SYSTEM_IS_NOT_READY_ERROR);
         }

        DispatchCheckoutFailedEvent(CheckoutVShopPackFailInfo.ERROR_CODE_WF_NOT_READY,errorMessage,cliTransactionId);
       }
     }

    public void ProcCheckoutVShopPacksSilent (int[] packIdArray,
                                              int[] packCountArray,
                                              long  cliTransactionId)
     {
      string webServerUrl = session.GetWebServerURL();

      if (webServerUrl != null)
       {
        Dictionary<string,string> queryParams = new Dictionary<string,string>();

        queryParams["proc_pack_id"]    = string.Join(",",packIdArray);
        queryParams["proc_pack_count"] = string.Join(",",packCountArray);
        queryParams["proc_client_transaction_id"] = cliTransactionId.ToString();

        requestHelper.SendWSRequest
         (webServerUrl + "/" + SilentPurchaseWebServicePath,
          queryParams,cliTransactionId);
       }
      else
       {
        DispatchCheckoutFailedEvent
         (CheckoutVShopPackFailInfo.ERROR_CODE_NETWORK_ERROR,
          "checkout endpoint is unreachable",
          cliTransactionId);
       }
     }

    public bool isVShopReady ()
     {
      return session.IsWebShopReady();
     }

    private void DispatchCheckoutSucceededEvent (MNVItemsProvider.TransactionInfo transactionInfo)
     {
      CheckoutVShopPackSuccessInfo info = new CheckoutVShopPackSuccessInfo(transactionInfo);

      CheckoutVShopPackSuccessEventHandler handler = CheckoutVShopPackSuccess;

      if (handler != null)
       {
        handler(info);
       }
     }

    private void DispatchCheckoutFailedEvent (int errorCode, string errorMessage, long cliTransactionId)
     {
      CheckoutVShopPackFailInfo info = new CheckoutVShopPackFailInfo(errorCode,errorMessage,cliTransactionId);

      CheckoutVShopPackFailEventHandler handler = CheckoutVShopPackFail;

      if (handler != null)
       {
        handler(info);
       }
     }

    private class RequestHelperEventHandler : MNVShopWSRequestHelper.IEventHandler
     {
      public RequestHelperEventHandler (MNVShopProvider parent)
       {
        this.parent = parent;
       }

      public bool VShopShouldParseResponse        (long   userId)
       {
        return userId == parent.session.GetMyUserId();
       }

      public void VShopPostVItemTransaction       (long   srvTransactionId,
                                                    long   cliTransactionId,
                                                    string itemsToAddStr,
                                                    bool   vShopTransactionEnabled)
       {
        Dictionary<string,string> queryParams = new Dictionary<string,string>();

        queryParams["server_transaction_id"] = srvTransactionId.ToString();
        queryParams["client_transaction_id"] = cliTransactionId.ToString();
        queryParams["items_to_add"]          = itemsToAddStr;

        MNVItemsProvider.TransactionInfo transactionInfo
         = parent.vItemsProvider.ApplyTransaction(queryParams,',',':');

        if (transactionInfo != null)
         {
          if (vShopTransactionEnabled)
           {
            parent.DispatchCheckoutSucceededEvent(transactionInfo);
           }
         }
        else
         {
          LogWarning("unable to process transaction - invalid parameters");
         }
       }

      public void VShopPostVShopTransactionFailed (long   cliTransactionId,
                                                    int    errorCode,
                                                    string errorMessage)
       {
        parent.DispatchCheckoutFailedEvent(errorCode,errorMessage,cliTransactionId);
       }

      public void VShopFinishTransaction          (string transactionId)
       {
        // is not used for "silent" requests
       }

      public void VShopWSRequestFailed            (long   cliTransactionId,
                                                    int    errorCode,
                                                    string errorMessage)
       {
        parent.DispatchCheckoutFailedEvent(errorCode,errorMessage,cliTransactionId);
       }

      private MNVShopProvider parent;
     }

    private void OnSessionExecUICommandReceived  (string cmdName, string cmdParam)
     {
      if (cmdName == "onVShopNeedShowDashboard")
       {
        ShowDashboardEventHandler handler = ShowDashboard;

        if (handler != null)
         {
          handler();
         }
       }
      else if (cmdName == "onVShopNeedHideDashboard")
       {
        HideDashboardEventHandler handler = HideDashboard;

        if (handler != null)
         {
          handler();
         }
       }
      else
       {
        bool ok = cmdName == "afterBuyVShopPackRequestSuccess";

        if (ok || cmdName == "afterBuyVShopPackRequestFail")
         {
          Dictionary<string,string> cmdParams = MNUtils.HttpGetRequestParseParams(cmdParam);

          if (ok)
           {
            MNVItemsProvider.TransactionInfo transactionInfo
             = vItemsProvider.ApplyTransaction(cmdParams,'\n','\t');

            if (transactionInfo != null)
             {
              DispatchCheckoutSucceededEvent(transactionInfo);
             }
            else
             {
              LogWarning("unable to process transaction - invalid parameters");
             }
           }
          else
           {
            DispatchCheckoutFailedEvent
             (MNUtils.ParseInt(MNUtils.DictReadValue(cmdParams,"error_code")) ?? CheckoutVShopPackFailInfo.ERROR_CODE_UNDEFINED,
              MNUtils.DictReadValue(cmdParams,"error_message") ?? "undefined error",
              MNUtils.ParseLong(MNUtils.DictReadValue(cmdParams,"client_transaction_id")) ?? MNVItemsProvider.TRANSACTION_ID_UNDEFINED);
           }
         }
       }
     }

    private void OnSessionVShopReadyStatusChanged (bool isVShopReady)
     {
      VShopReadyStatusChangedEventHandler handler =  VShopReadyStatusChanged;

      if (handler != null)
       {
        handler(isVShopReady);
       }
     }

    private void OnVocabularyDownloadFinished (int downloadStatus)
     {
      if (downloadStatus >= 0)
       {
        VShopInfoUpdatedEventHandler handler = VShopInfoUpdated;

        if (handler != null)
         {
          handler();
         }
       }
     }

    private static void LogWarning (string message)
     {
      MNDebug.warning("MNVShopProvider: " + message);
     }

    private MNSession              session;
    private MNVItemsProvider       vItemsProvider;
    private MNVShopWSRequestHelper requestHelper;

    private const string DATA_FILE_NAME = "MNVShopProvider.xml";
    private static readonly string[] VShopPackListEntriesXmlPath = { "GameVocabulary", "MNVShopProvider", "VShopPacks" };
    private static readonly string[] VShopCategoryListEntriesXmlPath = { "GameVocabulary", "MNVShopProvider", "VShopCategories" };
    private const string SilentPurchaseWebServicePath = "user_ajax_proc_silent_purchase.php";
   }
 }
