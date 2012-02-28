//
//  MNTrustedHosts.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public class MNTrustedHosts
   {
    public MNTrustedHosts ()
     {
      trustedHosts = new Dictionary<string,bool>();
     }

    public void AddHost (string host)
     {
      lock (thisLock)
       {
        trustedHosts[host]= true;
       }
     }

    public void RemoveHost (string host)
     {
      lock (thisLock)
       {
        trustedHosts.Remove(host);
       }
     }

    public void Clear ()
     {
      lock (thisLock)
       {
        trustedHosts.Clear();
       }
     }

    public bool IsTrusted (string host)
     {
      lock (thisLock)
       {
        return trustedHosts.ContainsKey(host);
       }
     }

    private Dictionary<string,bool> trustedHosts; // WP7 does not support HashSet
    private object thisLock = new object();
   }
 }
