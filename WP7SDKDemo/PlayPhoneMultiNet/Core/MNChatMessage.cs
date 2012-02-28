//
//  MNChatMessage.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

namespace PlayPhone.MultiNet.Core
 {
  public class MNChatMessage
   {
    public MNUserInfo Sender    { get; set; }
    public string     Message   { get; set; }
    public bool       IsPrivate { get; set; }

    public MNChatMessage (MNUserInfo sender, string message, bool isPrivate)
     {
      Sender    = sender;
      Message   = message;
      IsPrivate = isPrivate;
     }
   }
 }
