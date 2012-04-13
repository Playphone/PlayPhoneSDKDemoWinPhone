//
//  MNAppBeaconResponse.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core
 {
  public class MNAppBeaconResponse
   {
    public string ResponseText  { get; private set; }
    public long   CallSeqNumber { get; private set; }

    public MNAppBeaconResponse (string responseText, long callSeqNumber)
     {
      ResponseText  = responseText;
      CallSeqNumber = callSeqNumber;
     }
   }
 }
