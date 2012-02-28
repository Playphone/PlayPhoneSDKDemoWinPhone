//
//  MNJoinRoomInvitationParams.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

namespace PlayPhone.MultiNet.Core
 {
  public class MNJoinRoomInvitationParams
   {
    public int    FromUserSFId  { get; set; }
    public string FromUserName  { get; set; }
    public int    RoomSFId      { get; set; }
    public string RoomName      { get; set; }
    public int    RoomGameId    { get; set; }
    public int    RoomGameSetId { get; set; }
    public string InviteText    { get; set; }

    public MNJoinRoomInvitationParams (int fromUserSFId, string fromUserName,
                                       int roomSFId, string roomName,
                                       int roomGameId, int roomGameSetId,
                                       string inviteText)
     {
      FromUserSFId  = fromUserSFId;
      FromUserName  = fromUserName;
      RoomSFId      = roomSFId;
      RoomName      = roomName;
      RoomGameId    = roomGameId;
      RoomGameSetId = roomGameSetId;
      InviteText    = inviteText;
     }
   }
 }
