using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XmlSerialization : MonoBehaviour
{
    string DATA_PATH; 

    [SerializeField] Text mainText;
    [SerializeField] Text saveText;

    PlayerDataForXML playerData = new PlayerDataForXML();
    PlayerDataContainer dataContainer = new PlayerDataContainer();

    private void Start()
    {
        DATA_PATH = Application.persistentDataPath + "/xmlData.xml";

        OnLoad();
        Debug.Log("path: " + DATA_PATH);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerData.level += 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerDataForXML saveData = new PlayerDataForXML();
            saveData.name = playerData.name;
            saveData.level = playerData.level;
            saveData.playTime = playerData.playTime;
            dataContainer.playerDatas.Add(saveData);
        }

        playerData.playTime += Time.deltaTime;

        mainText.text = string.Format("닉네임: {0} \n레벨: {1}\n플레이시간: {2}",
            playerData.name, playerData.level, playerData.playTime);
    }

    public void OnSave()
    {
        dataContainer.Save(DATA_PATH);

        saveText.text = string.Format("닉네임: {0} \n레벨: {1}\n플레이시간: {2}",
            playerData.name, playerData.level, playerData.playTime);
    }

    public void OnLoad()
    {
        PlayerDataContainer loadData = dataContainer.Load(DATA_PATH);
        if (loadData != null)
        {
            playerData = loadData.playerDatas[0];

            saveText.text = string.Format("닉네임: {0} \n레벨: {1}\n플레이시간: {2}",
            playerData.name, playerData.level, playerData.playTime);
        }
    }
}
