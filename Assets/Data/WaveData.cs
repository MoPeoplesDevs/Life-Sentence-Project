using System;

[Serializable]

public class SpawnGroup
{
    public string enemyType;
    public int count;
    public float spawnInterval;
}

[Serializable]

public class WaveData
{
    public int waveNumber;
    public SpawnGroup[] spawnGroups;
}

[Serializable]

public class WaveConfig
{
    public WaveData[] waves;
}