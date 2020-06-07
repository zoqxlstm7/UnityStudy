using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml.Serialization;
using System.IO;
using System.Text;

[XmlRoot("PlayerDataCollection")]
public class PlayerDataContainer
{
    [XmlArray("PlayerDatas"), XmlArrayItem("PlayerData")]
    public List<PlayerDataForXML> playerDatas = new List<PlayerDataForXML>();

    public void Save(string path)
    {
        using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate), Encoding.UTF8))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerDataContainer));
            serializer.Serialize(sw, this);

            Debug.Log("save");
        }
    }

    public PlayerDataContainer Load(string path)
    {
        if (!File.Exists(path))
            return null;

        using(FileStream fwR = new FileStream(path, FileMode.Open))
        {
            Debug.Log("load");
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerDataContainer));
            return serializer.Deserialize(fwR) as PlayerDataContainer;
        }
    }
}
