//
//  MNWSResponse.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core.WS
 {
  public class MNWSResponse
   {
    public MNWSResponse ()
     {
      blocks = new Dictionary<string,object>();
     }

    public object GetDataForBlock (string blockName)
     {
      try
       {
        return blocks[blockName];
       }
      catch (KeyNotFoundException)
       {
        return null;
       }
     }

    internal void AddBlock (string name, object data)
     {
      blocks[name] = data;
     }

    private Dictionary<string,object> blocks;
   }
 }
