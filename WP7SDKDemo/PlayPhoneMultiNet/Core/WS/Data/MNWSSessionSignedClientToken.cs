//
//  MNWSSessionSignedClientToken.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSSessionSignedClientToken : MNWSGenericItem
   {
    public string GetClientTokenBody ()
     {
      return GetValueByName("client_token_body");
     }

    public string GetClientTokenSign ()
     {
      return GetValueByName("client_token_sign");
     }
   }
 }
