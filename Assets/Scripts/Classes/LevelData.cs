using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class LevelConfig
{
    public List<LevelData> levels;

    public static LevelConfig FromJson(string json)
    {
        return JsonUtility.FromJson<LevelConfig>(json);
    }
}

[Serializable]
public class LevelData
{
    public string name;
    public List<WaveData> waves;
}

[Serializable]
public class WaveData
{
    public List<EnemyData> enemies;
}

[Serializable]
public class EnemyData
{
    public string type;
    public int count;
    public float maxDelay;
}