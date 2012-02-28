//
//  MNOfflineScores.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

namespace PlayPhone.MultiNet.Core
 {
  public static class MNOfflineScores
   {
    public static bool SaveScore (MNVarStorage varStorage, long userId, int gameSetId, long score)
     {
      string varNamePrefix   = GetVarNamePrefix(userId,gameSetId);
      string scoreStr        = score.ToString();
      string timeStr         = MNUtils.GetUnixTime().ToString();
      string minScoreVarName = varNamePrefix + "min.score";
      string maxScoreVarName = varNamePrefix + "max.score";

      string minScoreString = varStorage.GetValue(minScoreVarName);
      string maxScoreString = varStorage.GetValue(maxScoreVarName);

      long? minScore = minScoreString == null ? null : MNUtils.ParseLong(minScoreString);
      long? maxScore = maxScoreString == null ? null : MNUtils.ParseLong(maxScoreString);

      if (minScore == null || score <= minScore)
       {
        varStorage.SetValue(minScoreVarName,scoreStr);
        varStorage.SetValue(varNamePrefix + "min.date",timeStr);
       }

      if (maxScore == null || score >= maxScore)
       {
        varStorage.SetValue(maxScoreVarName,scoreStr);
        varStorage.SetValue(varNamePrefix + "max.date",timeStr);
       }

      varStorage.SetValue(varNamePrefix + "last.score",scoreStr);
      varStorage.SetValue(varNamePrefix + "last.date",timeStr);

      return true;
     }

    private static string GetVarNamePrefix (long userId, int gameSetId)
     {
      return "offline." + userId.ToString() + ".score_pending." + gameSetId.ToString() + ".";
     }
   }
 }
