//
//  MNFacebookLoginDialog.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;

using Microsoft.Phone.Controls;

using Facebook;

namespace PlayPhone.MultiNet.Core.Facebook
 {
  public class MNFacebookLoginDialog : MNFacebookDialog
   {
    public string AccessToken { get; private set; }

    public MNFacebookLoginDialog (string appId, string[] permissions)
     {
      AccessToken      = null;
      this.appId       = appId;
      this.permissions = permissions;
     }

    protected override Uri GetDialogUri ()
     {
      Dictionary<string,object> _params = new Dictionary<string,object>();

      _params["response_type"] = "token";
      _params["display"]       = "touch";

      if (permissions != null && permissions.Length > 0)
       {
        _params["scope"] = string.Join(",",permissions);
       }

      FacebookOAuthClient oAuthClient = new FacebookOAuthClient();

      oAuthClient.AppId = appId;

      return oAuthClient.GetLoginUrl(_params);
     }

    protected override void OnBrowserNavigated (NavigationEventArgs e)
     {
      FacebookOAuthResult oAuthResult;

      if (FacebookOAuthResult.TryParse(e.Uri,out oAuthResult))
       {
        DialogResult result = new DialogResult(this);

        if (oAuthResult.IsSuccess)
         {
          AccessToken = oAuthResult.AccessToken;
         }
        else
         {
          result.SetError(oAuthResult.ErrorDescription);
         }

        DismissDialogWithResult(result);
       }
     }

    private string   appId;
    private string[] permissions;
   }
 }
