using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string name;
    public int level;
    public float playTime;

    public PlayerData(string name) => this.name = name;
}
