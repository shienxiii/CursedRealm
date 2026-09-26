using System;
using System.Collections.Generic;
using ProceduralInterior;
using ProceduralInterior.FunctionLib;
using UnityEngine;

namespace ProceduralInterior
{
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

        [ContextMenu("Spawn Level")]
        public void SpawnLevel()
        {
            if (Settings == null || _random == null) return;

            List<Interior> prefabs = Settings.InteriorPrefabs;
            if (prefabs == null || prefabs.Count == 0) return;

            // Spawning single interior for now
            int i = _random.Next(0, prefabs.Count);
            SamplerHelperFunctions.GetRandomElement(prefabs, _random);
            Interior prefab = prefabs[i];

            Interior newInterior = SpawnInterior(prefab, Vector3.zero);
            newInterior.SeedInterior(_random.Next());
            newInterior.GenerateInterior(0);

        }

        private Interior SpawnInterior(Interior inPrefab, Vector3 inPosition)
        {
            if (inPrefab == null || _random == null) return null;

            // apply random 90-degree rotation
            float yRotate = (_random.Next() % 4) * 90.0f;
            
            Interior newInterior = Instantiate(inPrefab, inPosition, Quaternion.Euler(0.0f, yRotate, 0.0f));

            return newInterior;
        }

        public void ReSeed()
        {
            int seed = unchecked((int)DateTime.Now.Ticks);
            _random = new System.Random(seed);
        }
    }
}