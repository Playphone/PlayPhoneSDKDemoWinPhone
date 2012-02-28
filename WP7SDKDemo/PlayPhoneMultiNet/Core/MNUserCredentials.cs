//
//  MNUserCredentials.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public class MNUserCredentials
   {
    public long      userId          { get; set; }
    public string    userName        { get; set; }
    public string    userAuthSign    { get; set; }
    public DateTime? lastLoginTime   { get; set; }
    public string    userAuxInfoText { get; set; }

    public MNUserCredentials (long userId, string userName, string userAuthSign, DateTime? lastLoginTime, string userAuxInfoText)
     {
      this.userId          = userId;
      this.userName        = userName;
      this.userAuthSign    = userAuthSign;
      this.lastLoginTime   = lastLoginTime;
      this.userAuxInfoText = userAuxInfoText;
     }

    private static Dictionary<long,MNUserCredentials> GetAllCredentials (MNVarStorage varStorage)
     {
      Dictionary<string,string> variables;

      lock (thisLock)
       {
        variables = varStorage.GetVariablesByMask("cred.*");
       }

      Dictionary<long,MNUserCredentials> users = new Dictionary<long,MNUserCredentials>();

      foreach (var varData in variables)
       {
        string[] varNameParts = varData.Key.Split('.');

        if (varNameParts.Length == 3)
         {
          long? userId = MNUtils.ParseLong(varNameParts[1]);

          if (userId != null)
           {
            MNUserCredentials credentials = null;

            if (!users.TryGetValue(userId.Value,out credentials))
             {
              credentials = new MNUserCredentials(userId.Value,null,null,null,null);

              users[(long)userId] = credentials;
             }

            string fieldName = varNameParts[2];

            if      (fieldName == "user_id")
             {
              /* skip var */
             }
            else if (fieldName == "user_name")
             {
              credentials.userName = varData.Value;
             }
            else if (fieldName == "user_auth_sign")
             {
              credentials.userAuthSign = varData.Value;
             }
            else if (fieldName == "user_last_login_time")
             {
              long? ticks = MNUtils.ParseLong(varData.Value);

              if (ticks != null)
               {
                credentials.lastLoginTime = new DateTime(ticks.Value);
               }
             }
            else if (fieldName == "user_aux_info_text")
             {
              credentials.userAuxInfoText = varData.Value;
             }
           }
         }
       }

      return users;
     }

    public static
    MNUserCredentials[] LoadAllCredentials (MNVarStorage varStorage)
     {
      Dictionary<long,MNUserCredentials> users = GetAllCredentials(varStorage);

      MNUserCredentials[] result = new MNUserCredentials[users.Values.Count];

      users.Values.CopyTo(result,0);

      return result;
     }

    public static void WipeCredentialsByUserId (MNVarStorage varStorage, long userId)
     {
      lock (thisLock)
       {
        varStorage.RemoveVariablesByMask("cred." + userId.ToString() + ".*");
       }
     }

    public static void WipeAllCredentials (MNVarStorage varStorage)
     {
      lock (thisLock)
       {
        varStorage.RemoveVariablesByMask("cred.*");
       }
     }

    public static void UpdateCredentials (MNVarStorage varStorage, MNUserCredentials newCredentials)
     {
      long userId = newCredentials.userId;
      string prefix = "cred." + userId.ToString() + ".";

      lock (thisLock)
       {
        varStorage.SetValue(prefix + "user_id",userId.ToString());

        if (newCredentials.userName != null)
         {
          varStorage.SetValue(prefix + "user_name",newCredentials.userName);
         }

        if (newCredentials.userAuthSign != null)
         {
          varStorage.SetValue(prefix + "user_auth_sign",newCredentials.userAuthSign);
         }

        if (newCredentials.lastLoginTime != null)
         {
          varStorage.SetValue(prefix + "user_last_login_time",newCredentials.lastLoginTime.Value.Ticks.ToString());
         }

        if (newCredentials.userAuxInfoText != null)
         {
          varStorage.SetValue(prefix + "user_aux_info_text",newCredentials.userAuxInfoText);
         }
       }
     }

    public static
    MNUserCredentials GetMostRecentlyLoggedUserCredentials (MNVarStorage varStorage)
     {
      Dictionary<long,MNUserCredentials> allCredentials = GetAllCredentials(varStorage);
      DateTime? mostRecentLoginDate = null;
      MNUserCredentials mostRecentCredentials = null;

      foreach (var entry in allCredentials)
       {
        DateTime? entryLoginTime = entry.Value.lastLoginTime;

        if (entry.Value.lastLoginTime != null)
         {
          if (mostRecentLoginDate == null || entryLoginTime.Value > mostRecentLoginDate)
           {
            mostRecentLoginDate   = entryLoginTime;
            mostRecentCredentials = entry.Value;
           }
         }
       }

      return mostRecentCredentials;
     }

    public static
    MNUserCredentials GetCredentialsByUserId (MNVarStorage varStorage, long userId)
     {
      Dictionary<long,MNUserCredentials> users = GetAllCredentials(varStorage);
      MNUserCredentials result;

      if (users.TryGetValue(userId,out result))
       {
        return result;
       }
      else
       {
        return null;
       }
     }

    private static object thisLock = new object();
   }
 }
