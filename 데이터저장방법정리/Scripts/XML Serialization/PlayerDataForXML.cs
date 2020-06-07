//using System.Xml;
using System.Xml.Serialization;

public class PlayerDataForXML
{
    [XmlAttribute("name")]
    public string name = "xml 저장";
    public int level;
    public float playTime;
}