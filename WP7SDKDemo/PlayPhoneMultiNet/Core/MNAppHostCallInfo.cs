//
//  MNAppHostCallInfo.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public class MNAppHostCallInfo
   {
    public const string CommandConnect                      = "apphost_connect.php";
    public const string CommandReconnect                    = "apphost_reconnect.php";
    public const string CommandGoBack                       = "apphost_goback.php";
    public const string CommandLogout                       = "apphost_logout.php";
    public const string CommandSendPrivateMessage           = "apphost_sendmess.php";
    public const string CommandSendPublicMessage            = "apphost_chatmess.php";
    public const string CommandJoinBuddyRoom                = "apphost_joinbuddyroom.php";
    public const string CommandJoinAutoRoom                 = "apphost_joinautoroom.php";
    public const string CommandPlayGame                     = "apphost_playgame.php";
    public const string CommandLoginFacebook                = "apphost_sn_facebook_login.php";
    public const string CommandResumeFacebook               = "apphost_sn_facebook_resume.php";
    public const string CommandLogoutFacebook               = "apphost_sn_facebook_logout.php";
    public const string CommandShowFacebookPublishDialog    = "apphost_sn_facebook_dialog_publish_show.php";
    public const string CommandShowFacebookPermissionDialog = "apphost_sn_facebook_dialog_permission_req_show.php";
    public const string CommandImportAddressBook            = "apphost_do_user_ab_import.php";
    public const string CommandGetAddressBookData           = "apphost_get_user_ab_data.php";
    public const string CommandNewBuddyRoom                 = "apphost_newbuddyroom.php";
    public const string CommandStartRoomGame                = "apphost_start_room_game.php";
    public const string CommandGetContext                   = "apphost_get_context.php";
    public const string CommandGetRoomUserList              = "apphost_get_room_userlist.php";
    public const string CommandGetGameResults               = "apphost_get_game_results.php";
    public const string CommandLeaveRoom                    = "apphost_leaveroom.php";
    public const string CommandImportUserPhoto              = "apphost_do_photo_import.php";
    public const string CommandSetRoomUserStatus            = "apphost_set_room_user_status.php";
    public const string CommandNavBarShow                   = "apphost_navbar_show.php";
    public const string CommandNavBarHide                   = "apphost_navbar_hide.php";
    public const string CommandScriptEval                   = "apphost_script_eval.php";
    public const string CommandWebViewReload                = "apphost_webview_reload.php";
    public const string CommandVarSave                      = "apphost_var_save.php"; // cannot be intercepted via mnSessionAppHostCallReceived:
    public const string CommandVarsClear                    = "apphost_vars_clear.php"; // cannot be intercepted via mnSessionAppHostCallReceived:
    public const string CommandVarsGet                      = "apphost_vars_get.php"; // cannot be intercepted via mnSessionAppHostCallReceived:
    public const string CommandVoid                         = "apphost_void.php";
    public const string CommandSetHostParam                 = "apphost_set_host_param.php"; // cannot be intercepted via mnSessionAppHostCallReceived:
    public const string CommandPluginMessageSubscribe       = "apphost_plugin_message_subscribe.php";
    public const string CommandPluginMessageUnSubscribe     = "apphost_plugin_message_unsubscribe.php";
    public const string CommandPluginMessageSend            = "apphost_plugin_message_send.php";
    public const string CommandSendHttpRequest              = "apphost_http_request.php"; // cannot be intercepted via mnSessionAppHostCallReceived:
    public const string CommandSetGameResults               = "apphost_set_game_results.php";
    public const string CommandExecUICommand                = "apphost_exec_ui_command.php";
    public const string CommandAddSourceDomain              = "apphost_add_source_domain.php"; // cannot be intercepted via mnSessionAppHostCallReceived:
    public const string CommandRemoveSourceDomain           = "apphost_remove_source_domain.php"; // cannot be intercepted via mnSessionAppHostCallReceived:
    public const string CommandAppIsInstalledQuery          = "apphost_app_is_installed.php";
    public const string CommandAppTryLaunch                 = "apphost_app_try_launch.php";
    public const string CommandAppShowInMarket              = "apphost_app_show_in_market.php";

    public string CommandName
     {
      get
       {
        return commandName;
       }
     }

    public IDictionary<string,string> CommandParams
     {
      get
       {
        return commandParams;
       }
     }

    public MNAppHostCallInfo (string commandName, IDictionary<string,string> commandParams)
     {
      this.commandName   = commandName;
      this.commandParams = commandParams;
     }

    private string                     commandName;
    private IDictionary<string,string> commandParams;
   }
 }
