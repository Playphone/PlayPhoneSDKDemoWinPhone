//
//  MNServerInfoProvider.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using PlayPhone.MultiNet.Core;

namespace PlayPhone.MultiNet.Providers
 {
  public class MNServerInfoProvider
   {
    public const int SERVER_TIME_INFO_KEY = 1;

    public delegate void ServerInfoItemReceivedEventHandler      (int key, string value);
    public delegate void ServerInfoItemRequestFailedEventHandler (int key, string error);

    public event ServerInfoItemReceivedEventHandler      ServerInfoItemReceived;
    public event ServerInfoItemRequestFailedEventHandler ServerInfoItemRequestFailed;

    public MNServerInfoProvider (MNSession session)
     {
      this.session = session;

      session.PluginMessageReceived += OnSessionPluginMessageReceived;
     }

    public void Shutdown ()
     {
      session.PluginMessageReceived -= OnSessionPluginMessageReceived;
     }

    public void RequestServerInfoItem (int key)
     {
      session.SendPluginMessage
       (PROVIDER_NAME,
        'g' + key.ToString() + ':' + REQUEST_NUMBER_API.ToString());
     }

    private void HandleGetResponse (string message)
     {
      string[] components = MNUtils.StringSplitWithLimit(message,':',4);

      if (components.Length < 4)
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

      if      (components[2] == RESPONSE_STATUS_OK)
       {
        ServerInfoItemReceivedEventHandler handler = ServerInfoItemReceived;

        if (handler != null)
         {
          handler(key,components[3]);
         }
       }
      else if (components[2] == RESPONSE_STATUS_ERROR)
       {
        ServerInfoItemRequestFailedEventHandler handler = ServerInfoItemRequestFailed;

        if (handler != null)
         {
          handler(key,components[3]);
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

      char cmd = message[0];

      if (cmd == REQUEST_CMD_GET)
       {
        HandleGetResponse(message.Substring(1));
       }
     }

    private MNSession session;

    private const char   REQUEST_CMD_GET       = 'g';
    private const string RESPONSE_STATUS_OK    = "s";
    private const string RESPONSE_STATUS_ERROR = "e";

    private const int REQUEST_NUMBER_API = 0;

    private const string PROVIDER_NAME = "com.playphone.mn.si";
   }
 }

