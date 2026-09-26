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
    public float startDelay;
    public SpawnGroup[] spawnGroups;
}

[Serializable]

public class WaveConfig
{
    public WaveData[] waves;
}