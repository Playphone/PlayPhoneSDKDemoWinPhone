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
    public delegate void VShopInfoUpdatedEventHandler ();

    public event VShopInfoUpdatedEventHandler VShopInfoUpdated;

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

    public MNVShopProvider (MNSession session)
     {
      this.session = session;

      session.GetGameVocabulary().DownloadFinished += OnVocabularyDownloadFinished;
     }

    public void Shutdown ()
     {
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
      return GetVShopPackListLow().First(value => value.Id == id);
     }

    public VShopCategoryInfo FindVShopCategoryById (int id)
     {
      return GetVShopCategoryListLow().First(value => value.Id == id);
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

    private MNSession session;

    private const string DATA_FILE_NAME = "MNVShopProvider.xml";
    private static readonly string[] VShopPackListEntriesXmlPath = { "GameVocabulary", "MNVShopProvider", "VShopPacks" };
    private static readonly string[] VShopCategoryListEntriesXmlPath = { "GameVocabulary", "MNVShopProvider", "VShopCategories" };
   }
 }
