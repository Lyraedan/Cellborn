using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootTable : MonoBehaviour
{
    public List<Loot> tables = new List<Loot>();

    void Start()
    {
        int selected = Random.Range(0, tables.Count);
        var table = tables[selected];

        float spawnChance = Random.Range(0, table.chancesOfSpawningLoot);
        bool doSpawn = Mathf.RoundToInt(spawnChance) == 0;

        if (doSpawn)
        {
            table.SpawnRandomPrefab(this);
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Instantiate(prefab, position, rotation);
    }
}

[System.Serializable]
public class LootPrefab
{
    public GameObject prefab;
    public Vector3 rotation = Vector3.zero;
}

[System.Serializable]
public class Loot
{
    public Transform spawnpoint;
    [Tooltip("The higher this value. The less likely loot is to spawn")] public float chancesOfSpawningLoot = 1;
    public List<LootPrefab> possiblePrefabs = new List<LootPrefab>();

    public GameObject SpawnPrefab(int index, LootTable table)
    {
        var loot = possiblePrefabs[index];
        return table.Spawn(loot.prefab, spawnpoint.position, Quaternion.Euler(loot.rotation));
    }

    public GameObject SpawnRandomPrefab(LootTable table)
    {
        int index = Random.Range(0, possiblePrefabs.Count);
        return SpawnPrefab(index, table);
    }
}