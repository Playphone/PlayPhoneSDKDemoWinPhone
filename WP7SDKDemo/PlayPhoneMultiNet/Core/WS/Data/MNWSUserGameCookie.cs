//
//  MNWSUserGameCookie.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSUserGameCookie : MNWSGenericItem
   {
    public long? GetUserId ()
     {
      return GetLongValue("user_id");
     }

    public int? GetCookieKey ()
     {
      return GetIntValue("cookie_key");
     }

    public string GetCookieValue ()
     {
      return GetValueByName("cookie_value");
     }
   }
 }
