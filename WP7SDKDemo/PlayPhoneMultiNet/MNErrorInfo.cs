//
//  MNErrorInfo.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet
 {
  public class MNErrorInfo
   {
    public MNErrorInfo (int actionCode, string errorMessage)
     {
      this.ActionCode   = actionCode;
      this.ErrorMessage = errorMessage;
     }

    public int    ActionCode   { get; set; }
    public string ErrorMessage { get; set; }

    public const int ACTION_CODE_UNDEFINED             = 0;

    public const int ACTION_CODE_LOGIN                 = 11;
    public const int ACTION_CODE_CONNECT               = 12;

    public const int ACTION_CODE_FB_CONNECT            = 21;
    public const int ACTION_CODE_FB_RESUME             = 22;

    public const int ACTION_CODE_POST_GAME_RESULT      = 51;

    public const int ACTION_CODE_JOIN_GAME_ROOM        = 101;
    public const int ACTION_CODE_CREATE_BUDDY_ROOM     = 102;

    public const int ACTION_CODE_LEAVE_ROOM            = 111;

    public const int ACTION_CODE_SET_USER_STATUS       = 121;

    public const int ACTION_CODE_START_BUDDY_ROOM_GAME = 151;
    public const int ACTION_CODE_STOP_ROOM_GAME        = 152;

    public const int ACTION_CODE_LOAD_CONFIG           = 401;

    public const int ACTION_CODE_OTHER_MIN_VALUE       = 1001;
   }
 }
