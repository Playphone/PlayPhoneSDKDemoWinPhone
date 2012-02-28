//
//  MNWSXmlGenericItemListParser.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Xml;
using PlayPhone.MultiNet.Core.WS.Data;
using System.Collections.Generic;

namespace PlayPhone.MultiNet.Core.WS.Parser
 {
  public class MNWSXmlGenericItemListParser<E> : IMNWSXmlDataParser where E : MNWSGenericItem, new()
   {
    public MNWSXmlGenericItemListParser (string itemTagName)
     {
      this.itemTagName = itemTagName;
     }

    public override object ParseElement (XmlReader reader)
     {
      MNWSXmlGenericItemParser<E> itemDataParser = new MNWSXmlGenericItemParser<E>();
      List<E>                     list           = new List<E>();

      reader.Read();
      reader.MoveToContent();

      while (reader.NodeType == XmlNodeType.Element)
       {
        if (reader.Name == itemTagName)
         {
          list.Add((E)itemDataParser.ParseElement(reader));
         }

        reader.Read();
        reader.MoveToContent();
       }

      return list;
     }

    private string itemTagName;
   }
 }
