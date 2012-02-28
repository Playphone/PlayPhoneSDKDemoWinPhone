//
//  MNFacebookPermissionsDialog.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.Facebook
 {
  public class MNFacebookPermissionsDialog : MNFacebookGenericDialog
   {
    public MNFacebookPermissionsDialog (string appId, string accessToken, string permissions)
           : base("permissions.request",appId,accessToken)
     {
      SetParam("perms",permissions);
     }
   }
 }
