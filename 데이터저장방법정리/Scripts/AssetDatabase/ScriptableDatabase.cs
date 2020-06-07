using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEditor;

public class ScriptableDatabase : MonoBehaviour
{
    string DATA_PATH;

    [SerializeField] Text mainText;
    [SerializeField] Text saveText;

    [SerializeField] PlayerData playerData = new PlayerData("에셋데이터");
    PlayerDataForScriptable saveData;

    private void Start()
    {
        DATA_PATH = "Assets/Resources/Data/AssetData.asset";

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
        
    }

    public void OnLoad()
    {
        saveData = Resources.Load<PlayerDataForScriptable>("Data/AssetData");
        if (saveData == null)
        {
            CreateAsset();
            return;
        }

        playerData = saveData.playerData;
    }

    void CreateAsset()
    {
        Debug.Log("create");
        saveData = ScriptableObject.CreateInstance<PlayerDataForScriptable>();
        saveData.playerData = playerData;

        AssetDatabase.CreateAsset(saveData, DATA_PATH);
        AssetDatabase.SaveAssets();
    }
}
