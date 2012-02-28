//
//  MNFacebookPublishDialog.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.Facebook
 {
  public class MNFacebookPublishDialog : MNFacebookGenericDialog
   {
    public MNFacebookPublishDialog (string appId, string accessToken, string message, string attachment,
                                    string actionLinks, string targetId)
           : base("stream.publish",appId,accessToken)
     {
      SetParam("message",message);
      SetParam("attachment",attachment);
      SetParam("action_links",actionLinks);
      SetParam("target_id",targetId);
     }
   }
 }
