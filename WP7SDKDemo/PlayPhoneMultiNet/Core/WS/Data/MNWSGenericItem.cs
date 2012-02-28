using System;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSGenericItem
   {
    public MNWSGenericItem ()
     {
      data = new Dictionary<string,string>();
     }

    public string GetValueByName (string name)
     {
      try
       {
        return data[name];
       }
      catch (KeyNotFoundException)
       {
        return null;
       }
     }

    public void PutValue (string name, string value)
     {
      data[name] = value;
     }

    public int? GetIntValue (string name)
     {
      return MNUtils.ParseInt(GetValueByName(name));
     }

    public uint? GetUIntValue (string name)
     {
      return MNUtils.ParseUInt(GetValueByName(name));
     }

    public long? GetLongValue (string name)
     {
      return MNUtils.ParseLong(GetValueByName(name));
     }

    public bool? GetBooleanValue (String name)
     {
      string val = GetValueByName(name);

      if (val == null)
       {
        return null;
       }
      else if (val == "true")
       {
        return true;
       }
      else if (val == "false")
       {
        return false;
       }
      else
       {
        return null;
       }
     }

    private Dictionary<string,string> data;
   }
 }
