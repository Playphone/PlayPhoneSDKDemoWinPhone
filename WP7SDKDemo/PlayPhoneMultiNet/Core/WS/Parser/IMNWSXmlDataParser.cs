using System.Xml;

namespace PlayPhone.MultiNet.Core.WS.Parser
 {
  public abstract class IMNWSXmlDataParser
   {
    public abstract object ParseElement (XmlReader reader);
   }
 }
