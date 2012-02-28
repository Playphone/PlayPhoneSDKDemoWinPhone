//
//  MNXmlTools.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Xml;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core
 {
  public static class MNXmlTools
   {
    public static void ReaderReadToContent (XmlReader reader)
     {
      reader.Read(); reader.MoveToContent();
     }

    public static void ReaderSkipChildrenToContent (XmlReader reader)
     {
      reader.Skip(); reader.MoveToContent();
     }

    public static bool SeekElementByPath (XmlReader reader, string[] path)
     {
      int pathLen = path.Length;

      if (pathLen == 0)
       {
        return false;
       }

      ReaderReadToContent(reader);

      if (reader.NodeType != XmlNodeType.Element || reader.Name != path[0])
       {
        return false;
       }

      bool ok = true;

      for (int i = 1; i < pathLen && ok; i++)
       {
        ReaderReadToContent(reader);

        while (reader.NodeType == XmlNodeType.Element && reader.Name != path[i])
         {
          ReaderSkipChildrenToContent(reader);
         }

        if (reader.NodeType != XmlNodeType.Element)
         {
          ok = false;
         }
       }

      return ok;
     }

    public static List<Dictionary<string,string>> ParseItemList (XmlReader reader, string itemTagName)
     {
      List<Dictionary<string,string>> result = new List<Dictionary<string,string>>();

      ReaderReadToContent(reader);

      while (reader.NodeType != XmlNodeType.EndElement)
       {
        if (reader.NodeType == XmlNodeType.Element && reader.Name == itemTagName)
         {
          Dictionary<string,string> itemData = new Dictionary<string,string>();

          ReaderReadToContent(reader);

          while (reader.NodeType != XmlNodeType.EndElement)
           {
            if (reader.NodeType == XmlNodeType.Element)
             {
              string key   = reader.Name;
              string value = reader.ReadInnerXml().Trim();

              itemData[key] = value;
             }

            ReaderReadToContent(reader);
           }
          
          result.Add(itemData);
         }

        ReaderReadToContent(reader);
       }

      return result;
     }
   }
 }
