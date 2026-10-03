using System;
using System.Collections.Generic;
using ProceduralInterior;
using ProceduralInterior.FunctionLib;
using ProceduralInterior.Types;
using UnityEngine;

namespace ProceduralInterior
{
    public class InteriorManager : MonoBehaviour
    {
        private static InteriorManager _instance = null;
        private static ProceduralInteriorSettings _settings = null;
        private Dictionary<System.Guid, int> _interiorIndex;
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

        [ContextMenu("Initialize")]
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
            CacheInteriorSettingsIndex();
        }

        [ContextMenu("Spawn Level")]
        public void SpawnLevel()
        {
            Debug.Log("Spawn Level called");
            if (!Settings || _random == null) return;
            Debug.Log("Initialization check complete");
            List<InteriorSettings> options = Settings.Interior;
            
            if (options == null || options.Count == 0) return;
            Debug.Log("Options found");

            // Spawning single interior for now
            InteriorSettings option = SamplerHelperFunctions.GetRandomElement(options, _random);
            Interior prefab = option.Prefabs;

            if (!prefab) return;

            Interior newInterior = InteriorSpawner.SpawnInterior(prefab, _random, Vector3.zero);
            newInterior.SeedInterior(_random.Next());
            newInterior.SampleInterior(0);
            InteriorSpawner.SpawnStructures(newInterior);
        }

        public void ReSeed()
        {
            int seed = unchecked((int)DateTime.Now.Ticks);
            _random = new System.Random(seed);
        }

        private void CacheInteriorSettingsIndex()
        {
            // We want to cache the Interior assets to the index that corresponds to InteriorSettings._settings
            if(!_settings)
                _settings = ProceduralInteriorSettings.TryGetSettings();

            if (_interiorIndex == null)
                _interiorIndex = new Dictionary<Guid, int>();
            else
                _interiorIndex.Clear();

            List<InteriorSettings> intSettings = _settings.Interior;

            for (int i = 0; i < intSettings.Count; i++)
                _interiorIndex.Add(intSettings[i].Prefabs.AssetID, i);
        }
    }
}