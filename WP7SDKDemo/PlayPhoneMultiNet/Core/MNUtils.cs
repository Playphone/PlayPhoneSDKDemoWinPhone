//
//  MNUtils.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Text;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public static class MNUtils
   {
    public static long? ParseLong (string s)
     {
      if (s == null)
       {
        return null;
       }

      long result;

      if (long.TryParse(s,out result))
       {
        return result;
       }
      else
       {
        return null;
       }
     }

    public static int? ParseInt (string s)
     {
      if (s == null)
       {
        return null;
       }

      int result;

      if (int.TryParse(s,out result))
       {
        return result;
       }
      else
       {
        return null;
       }
     }

    public static uint? ParseUInt (string s)
     {
      if (s == null)
       {
        return null;
       }

      uint result;

      if (uint.TryParse(s,out result))
       {
        return result;
       }
      else
       {
        return null;
       }
     }

    public static int[] ParseCSInts (String s)
     {
      if (s == null)
       {
        return null;
       }

      string[] elements = s.Split(',');
      int[]    result   = new int[elements.Length];

      for (int index = 0; index < elements.Length; index++)
       {
        int? value = ParseInt(elements[index]);

        if (value != null)
         {
          result[index] = value.Value;
         }
        else
         {
          return null;
         }
       }

      return result;
     }

    public static long[] ParseCSLongs (String s)
     {
      if (s == null)
       {
        return null;
       }

      string[] elements = s.Split(',');
      long[]   result   = new long[elements.Length];

      for (int index = 0; index < elements.Length; index++)
       {
        long? value = ParseLong(elements[index]);

        if (value != null)
         {
          result[index] = value.Value;
         }
        else
         {
          return null;
         }
       }

      return result;
     }

    public static long GetUnixTime ()
     {
      return (long)DateTime.Now.Subtract(epoch).TotalSeconds;
     }

    public static long GetUnitTime (DateTime time)
     {
      return (long)time.Subtract(new DateTime(1970,1,1)).TotalSeconds;
     }

    public static V DictReadValue<K,V> (IDictionary<K,V> dict, K key)
     {
      V value;

      if (dict.TryGetValue(key, out value))
       {
        return value;
       }
      else
       {
        return default(V);
       }
     }

    public static string StringGetMD5String (string s)
     {
      return ByteArrayToHexString(MNMD5.CalculateHash(Encoding.UTF8.GetBytes(s)));
     }

// there is no Split(char[],int) method in Silverlight and Windows Phone,
// here is simple implementation
#if WINDOWS_PHONE
    public static string[] StringSplitWithLimit (string s, char ch, int limit)
     {
      List<string> parts = new List<string>();
      int pos      = 0;
      int count    = 1;
      int splitPos = 0;

      while (count < limit && splitPos >= 0)
       {
        splitPos = s.IndexOf(ch,pos);

        if (splitPos >= 0)
         {
          parts.Add(s.Substring(pos,splitPos - pos));

          pos = splitPos + 1;

          count++;
         }
       }

      if (pos <= s.Length)
       {
        parts.Add(s.Substring(pos));
       }

      return parts.ToArray();
     }
#endif

    public static string ByteArrayToHexString (byte[] data)
     {
      StringBuilder builder = new StringBuilder();

      for (int i = 0; i < data.Length; i++)
       {
        builder.Append(data[i].ToString("x2"));
       }

      return builder.ToString();
     }

    private static readonly string[][] JSStringEscapePairs = new [] {
     new [] { "\\",   "\\\\" },
     new [] { "\r",   "\\r"  },
     new [] { "\n",   "\\n"  },
     new [] { "\t",   "\\t"  },
     new [] { "\"", "\\x22"  },
     new [] { "\'", "\\x27"  },
     new [] {  "&", "\\x26"  },
     new [] {  "<", "\\x3C"  },
     new [] {  ">", "\\x3E"  }
    };

    public static string StringAsJSString (string str)
     {
      if (str == null)
       {
        return "null";
       }

      StringBuilder result = new StringBuilder(str);

      foreach (var pair in JSStringEscapePairs)
       {
        result.Replace(pair[0],pair[1]);
       }

      result.Insert(0,"'");
      result.Append("'");

      return result.ToString();
     }

    public static string StringEscapeCharSimple
                          (string str, char charToEscape, char escapeChar)
     {
      int charToEscapeCode = (int)charToEscape;

      return str.Replace(charToEscape.ToString(),
                         escapeChar.ToString() + String.Format("{0:2X}",charToEscapeCode));
     }

    public static string StringEscapeSimple
                          (string str, char charToEscape, char escapeChar)
     {
      return StringEscapeCharSimple
              (StringEscapeCharSimple(str,escapeChar,escapeChar),
               charToEscape,
               escapeChar);
     }

    public static string StringUnEscapeCharSimple
                          (String str, char charToEscape, char escapeChar)
     {
      int charToEscapeCode = (int)charToEscape;

      return str.Replace(escapeChar.ToString() + String.Format("{0:2X}",charToEscapeCode),
                         charToEscape.ToString());
     }

    public static string StringUnEscapeSimple
                          (string str, char charToEscape, char escapeChar)
     {
      return StringUnEscapeCharSimple
              (StringUnEscapeCharSimple(str,charToEscape,escapeChar),
               escapeChar,
               escapeChar);
     }

    public static String MakeGameSecretByComponents (uint secret1, uint secret2, uint secret3, uint secret4)
     {
      return String.Format("{0:x8}-{1:x8}-{2:x8}-{3:x8}",secret1,secret2,secret3,secret4);
     }

    public static bool ParseMNUserName (string mnUserName, out string shortUserName, out long userId)
     {
      shortUserName = null;
      userId        = MNConst.MN_USER_ID_UNDEFINED;

      if (mnUserName == null)
       {
        return false;
       }

      int length = mnUserName.Length;
      int pos    = length - 1;

      if (pos < 0)
       {
        return false;
       }

      if (mnUserName[pos] != ']')
       {
        return false;
       }

      long tempId = 0;
      long factor = 1;

      for (pos = pos - 1; pos >= 0 && Char.IsDigit(mnUserName,pos); pos--)
       {
        tempId += factor * (long)Char.GetNumericValue(mnUserName,pos);
        factor *= 10;
       }

      if (tempId == 0)
       {
        return false;
       }

      if (pos < 0 || mnUserName[pos] != '[')
       {
        return false;
       }

      pos--;

      if (pos < 0 || mnUserName[pos] != ' ')
       {
        return false;
       }

      shortUserName = mnUserName.Substring(0,pos);
      userId        = tempId;

      return true;
     }

    public static Dictionary<string,string> HttpGetRequestParseParams (string queryParams)
     {
      Dictionary<string,string> result = new Dictionary<string,string>();

      queryParams = queryParams.Trim();

      if (queryParams.Length > 0)
       {
        string[] parts = queryParams.Replace('+',' ').Split('&');

        foreach (string param in parts)
         {
          int index = param.IndexOf('=');

          if (index >= 0)
           {
            string name  = param.Substring(0,index);
            string value = param.Substring(index + 1);

            result.Add(Uri.UnescapeDataString(name),Uri.UnescapeDataString(value));
           }
          else
           {
            result.Add(Uri.UnescapeDataString(param),"");
           }
         }
       }

      return result;
     }

    public static string HttpGetRequestBuildParamsString (Dictionary<string,string> queryParams)
     {
      MNStringJoiner paramString = new MNStringJoiner("&");

      foreach (var param in queryParams)
       {
        paramString.Join(Uri.EscapeUriString(param.Key) + "=" + Uri.EscapeUriString(param.Value));
       }

      return paramString.ToString();
     }

    private static DateTime epoch = new DateTime(1970,1,1);
   }

  public class MNStringJoiner
   {
    public MNStringJoiner (string joinString)
     {
      this.joinString = joinString;
      empty           = true;
      str             = new StringBuilder();
     }

    public void Join (string str)
     {
      if (!empty)
       {
        this.str.Append(joinString);
       }
      else
       {
        empty = false;
       }

      this.str.Append(str);
     }

    public override string ToString ()
     {
      return this.str.ToString();
     }

    private string        joinString;
    private bool          empty;
    private StringBuilder str;
   }
 }
