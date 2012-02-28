//
//  MNWSXmlGenericItemParser.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Xml;
using PlayPhone.MultiNet.Core.WS.Data;

namespace PlayPhone.MultiNet.Core.WS.Parser
 {
  public class MNWSXmlGenericItemParser<E> : IMNWSXmlDataParser where E : MNWSGenericItem,new()
   {
    public override object ParseElement (XmlReader reader)
     {
      MNWSGenericItem item = new E();

      reader.Read();
      reader.MoveToContent();

      while (reader.NodeType == XmlNodeType.Element)
       {
        item.PutValue(reader.Name,reader.ReadInnerXml().Trim());

        reader.Read();
        reader.MoveToContent();
       }

      return item;
     }
   }
 }
