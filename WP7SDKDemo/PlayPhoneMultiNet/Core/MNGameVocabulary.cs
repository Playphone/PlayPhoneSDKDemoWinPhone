//
//  MNGameVocabulary.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.IO;
using System.Text;

namespace PlayPhone.MultiNet.Core
 {
  public class MNGameVocabulary
   {
    public const int MN_GV_DOWNLOAD_SUCCESS = 0;
    public const int MN_GV_DOWNLOAD_FAIL    = -1;

    public const int MN_GV_UPDATE_STATUS_DOWNLOAD_IN_PROGRESS = -200;
    public const int MN_GV_UPDATE_STATUS_CHECK_IN_PROGRESS    = -100;
    public const int MN_GV_UPDATE_STATUS_UNKNOWN              = -1;
    public const int MN_GV_UPDATE_STATUS_UP_TO_DATE           = 0;
    public const int MN_GV_UPDATE_STATUS_NEED_DOWNLOAD        = 1;

    public delegate void StatusUpdatedEventHandler    (int updateStatus);
    public delegate void DownloadStartedEventHandler  ();
    public delegate void DownloadFinishedEventHandler (int downloadStatus);

    public event StatusUpdatedEventHandler    StatusUpdated;
    public event DownloadStartedEventHandler  DownloadStarted;
    public event DownloadFinishedEventHandler DownloadFinished;

    public MNGameVocabulary (MNSession session)
     {
      this.session      = session;
      VocabularyStatus  = MN_GV_UPDATE_STATUS_UNKNOWN;
      webServerUrl      = null;
      dataDownloader    = null;
      versionDownloader = null;

      session.ConfigLoadStarted += OnSessionConfigLoadStarted;
      session.ConfigLoaded      += OnSessionConfigLoaded;
      session.ErrorOccurred     += OnSessionErrorOccurred;
     }

    public int VocabularyStatus { get; private set; }

    public bool StartDownload ()
     {
      if (VocabularyStatus != MN_GV_UPDATE_STATUS_NEED_DOWNLOAD)
       {
        return false;
       }

      SetVocabularyStatus(MN_GV_UPDATE_STATUS_DOWNLOAD_IN_PROGRESS);

      DispatchDownloadStartedEvent();

      string webServerUrl = GetWebServerUrl();

      if (webServerUrl != null)
       {
        string dataUrl = webServerUrl + "/" + VOCABULARY_DATA_URL_PATH;

        dataDownloader = new MNURLFileDownloader();

        if (MNPlatformWinPhone.CreateTempDirectory())
         {
          dataDownloader.LoadUrl
           (MNPlatformWinPhone.GetTempFileFullName(GV_FILE_TEMP_NAME),
            dataUrl,
            session.BuildDefaultQueryParamsDict(false),
            OnDataDownloadSucceeded,
            OnDataDownloadFailed);

          return true;
         }
       }

      DispatchDownloadFinishedEvent(MN_GV_DOWNLOAD_FAIL);

      SetVocabularyStatus(MN_GV_UPDATE_STATUS_NEED_DOWNLOAD);

      return false;
     }

    public void CheckForUpdate ()
     {
      if (VocabularyStatus == MN_GV_UPDATE_STATUS_DOWNLOAD_IN_PROGRESS ||
          VocabularyStatus == MN_GV_UPDATE_STATUS_CHECK_IN_PROGRESS ||
          VocabularyStatus == MN_GV_UPDATE_STATUS_NEED_DOWNLOAD)
       {
        return;
       }

      SetVocabularyStatus(MN_GV_UPDATE_STATUS_CHECK_IN_PROGRESS);

      if (versionDownloader != null)
       {
        versionDownloader.Cancel();
       }

      string webServerUrl = GetWebServerUrl();

      if (webServerUrl != null)
       {
        string versionUrl = webServerUrl + "/" + VOCABULARY_VERSION_URL_PATH;

        versionDownloader = new MNURLStringDownloader();

        versionDownloader.LoadUrl
         (versionUrl,
          session.BuildDefaultQueryParamsDict(false),
          OnVersionDownloadSucceeded,
          OnVersionDownloadFailed);
       }
      else
       {
        SetVocabularyStatus(MN_GV_UPDATE_STATUS_UNKNOWN);
       }
     }

    public static bool IsUpdateStatusFinal (int status)
     {
      return status > -100;
     }

    public byte[] GetFileData (string fileName)
     {
      if (CachedFileExists())
       {
        return GetCachedFileData(fileName);
       }
      else
       {
        return GetAssetsFileData(fileName);
       }
     }

    private byte[] GetCachedFileData (string fileName)
     {
      byte[] data = null;

      using (Stream dataFileStream = MNPlatformWinPhone.GetDataFileReadStream(GV_FILE_NAME))
       {
        data = MNZipTool.GetFileDataFromArchiveStream(dataFileStream,fileName);
       }

      return data;
     }

    private byte[] GetAssetsFileData (string fileName)
     {
      MNDebug.NotImpl("MNGameVocabulary: offline data pack is not supported");

      return null;
     }

    private bool CachedFileExists ()
     {
      return MNPlatformWinPhone.DataFileExists(GV_FILE_NAME);
     }

    private void OnVersionDownloadSucceeded  (string remoteVersion)
     {
      if (remoteVersion == null || remoteVersion.Length == 0)
       {
        SetVocabularyStatus(MN_GV_UPDATE_STATUS_UNKNOWN);
       }
      else
       {
        string localVersion = ReadLocalVersion();

        if (remoteVersion == localVersion)
         {
          SetVocabularyStatus(MN_GV_UPDATE_STATUS_UP_TO_DATE);
         }
        else
         {
          SetVocabularyStatus(MN_GV_UPDATE_STATUS_NEED_DOWNLOAD);
         }
       }
     }

    private void OnVersionDownloadFailed  (int httpStatus, string message)
     {
      // if data download is in progress, new state will be set after
      // download finishes
      if (VocabularyStatus != MN_GV_UPDATE_STATUS_DOWNLOAD_IN_PROGRESS)
       {
        SetVocabularyStatus(MN_GV_UPDATE_STATUS_UNKNOWN);
       }
     }

    private void OnDataDownloadSucceeded  ()
     {
      if (MNPlatformWinPhone.MoveTempFileToDataFile(GV_FILE_TEMP_NAME,GV_FILE_NAME))
       {
        DispatchDownloadFinishedEvent(MN_GV_DOWNLOAD_SUCCESS);
        SetVocabularyStatus(MN_GV_UPDATE_STATUS_UP_TO_DATE);
       }
      else
       {
        DispatchDownloadFinishedEvent(MN_GV_DOWNLOAD_FAIL);
        SetVocabularyStatus(MN_GV_UPDATE_STATUS_NEED_DOWNLOAD);
       }
     }

    private void OnDataDownloadFailed  (int httpStatus, string message)
     {
      DispatchDownloadFinishedEvent(MN_GV_DOWNLOAD_FAIL);

      SetVocabularyStatus(MN_GV_UPDATE_STATUS_NEED_DOWNLOAD);
     }

    private void OnSessionConfigLoadStarted ()
     {
      SetVocabularyStatus(MN_GV_UPDATE_STATUS_CHECK_IN_PROGRESS);
     }

    private void OnSessionConfigLoaded ()
     {
      string newWebServerUrl = session.GetWebServerURL();

      if (newWebServerUrl != null)
       {
        webServerUrl = newWebServerUrl;
       }

      OnVersionDownloadSucceeded(session.GetConfigData().gameVocabularyVersion);
     }

    private void OnSessionErrorOccurred (MNErrorInfo errorInfo)
     {
      if (errorInfo.ActionCode == MNErrorInfo.ACTION_CODE_LOAD_CONFIG)
       {
        SetVocabularyStatus(MN_GV_UPDATE_STATUS_UNKNOWN);
       }
     }

    private void DispatchDownloadStartedEvent ()
     {
      DownloadStartedEventHandler handler = DownloadStarted;

      if (handler != null)
       {
        handler();
       }
     }

    private void DispatchDownloadFinishedEvent (int downloadStatus)
     {
      DownloadFinishedEventHandler handler = DownloadFinished;

      if (handler != null)
       {
        handler(downloadStatus);
       }
     }

    private void SetVocabularyStatus (int newStatus)
     {
      VocabularyStatus = newStatus;

      StatusUpdatedEventHandler handler = StatusUpdated;

      if (handler != null)
       {
        handler(newStatus);
       }
     }

    private string ReadLocalVersion ()
     {
      byte[] data = GetFileData(LOCAL_VERSION_FILE_NAME);

      if (data == null)
       {
        return "";
       }

      try
       {
        return UTF8Encoding.UTF8.GetString(data,0,data.Length);
       }
      catch (DecoderFallbackException)
       {
        return "";
       }
     }

    private string GetWebServerUrl ()
     {
      if (webServerUrl == null)
       {
        webServerUrl = session.GetWebServerURL();
       }

      return webServerUrl;
     }

    private MNSession             session;
    private string                webServerUrl;
    private MNURLFileDownloader   dataDownloader;
    private MNURLStringDownloader versionDownloader;

    private const string GV_FILE_NAME      = "data_game_vocabulary.zip";
    private const string GV_FILE_TEMP_NAME = GV_FILE_NAME + ".tmp";
    private const string LOCAL_VERSION_FILE_NAME = "data_game_vocabulary_version.txt";
    private const string VOCABULARY_VERSION_URL_PATH = "data_game_vocabulary_version_txt.php";
    private const string VOCABULARY_DATA_URL_PATH    = "data_game_vocabulary_zip.php";
   }
 }
