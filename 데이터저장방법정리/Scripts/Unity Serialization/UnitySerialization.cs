using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class UnitySerialization : MonoBehaviour
{
    [SerializeField] Text mainText;
    [SerializeField] Text saveText;

    [SerializeField] PlayerData playerData = new PlayerData("직렬화 저장");

    private void Start()
    {
        OnLoad();

        Debug.Log("path: " + Application.persistentDataPath);
        Debug.Log("path: " + Application.dataPath);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerData.level += 1;
        }

        playerData.playTime += Time.deltaTime;

        mainText.text = string.Format("닉네임: {0} \n레벨: {1}\n플레이시간: {2}",
            playerData.name, playerData.level, playerData.playTime);
    }

    public void OnSave()
    {
        using (FileStream fsW = new FileStream(Application.persistentDataPath + "/UnitySerialization.dat", FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fsW, playerData);

            saveText.text = string.Format("닉네임: {0} \n레벨: {1}\n플레이시간: {2}",
            playerData.name, playerData.level, playerData.playTime);
        }
    }

    public void OnLoad()
    {
        if (!File.Exists(Application.persistentDataPath + "UnitySerialization.dat"))
            return;

        using(FileStream fwR = new FileStream(Application.persistentDataPath + "/UnitySerialization.dat", FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            playerData = bf.Deserialize(fwR) as PlayerData;

            saveText.text = string.Format("닉네임: {0} \n레벨: {1}\n플레이시간: {2}",
            playerData.name, playerData.level, playerData.playTime);
        }
    }
}
