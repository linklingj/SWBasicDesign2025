using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> mapPrefabs;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        int randIdx = Random.Range(0, mapPrefabs.Count);
        
        Instantiate(mapPrefabs[randIdx], transform);
    }
}
