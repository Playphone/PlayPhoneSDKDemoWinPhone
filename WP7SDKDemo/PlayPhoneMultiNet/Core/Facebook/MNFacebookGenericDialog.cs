//
//  MNFacebookGenericDialog.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Windows.Navigation;
using System.Collections.Generic;

using Microsoft.Phone.Controls;

namespace PlayPhone.MultiNet.Core.Facebook
 {
  public class MNFacebookGenericDialog : MNFacebookDialog
   {
    public MNFacebookGenericDialog (string action, string appId, string accessToken,  Dictionary<string,string> _params = null)
     {
      this.appId       = appId;
      this.accessToken = accessToken;
      this.action      = action;
      this._params     = _params ?? new Dictionary<string,string>();
     }

    public void SetParam (string name, string value)
     {
      if (value != null)
       {
        _params[name] = value;
       }
      else
       {
        _params.Remove(name);
       }
     }

    protected override Uri GetDialogUri ()
     {
      Dictionary<string,string> queryParams = new Dictionary<string,string>(_params);

      queryParams["display"]      = "touch";
      queryParams["redirect_uri"] = REDIRECT_URL;
      queryParams["app_id"]       = appId;

      if (accessToken != null)
       {
        queryParams["access_token"] = accessToken;
       }

      return new Uri(FACEBOOK_DIALOG_ENDPOINT_URL + action + "?" + MNUtils.HttpGetRequestBuildParamsString(queryParams));
     }

    protected override void OnBrowserNavigating (NavigatingEventArgs e)
     {
      if (e.Uri.AbsolutePath.Contains("login_success.html"))
       {
        e.Cancel = true;

        DismissDialogWithResult(new DialogResult(this));
       }
     }

    private string                    appId;
    private string                    accessToken;
    private string                    action;
    private Dictionary<string,string> _params;

    private const string FACEBOOK_DIALOG_ENDPOINT_URL = "https://m.facebook.com/dialog/";
    private const string REDIRECT_URL                 = "http://www.facebook.com/connect/login_success.html";
   }
 }
