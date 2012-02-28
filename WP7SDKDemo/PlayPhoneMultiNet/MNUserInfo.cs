//
//  MNUserInfo.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

namespace PlayPhone.MultiNet
 {
  public class MNUserInfo
   {
    public long   UserId   { get; set; }
    public int    UserSFId { get; set; }
    public string UserName { get; set; }
    public string UserAvatarUrl
     {
      get
       {
        return webBaseUrl == null ? null : webBaseUrl + AVATAR_URL_SUFFIX + UserId.ToString();
       }
     }

    public MNUserInfo ()
     {
      UserId     = MNConst.MN_USER_ID_UNDEFINED;
      UserSFId   = MNConst.MN_USER_SFID_UNDEFINED;
      UserName   = null;
      webBaseUrl = null;
     }

    public MNUserInfo (long userId, int userSFId, string userName, string webBaseUrl)
     {
      UserId          = userId;
      UserSFId        = userSFId;
      UserName        = userName;
      this.webBaseUrl = webBaseUrl;
     }

    private string webBaseUrl;

    private const string AVATAR_URL_SUFFIX = "/user_image_data.php?sn_id=0&user_id=";
   }
 }
