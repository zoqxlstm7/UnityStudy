using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;
using System.Text;

public class JsonSerialization : MonoBehaviour
{
    string DATA_PATH;

    [SerializeField] Text mainText;
    [SerializeField] Text saveText;

    [SerializeField] PlayerData playerData = new PlayerData("JSON 저장");

    private void Start()
    {
        DATA_PATH = Application.persistentDataPath + "/jsonData.json";

        OnLoad();
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
        using(StreamWriter sw = new StreamWriter(new FileStream(DATA_PATH, FileMode.OpenOrCreate), Encoding.UTF8))
        {
            string json = JsonUtility.ToJson(playerData);
            sw.Write(json);

            saveText.text = string.Format("닉네임: {0} \n레벨: {1}\n플레이시간: {2}",
            playerData.name, playerData.level, playerData.playTime);
        }
    }

    public void OnLoad()
    {
        if (!File.Exists(DATA_PATH))
            return;

        using(StreamReader sr = new StreamReader(new FileStream(DATA_PATH, FileMode.Open), Encoding.UTF8))
        {
            string json = sr.ReadToEnd();
            playerData = JsonUtility.FromJson<PlayerData>(json);
        }
    }
}
