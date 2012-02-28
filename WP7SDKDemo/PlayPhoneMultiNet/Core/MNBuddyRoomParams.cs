//
//  MNBuddyRoomParams.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

namespace PlayPhone.MultiNet.Core
 {
  public class MNBuddyRoomParams
   {
    public string  RoomName       { get; set; }
    public int     GameSetId      { get; set; }
    public string  ToUserIdList   { get; set; }
    public string  ToUserSFIdList { get; set; }
    public string  InviteText     { get; set; }

    public MNBuddyRoomParams (string  roomName,
                              int     gameSetId,
                              string  toUserIdList,
                              string  toUserSFIdList,
                              string  inviteText)
     {
      RoomName       = roomName;
      GameSetId      = gameSetId;
      ToUserIdList   = toUserIdList;
      ToUserSFIdList = toUserSFIdList;
      InviteText     = inviteText;
     }
   }
 }
