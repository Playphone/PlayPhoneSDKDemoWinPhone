//
//  MNGameCookiesProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNGameCookiesProvider
   {
    public delegate void GameCookieDownloadSucceededEventHandler (int key, string cookie);
    public delegate void GameCookieDownloadFailedEventHandler    (int key, string error);
    public delegate void GameCookieUploadSucceededEventHandler   (int key);
    public delegate void GameCookieUploadFailedEventHandler      (int key, string error);

    public event GameCookieDownloadSucceededEventHandler GameCookieDownloadSucceeded;
    public event GameCookieDownloadFailedEventHandler    GameCookieDownloadFailed;
    public event GameCookieUploadSucceededEventHandler   GameCookieUploadSucceeded;
    public event GameCookieUploadFailedEventHandler      GameCookieUploadFailed;

    public MNGameCookiesProvider (MNSession session)
     {
      this.session = session;

      session.PluginMessageReceived += OnSessionPluginMessageReceived;
     }

    public void Shutdown ()
     {
      session.PluginMessageReceived -= OnSessionPluginMessageReceived;
     }

    public void DownloadUserCookie (int key)
     {
      session.SendPluginMessage(PROVIDER_NAME,'g' + key.ToString() + ":0");
     }

    public void UploadUserCookie (int key, string cookie)
     {
      if (cookie != null)
       {
        if (cookie.Length <= COOKIE_DATA_MAX_LENGTH)
         {
          session.SendPluginMessage
           (PROVIDER_NAME,'p' + key.ToString() + ":0:" + cookie);
         }
        else
         {
          DispatchCookieUploadFailedEvent(key,"game cookie data length exceeds allowed limit");
         }
       }
      else
       {
        session.SendPluginMessage(PROVIDER_NAME,'d' + key.ToString() + ":0");
       }
     }

    private void HandleGetResponse (string[] components)
     {
      if (components.Length < 3)
       {
        return;
       }

      int key;
      int requestNumber;

      if (!int.TryParse(components[0],out key))
       {
        return;
       }

      if (!int.TryParse(components[1],out requestNumber))
       {
        return;
       }

      if (requestNumber != REQUEST_NUMBER_API)
       {
        return;
       }

      string data = components.Length < 4 ? null : components[3];

      if (components[2] == RESPONSE_STATUS_OK)
       {
        DispatchCookieDownloadSucceededEvent(key,data);
       }
      else if (components[2] == RESPONSE_STATUS_ERROR)
       {
        DispatchCookieDownloadFailedEvent(key,data);
       }
     }

    private void HandlePutResponse (string[] components)
     {
      if (components.Length < 3)
       {
        return;
       }

      int key;
      int requestNumber;

      if (!int.TryParse(components[0],out key))
       {
        return;
       }

      if (!int.TryParse(components[1],out requestNumber))
       {
        return;
       }

      if (requestNumber != REQUEST_NUMBER_API)
       {
        return;
       }

      if (components[2] == RESPONSE_STATUS_OK)
       {
        DispatchCookieUploadSucceededEvent(key);
       }
      else if (components[2] == RESPONSE_STATUS_ERROR)
       {
        string message = components.Length < 4 ? null : components[3];

        DispatchCookieUploadFailedEvent(key,message);
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

      char cmd = message[0];

      if (cmd == REQUEST_CMD_GET)
       {
        HandleGetResponse(MNUtils.StringSplitWithLimit(message.Substring(1),':',4));
       }
      else if (cmd == REQUEST_CMD_PUT || cmd == REQUEST_CMD_DEL)
       {
        HandlePutResponse(MNUtils.StringSplitWithLimit(message.Substring(1),':',4));
       }
     }

    private void DispatchCookieDownloadSucceededEvent (int key, string cookie)
     {
      GameCookieDownloadSucceededEventHandler handler = GameCookieDownloadSucceeded;

      if (handler != null)
       {
        handler(key,cookie);
       }
     }

    private void DispatchCookieDownloadFailedEvent (int key, string error)
     {
      GameCookieDownloadFailedEventHandler handler = GameCookieDownloadFailed;

      if (handler != null)
       {
        handler(key,error);
       }
     }

    private void DispatchCookieUploadSucceededEvent (int key)
     {
      GameCookieUploadSucceededEventHandler handler = GameCookieUploadSucceeded;

      if (handler != null)
       {
        handler(key);
       }
     }

    private void DispatchCookieUploadFailedEvent (int key, string error)
     {
      GameCookieUploadFailedEventHandler handler = GameCookieUploadFailed;

      if (handler != null)
       {
        handler(key,error);
       }
     }

    private MNSession session;

    private const string PROVIDER_NAME = "com.playphone.mn.guc";

    private const char REQUEST_CMD_GET = 'g';
    private const char REQUEST_CMD_PUT = 'p';
    private const char REQUEST_CMD_DEL = 'd';

    private const string RESPONSE_STATUS_OK    = "s";
    private const string RESPONSE_STATUS_ERROR = "e";

    private const int REQUEST_NUMBER_API = 0;

    private const int COOKIE_DATA_MAX_LENGTH = 1024;
   }
 }

