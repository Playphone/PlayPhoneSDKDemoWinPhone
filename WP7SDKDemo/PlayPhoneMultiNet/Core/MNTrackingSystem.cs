//
//  MNTrackingSystem.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public class MNTrackingSystem
   {
    public delegate void AppBeaconResponseReceivedEventHandler (MNAppBeaconResponse response);

    public event AppBeaconResponseReceivedEventHandler AppBeaconResponseReceived;

    public IDictionary<string,string> TrackingVars
     {
      get
       {
        return trackingVars;
       }
     }

    public MNTrackingSystem (MNSession session)
     {
      beaconUrlTemplate   = null;
      shutdownUrlTemplate = null;
      launchTracked       = false;
      installTracker      = new InstallTracker();
      trackingVars        = SetupTrackingVars(session);
     }

    public void TrackLaunch (string urlTemplate, MNSession session)
     {
      lock (thisLock)
       {
        if (launchTracked)
         {
          return;
         }
        else
         {
          launchTracked = true;
         }
       }

      UrlTemplate launchUrlTemplate = new UrlTemplate(urlTemplate,trackingVars);

      launchUrlTemplate.SendLaunchTrackingRequest(session);
     }

    public void TrackInstallWithUrlTemplate (string urlTemplate, MNSession session)
     {
      installTracker.SetUrlTemplate(urlTemplate,trackingVars,session);
     }

    public void SetShutdownUrlTemplate (string urlTemplate)
     {
      lock (thisLock)
       {
        shutdownUrlTemplate = new UrlTemplate(urlTemplate,trackingVars);
       }
     }

    public void TrackShutdown (MNSession session)
     {
      lock (thisLock)
       {
        if (shutdownUrlTemplate != null)
         {
          shutdownUrlTemplate.SendShutdownTrackingRequest(session);
         }
       }
     }

    public void SetBeaconUrlTemplate (string urlTemplate)
     {
      lock (thisLock)
       {
        beaconUrlTemplate = new UrlTemplate(urlTemplate,trackingVars);
       }
     }

    public void SendBeacon (string beaconAction, string beaconData, long beaconCallSeqNumber, MNSession session)
     {
      lock (thisLock)
       {
        if (beaconUrlTemplate != null)
         {
          beaconUrlTemplate.SendBeacon(beaconAction,beaconData,beaconCallSeqNumber,session,OnAppBeaconResponseReceived);
         }
       }
     }

    private void OnAppBeaconResponseReceived (MNAppBeaconResponse response)
     {
      AppBeaconResponseReceivedEventHandler handler = AppBeaconResponseReceived;

      if (handler != null)
       {
        handler(response);
       }
     }

    private static string GetCountryCodeByCultureName (string cultureName)
     {
      //culture name has the following format: languagecode-country/regioncode

      int lastPos  = cultureName.IndexOf('/');
      int startPos = cultureName.IndexOf('-') + 1; // if not found -1 turns into zero, exactly what we need

      if (lastPos < 0)
       {
        lastPos = cultureName.Length;
       }

      return cultureName.Substring(startPos,lastPos - startPos);
     }

    private static void SetupTrackingVar (Dictionary<string,string> vars, string name, string value)
     {
      if (value == null)
       {
        return;
       }

      vars[name] = value;
      vars["@" + name] = MNUtils.StringGetMD5String(value);
     }

    private static Dictionary<string,string> SetupTrackingVars (MNSession session)
     {
      Dictionary<string,string> vars = new Dictionary<string,string>();
      string cultureName = System.Globalization.CultureInfo.CurrentCulture.Name;
      string countryCode = GetCountryCodeByCultureName(cultureName);

      SetupTrackingVar(vars,"tv_udid",session.GetUniqueAppId());
//      SetupTrackingVar(vars,"tv_udid2"],"");
//      SetupTrackingVar(vars,"tv_device_name","");
      SetupTrackingVar(vars,"tv_device_type",MNPlatformWinPhone.GetDeviceModel());
      SetupTrackingVar(vars,"tv_os_version",Environment.OSVersion.Version.ToString());
      SetupTrackingVar(vars,"tv_country_code",countryCode);
      SetupTrackingVar(vars,"tv_language_code",System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
      SetupTrackingVar(vars,"mn_game_id",session.GetGameId().ToString());
      SetupTrackingVar(vars,"mn_dev_type", MNPlatformWinPhone.GetDeviceType().ToString());
      SetupTrackingVar(vars,"mn_dev_id",MNUtils.StringGetMD5String(session.GetUniqueAppId()));
      SetupTrackingVar(vars,"mn_client_ver",MNSession.CLIENT_API_VERSION);
      SetupTrackingVar(vars,"mn_client_locale",cultureName);
      SetupTrackingVar(vars,"mn_app_ver_ext",MNPlatformWinPhone.GetAppVerExternal());
      SetupTrackingVar(vars,"mn_app_ver_int",MNPlatformWinPhone.GetAppVerInternal());

      SetupTrackingVar(vars,"mn_launch_time",session.GetLaunchTime().ToString());
      SetupTrackingVar(vars,"mn_launch_id",session.GetLaunchId());
      SetupTrackingVar(vars,"mn_install_id",session.GetInstallId());
      SetupTrackingVar(vars,"mn_tz_info",((int)TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds).ToString() + "+*+");

      return vars;
     }

    private class UrlTemplate
     {
      public UrlTemplate (string urlTemplate, Dictionary<string,string> trackingVars)
       {
        ParseTemplate(urlTemplate,trackingVars);
       }

      public void SendLaunchTrackingRequest (MNSession session)
       {
        SendBeacon(null,null,session);
       }

      public void SendInstallTrackingRequest (MNSession session, AppBeaconResponseReceivedEventHandler responseHandler)
       {
        SendBeacon(null,null,0,session,responseHandler);
       }

      public void SendShutdownTrackingRequest (MNSession session)
       {
        SendBeacon(null,null,session);
       }

      private void AddDynamicVars (MNStringJoiner joiner, List<DynamicVar> vars, string value)
       {
        if (vars == null)
         {
          return;
         }

        if (value == null)
         {
          value = "";
         }

        string hashedValue = null;

        foreach (DynamicVar var in vars)
         {
          if (var.UseHashedValue)
           {
            if (hashedValue == null)
             {
              hashedValue = MNUtils.StringGetMD5String(value);
             }

            joiner.Join(var.Name + '=' + Uri.EscapeUriString(hashedValue));
           }
          else
           {
            joiner.Join(var.Name + '=' + Uri.EscapeUriString(value));
           }
         }
       }

      public void SendBeacon (string beaconAction, string beaconData, MNSession session)
       {
        SendBeacon(beaconAction,beaconData,0,session,null);
       }

      public void SendBeacon (string beaconAction, string beaconData, long requestNumber,
                              MNSession session, AppBeaconResponseReceivedEventHandler responseHandler)
       {
        string postBody;

        postBody = postBodyStringBuilder.ToString();

        if (userIdVars == null && userSIdVars == null &&
            beaconActionVars == null && beaconDataVars == null)
         {
         }
        else
         {
          MNStringJoiner dynamicVarsJoiner = new MNStringJoiner("&");

          long   userId     = session.GetMyUserId();
          string userSIdStr = session.GetMySId();

          string userIdStr = userId != MNConst.MN_USER_ID_UNDEFINED ? userId.ToString() : "";

          AddDynamicVars(dynamicVarsJoiner,userIdVars,userIdStr);
          AddDynamicVars(dynamicVarsJoiner,userSIdVars,userSIdStr);
          AddDynamicVars(dynamicVarsJoiner,beaconActionVars,beaconAction);
          AddDynamicVars(dynamicVarsJoiner,beaconDataVars,beaconData);

          if (postBody.Length > 0)
           {
            postBody = postBody + '&' + dynamicVarsJoiner.ToString();
           }
          else
           {
            postBody = dynamicVarsJoiner.ToString();
           }
         }

        string fullUrl = postBody.Length > 0 ? url + '?' + postBody : url;

        MNURLStringDownloader downloader = new MNURLStringDownloader();
        downloader.LoadUrl(new Uri(fullUrl),
                           (responseText) =>
                             {
                              if (responseHandler != null)
                               {
                                responseHandler(new MNAppBeaconResponse(responseText,requestNumber));
                               }
                             },
                           (httpStatus,message) =>
                             {
                              if (responseHandler != null)
                               {
                                responseHandler(new MNAppBeaconResponse(null,requestNumber));
                               }
                             });
       }

      private void ParseTemplate (string urlTemplate, Dictionary<string,string> trackingVars)
       {
        int    pos;
        string paramString;

        pos = urlTemplate.IndexOf('?');

        if (pos >= 0)
         {
          url         = urlTemplate.Substring(0,pos);
          paramString = urlTemplate.Substring(pos + 1);
         }
        else
         {
          url         = urlTemplate;
          paramString = "";
         }

        postBodyStringBuilder = new MNStringJoiner("&");

        userIdVars       = null;
        userSIdVars      = null;
        beaconActionVars = null;
        beaconDataVars   = null;

        if (paramString.Length > 0)
         {
          string[] parameters = paramString.Split('&');

          foreach (string param in parameters)
           {
            string name;
            string value;

            pos = param.IndexOf('=');

            if (pos >= 0)
             {
              name  = param.Substring(0,pos);
              value = param.Substring(pos + 1);
             }
            else
             {
              name  = param;
              value = "";
             }

            string metaVarName    = GetMetaVarName(value);
            bool   useHashedValue = metaVarName != null && metaVarName.StartsWith("@");
            string dynVarName     = useHashedValue ? metaVarName.Substring(1) : metaVarName;

            if (metaVarName != null)
             {
              value = MNUtils.DictReadValue(trackingVars,metaVarName);

              if      (value != null)
               {
                postBodyStringBuilder.Join(name + '=' + Uri.EscapeUriString(value));
               }
              else if (dynVarName == "mn_user_id")
               {
                if (userIdVars == null)
                 {
                  userIdVars = new List<DynamicVar>();
                 }

                userIdVars.Add(new DynamicVar(name,useHashedValue));
               }
              else if (dynVarName == "mn_user_sid")
               {
                if (userSIdVars == null)
                 {
                  userSIdVars = new List<DynamicVar>();
                 }

                userSIdVars.Add(new DynamicVar(name,useHashedValue));
               }
              else if (dynVarName == "bt_beacon_action_name")
               {
                if (beaconActionVars == null)
                 {
                  beaconActionVars = new List<DynamicVar>();
                 }

                beaconActionVars.Add(new DynamicVar(name,useHashedValue));
               }
              else if (dynVarName == "bt_beacon_data")
               {
                if (beaconDataVars == null)
                 {
                  beaconDataVars = new List<DynamicVar>();
                 }

                beaconDataVars.Add(new DynamicVar(name,useHashedValue));
               }
              else
               {
                postBodyStringBuilder.Join(name + '=');
               }
             }
            else
             {
              postBodyStringBuilder.Join(name + '=' + value);
             }
           }
         }
       }

      private static string GetMetaVarName (string str)
       {
        if (str.StartsWith("{") && str.EndsWith("}"))
         {
          return str.Substring(1,str.Length - 2);
         }
        else
         {
          return null;
         }
       }

      private string           url;
      private MNStringJoiner   postBodyStringBuilder;
      private List<DynamicVar> userIdVars;
      private List<DynamicVar> userSIdVars;
      private List<DynamicVar> beaconActionVars;
      private List<DynamicVar> beaconDataVars;

      private class DynamicVar
       {
        public string Name           { get; private set; }
        public bool   UseHashedValue { get; private set; }

        public DynamicVar (string name, bool useHashedValue)
         {
          Name           = name;
          UseHashedValue = useHashedValue;
         }
       }
     }

    private class InstallTracker
     {
      public InstallTracker ()
       {
        session               = null;
        requestInProgress     = false;
        installAlreadyTracked = false;
       }

      public void SetUrlTemplate (string urlTemplate, Dictionary<string,string> trackingVars, MNSession session)
       {
        lock (thisLock)
         {
          if (requestInProgress || installAlreadyTracked)
           {
            return;
           }

          string timestamp = session.VarStorageGetValueForVariable(RESPONSE_TIMESTAMP_VARNAME);

          if (timestamp != null)
           {
            installAlreadyTracked = true;

            return;
           }

          requestInProgress = true;
          this.session      = session;

          UrlTemplate installUrlTemplate = new UrlTemplate(urlTemplate,trackingVars);

          installUrlTemplate.SendInstallTrackingRequest(session,OnAppBeaconResponseReceived);
         }
       }

      private void OnAppBeaconResponseReceived (MNAppBeaconResponse response)
       {
        lock (thisLock)
         {
          string responseText = response.ResponseText;

          if (responseText != null)
           {
            if (session != null)
             {
              session.VarStorageSetValue(RESPONSE_TIMESTAMP_VARNAME,MNUtils.GetUnixTime().ToString());
              session.VarStorageSetValue(RESPONSE_TEXT_VARNAME,Uri.EscapeUriString(responseText));

              installAlreadyTracked = true;
             }
           }

          requestInProgress = false;
          session           = null;
         }
       }

      private MNSession session;
      private bool      requestInProgress;
      private bool      installAlreadyTracked;
      private object    thisLock = new object();

      private const string RESPONSE_TIMESTAMP_VARNAME = "app.install.track.done";
      private const string RESPONSE_TEXT_VARNAME      = "app.install.track.response";
     }

    private bool                      launchTracked;
    private Dictionary<string,string> trackingVars;
    private UrlTemplate               beaconUrlTemplate;
    private UrlTemplate               shutdownUrlTemplate;
    private InstallTracker            installTracker;

    private object thisLock = new object();
   }
 }
