//
//  MNSocNetSessionFB.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using PlayPhone.MultiNet.Core.Facebook;
using System.IO;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public class MNSocNetSessionFB
   {
    public const int SOCNET_ID = 1;

    public string AccessToken { get; private set; }

    public MNSocNetSessionFB ()
     {
      appId       = null;
      AccessToken = null;
     }

    public void SetAppId (string appId)
     {
      this.appId = appId;

      RestoreAccessToken();
     }

    public void Authorize (string[] permissions, MNFacebookDialog.DialogEventHandler eventHandler)
     {
      if (appId == null)
       {
        eventHandler(BuildAppIdUndefinedDialogResult());

        return;
       }

      MNFacebookLoginDialog loginDialog = new MNFacebookLoginDialog(appId,permissions);

      loginDialog.DialogCompleted += (result) =>
       {
        if (result.Succeeded)
         {
          StoreAccessToken(loginDialog.AccessToken);
         }

        eventHandler(result);
       };

      loginDialog.Show();
     }

    public void Resume (MNFacebookDialog.DialogEventHandler eventHandler)
     {
      if (appId == null)
       {
        eventHandler(BuildAppIdUndefinedDialogResult());

        return;
       }

      MNFacebookDialog.DialogResult result = new MNFacebookDialog.DialogResult(null);

      if (!IsAccessTokenValid())
       {
        result.SetError("Facebook connection failed (session cannot be resumed)");
       }

      eventHandler(result);
     }

    public void Logout ()
     {
      if (appId != null)
       {
        RemoveAccessToken();
       }
     }

    public bool IsAuthorized ()
     {
      return AccessToken != null;
     }

    public void ShowStreamDialog (string prompt, string attachement, string targetId, string actionLinks, MNFacebookDialog.DialogEventHandler eventHandler)
     {
      if (appId == null)
       {
        eventHandler(BuildAppIdUndefinedDialogResult());

        return;
       }

      MNFacebookPublishDialog publishDialog = new MNFacebookPublishDialog(appId,AccessToken,prompt,attachement,actionLinks,targetId);

      publishDialog.DialogCompleted += eventHandler;

      publishDialog.Show();
     }

    public void ShowPermissionDialog (string permissions, MNFacebookDialog.DialogEventHandler eventHandler)
     {
      if (appId == null)
       {
        eventHandler(BuildAppIdUndefinedDialogResult());

        return;
       }

      MNFacebookPermissionsDialog permissionsDialog = new MNFacebookPermissionsDialog(appId,AccessToken,permissions);

      permissionsDialog.DialogCompleted += eventHandler;

      permissionsDialog.Show();
     }

    public void ShowGenericDialog (string action, Dictionary<string,string> _params, MNFacebookDialog.DialogEventHandler eventHandler)
     {
      if (appId == null)
       {
        eventHandler(BuildAppIdUndefinedDialogResult());

        return;
       }

      MNFacebookGenericDialog dialog = new MNFacebookGenericDialog(action,appId,AccessToken,_params);

      dialog.DialogCompleted += eventHandler;

      dialog.Show();
     }

    private void RestoreAccessToken ()
     {
      AccessToken = null;

      using (Stream tokenFileStream = MNPlatformWinPhone.GetDataFileReadStream(TOKEN_FILE_NAME))
       {
        if (tokenFileStream != null)
         {
          try
           {
            using (StreamReader reader = new StreamReader(tokenFileStream))
             {
              AccessToken = reader.ReadToEnd();
             }
           }
          catch (Exception)
           {
            // ignore any io or memory errors
           }
         }
       }
     }

    private void StoreAccessToken (string accessToken)
     {
      AccessToken = accessToken;

      using (Stream tokenFileStream = MNPlatformWinPhone.GetDataFileWriteStream(TOKEN_FILE_NAME))
       {
        if (tokenFileStream != null)
         {
          try
           {
            using (StreamWriter writer = new StreamWriter(tokenFileStream))
             {
              writer.Write(AccessToken);
             }
           }
          catch (Exception)
           {
            // ignore any io or memory errors
           }
         }
       }
     }

    private void RemoveAccessToken ()
     {
      MNPlatformWinPhone.DeleteDataFile(TOKEN_FILE_NAME);

      AccessToken = null;
     }

    private bool IsAccessTokenValid ()
     {
      return AccessToken != null;
     }

    private MNFacebookDialog.DialogResult BuildAppIdUndefinedDialogResult ()
     {
      MNFacebookDialog.DialogResult result = new MNFacebookDialog.DialogResult(null);

      result.SetError("Facebook application id is undefined");

      return result;
     }

    private string appId;

    private const string TOKEN_FILE_NAME = "fbsession.dat";
   }
 }
