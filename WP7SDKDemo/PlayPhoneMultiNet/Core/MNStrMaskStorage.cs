//
//  MNStrMaskStorage.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  class MNStrMaskStorage
   {
    public MNStrMaskStorage ()
     {
      masks = new Dictionary<string,bool>();
     }

    public void AddMask (string mask)
     {
      lock (thisLock)
       {
        masks[mask] = true;
       }
     }

    public void RemoveMask (string mask)
     {
      lock (thisLock)
       {
        masks.Remove(mask);
       }
     }

    public bool CheckString (string str)
     {
      lock (thisLock)
       {
        foreach (var entry in masks)
          {
           if (CheckString(entry.Key,str))
            {
             return true;
            }
          }
       }

      return false;
     }

    private static bool CheckString (string mask, string str)
     {
      if (mask.EndsWith(WILDCARD_STR))
       {
        return str.StartsWith(mask.Substring(0,mask.Length - 1));
       }
      else
       {
        return mask == str;
       }
     }

    private const string WILDCARD_STR = "*";

    private Dictionary<string,bool> masks;
    private object thisLock = new object();
   }
 }
