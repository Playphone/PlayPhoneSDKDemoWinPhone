//
//  MNAppHostCallEventArgs.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;

namespace PlayPhone.MultiNet.Core
 {
  public class MNAppHostCallEventArgs
   {
    public MNAppHostCallInfo AppHostCallInfo
     {
      get
       {
        return appHostCallInfo;
       }
     }

    public bool IsCancelled
     {
      get
       {
        return cancelled;
       }
     }

    public MNAppHostCallEventArgs (MNAppHostCallInfo appHostCallInfo)
     {
      this.appHostCallInfo = appHostCallInfo;
      this.cancelled       = false;
     }

    public void Cancel ()
     {
      cancelled = true;
     }

    private MNAppHostCallInfo appHostCallInfo;
    private bool              cancelled;
   }
 }
