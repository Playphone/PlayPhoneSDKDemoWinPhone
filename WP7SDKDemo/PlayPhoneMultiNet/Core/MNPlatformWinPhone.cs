//
//  MNPlatformWinPhone.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System.IO.IsolatedStorage;
using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Windows.Resources;
using System.Xml;

using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;

namespace PlayPhone.MultiNet.Core
 {
  public static class MNPlatformWinPhone
   {
    public static string GetMultiNetConfigURL ()
     {
      Dictionary<string,string> config    = LoadMultiNetConfFile();
      string                    configUrl = null;

      if (config != null)
       {
        if (!config.TryGetValue(MULTINET_CONFIG_URL_PARAM,out configUrl))
         {
          configUrl = null;
         }
       }

      return configUrl;
     }

    public static string GetUniqueDeviceIdentifier ()
     {
      object deviceId;

      if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId",out deviceId))
       {
        return MNUtils.ByteArrayToHexString((byte[])deviceId);
       }
      else
       {
        //FIXME: it is not the best solution to return empty string here
        return "";
       }
     }

    public static string GetAppVerInternal ()
     {
      string version        = null;
      Uri    appManifestUri = new Uri("WMAppManifest.xml",UriKind.Relative);

      try
       {
        using (Stream sourceStream = Application.GetResourceStream(appManifestUri).Stream)
         {
          if (sourceStream != null)
           {
            XmlReaderSettings readerSettings = new XmlReaderSettings();

            readerSettings.DtdProcessing = DtdProcessing.Ignore;

            using (XmlReader reader = XmlReader.Create(sourceStream,readerSettings))
             {
              if (reader.ReadToDescendant("App"))
               {
                if (reader.IsStartElement())
                 {
                  version = reader.GetAttribute("Version");
                 }
               }
             }
           }
         }
       }
      catch (Exception)
       {
       }

      return version;
     }

    public static string GetAppVerExternal ()
     {
      string extVersion = null;
      string intVersion = GetAppVerInternal();

      if (intVersion != null)
       {
        int pos = intVersion.IndexOf('.');

        if (pos >= 0)
         {
          pos = intVersion.IndexOf('.',pos + 1);
         }

        if (pos >= 0)
         {
          extVersion = intVersion.Substring(0,pos);
         }
        else
         {
          extVersion = intVersion;
         }
       }

      return extVersion;
     }

    public static int GetDeviceType ()
     {
      return DEVICE_TYPE_CODE;
     }

    public static string GetDeviceInfoString ()
     {
      TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

      string info = DeviceStatus.DeviceName + "|" +
                    DEVICE_OS_NAME + "|" +
                    Environment.OSVersion.Version.ToString() + "|" +
                    System.Globalization.CultureInfo.CurrentCulture.Name + "|{" +
                    (int)localTimeZone.BaseUtcOffset.TotalSeconds + "+" +
                    "*" + "+" +
                    /*localTimeZone.StandardName +*/ "}"; // .NET does not have information on tz abbreviations

      return info;
     }

    private static bool CreateIsolatedStorageDir (string path)
     {
      bool ok = true;

      IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

      if (!store.DirectoryExists(path))
       {
        try
         {
          store.CreateDirectory(path);
         }
        catch (Exception)
         {
          ok = false;
         }
       }

      return ok;
     }

    public static string GetDataFileFullName (string dataFileShortName)
     {
      return MULTINET_DATA_DIR + "\\" + dataFileShortName;
     }

    public static string GetTempFileFullName (string tempFileShortName)
     {
      return MULTINET_TEMP_DIR + "\\" + tempFileShortName;
     }

    public static bool CreateDataDirectory ()
     {
      return CreateIsolatedStorageDir(MULTINET_DATA_DIR);
     }

    public static bool DataFileExists (string fileName)
     {
      IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

      return store.FileExists(GetDataFileFullName(fileName));
     }

    public static Stream GetDataFileStream (string fileName, FileMode mode, FileAccess access)
     {
      IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

      Stream result = null;

      try
       {
        result = new IsolatedStorageFileStream(GetDataFileFullName(fileName),mode,access,store);
       }
      catch (IsolatedStorageException)
       {
       }

      return result;
     }

    public static Stream GetDataFileReadStream (string fileName)
     {
      return GetDataFileStream(fileName,FileMode.Open,FileAccess.Read);
     }

    public static Stream GetDataFileWriteStream (string fileName)
     {
      return GetDataFileStream(fileName,FileMode.OpenOrCreate,FileAccess.Write);
     }

    public static bool DeleteDataFile (string fileName)
     {
      IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

      try
       {
        store.DeleteFile(GetDataFileFullName(fileName));

        return true;
       }
      catch (IsolatedStorageException)
       {
        return false;
       }
     }

    public static bool MoveTempFileToDataFile (string tempFileName, string dataFileName)
     {
      IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

      try
       {
        string tempFullFileName = MULTINET_TEMP_DIR + "\\" + tempFileName;
        string dataFullFileName = GetDataFileFullName(dataFileName);

        if (store.FileExists(dataFullFileName))
         {
          store.DeleteFile(dataFullFileName);
         }

        store.MoveFile(tempFullFileName,dataFullFileName);

        return true;
       }
      catch (Exception e)
       {
        MNDebug.error("unable to move temp file into data dir: " + e.ToString());
        
        return false;
       }
     }

    public static bool CreateTempDirectory ()
     {
      return CreateIsolatedStorageDir(MULTINET_TEMP_DIR);
     }

    public static void CopyStreamToFile (string destFileName, Stream sourceStream)
     {
      IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

      using (var destStream = new IsolatedStorageFileStream(destFileName,FileMode.OpenOrCreate,store))
       {
        sourceStream.CopyTo(destStream);
       }
     }

    public static Dictionary<string,string> ReadAppExtParams ()
     {
      Dictionary<string,string> appExtParams = new Dictionary<string,string>();

      try
       {
        StreamResourceInfo sourceStreamInfo = GetResourceStream(MULTINET_APPEXT_PARAMS_FILENAME);

        if (sourceStreamInfo != null)
         {
          StreamReader reader = new StreamReader(sourceStreamInfo.Stream);

          while (!reader.EndOfStream)
           {
            string line = reader.ReadLine().Trim();

            if (line.Length > 0)
             {
              int pos = line.IndexOf('=');

              if (pos < 0)
               {
                MNDebug.warning("invalid appExtParam line format: [" + line + "], ignored");
               }
              else
               {
                appExtParams[MULTINET_APPEXT_PARAMS_PREFIX + line.Substring(0,pos).TrimEnd()] = line.Substring(pos + 1).TrimStart();
               }
             }
           }
         }
       }
      catch (IOException)
       {
       }

      return appExtParams;
     }

    private static Dictionary<string,string> LoadPListFile (Stream sourceStream)
     {
      bool ok                          = true;
      Dictionary<string,string> result = new Dictionary<string,string>();

      XmlReaderSettings readerSettings = new XmlReaderSettings();

      readerSettings.DtdProcessing = DtdProcessing.Ignore;

      using (XmlReader reader = XmlReader.Create(sourceStream,readerSettings))
       {
        reader.Read();
        reader.MoveToContent();

        if (reader.NodeType == XmlNodeType.Element && reader.Name == "plist")
         {
          reader.Read();
          reader.MoveToContent();
         }
        else
         {
          ok = false;

          MNDebug.error("unable to parse .plist file - \"plist\" element not found");
         }

        if (reader.NodeType == XmlNodeType.Element && reader.Name == "dict")
         {
          bool done = false;

          while (ok && !done)
           {
            reader.Read();
            reader.MoveToContent();

            if (reader.NodeType == XmlNodeType.EndElement)
             {
              if (reader.Name == "dict")
               {
                done = true;
               }
             }

            if (!done)
             {
              if (reader.NodeType == XmlNodeType.Element && reader.Name == "key")
               {
                string key = reader.ReadInnerXml().Trim();

                reader.Read();
                reader.MoveToContent();

                if (reader.NodeType == XmlNodeType.Element)
                 {
                  string value = null;

                  if (reader.Name == "string")
                   {
                    value = reader.ReadInnerXml().Trim();
                   }
                  else
                   {
                    MNDebug.error("unable to parse .plist file - unsupported value type for key " + key);

                    ok = false;
                   }

                  if (ok)
                   {
                    result[key] = value;
                   }
                 }
                else
                 {
                  MNDebug.error("unable to parse .plist file - missing value for key " + key);

                  ok = false;
                 }
               }
              else
               {
                MNDebug.error("unable to parse .plist file - \"key\" element is missing");

                ok = false;
               }
             }
           }
         }
        else
         {
          ok = false;

          MNDebug.error("unable to parse .plist file - \"dict\" element not found");
         }
       }
 
      return ok ? result : null;
     }

    private static Dictionary<string,string> LoadMultiNetConfFile ()
     {
      Dictionary<string,string> result = null;

      try
       {
        StreamResourceInfo sourceStreamInfo = GetResourceStream(MULTINET_CONF_FILENAME);

        if (sourceStreamInfo != null)
         {
          result = LoadPListFile(sourceStreamInfo.Stream);
         }
       }
      catch (IOException)
       {
       }

      return result;
     }

    public static StreamResourceInfo GetResourceStream (string path)
     {
      // First, try to find resource inside assembly:
      string embeddedPath = "/" + ASSEMBLY_NAME + ";component/" + path;

      StreamResourceInfo sourceStreamInfo = Application.GetResourceStream
                                             (new Uri(embeddedPath,UriKind.Relative));

      if (sourceStreamInfo == null)
       {
        // If resource not found, trying to find it inside content
        sourceStreamInfo = Application.GetResourceStream(new Uri(path,UriKind.Relative));
       }

      return sourceStreamInfo;
     }

    public static double GetSystemTrayHeight ()
     {
      return SystemTray.IsVisible ? SystemTrayPortraitHeight : 0;
     }

    private const int DEVICE_TYPE_CODE = 5000;
    private const string DEVICE_OS_NAME         = "WinPhone";
    private const string ASSEMBLY_NAME          = "PlayPhoneMultiNet";
    private const string MULTINET_RESOURCE_DIR  = "Assets";
    //FIXME: make it private
    public  const string MULTINET_DATA_DIR      = "multinet";
    private const string MULTINET_TEMP_DIR      = "multinet_temp";
    private const string MULTINET_CONF_FILENAME = MULTINET_RESOURCE_DIR + "/multinet.plist";
    private const string MULTINET_APPEXT_PARAMS_FILENAME = MULTINET_RESOURCE_DIR + "/multinet_appext.ini";
    private const string MULTINET_APPEXT_PARAMS_PREFIX = "appext_";

    private const string MULTINET_CONFIG_URL_PARAM = "MultiNetConfigServerURL";

    private const double SystemTrayPortraitHeight = 32.0;
   }
 }
