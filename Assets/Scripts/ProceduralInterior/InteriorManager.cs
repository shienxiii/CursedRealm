using System;
using System.Collections.Generic;
using ProceduralInterior;
using UnityEngine;

public class InteriorManager : MonoBehaviour
{
    private static InteriorManager _instance = null;
    private static ProceduralInteriorSettings _settings = null;
    public static System.Random _random = null;
    private static SerializableDictionary<Interior, int> _interiorMap;

    public static InteriorManager Instance => _instance;
    public static ProceduralInteriorSettings Settings => _settings;
    
    private void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        
        _instance = this;
        _settings = ProceduralInteriorSettings.TryGetSettings();
        ReSeed();
    }

    [ContextMenu("Spawn Interior")]
    public void SpawnLevel()
    {
        if (Settings == null) return;

        List<Interior> prefabs = Settings.InteriorPrefabs;
        if (prefabs == null || prefabs.Count == 0) return;
        
        // Spawning single interior for now
        int i = _random.Next(0, prefabs.Count);
        GameObject prefab = prefabs[i].gameObject;
        SpawnInterior(prefab, Vector3.zero);
    }

    private GameObject SpawnInterior(GameObject inPrefab, Vector3 inPosition)
    {
        if (inPrefab == null) return null;

        GameObject newInterior = Instantiate(inPrefab, inPosition, Quaternion.identity);

        return newInterior;
    }

    public void ReSeed()
    {
        int seed = unchecked((int)DateTime.Now.Ticks);
        _random = new System.Random(seed);
    }
}
