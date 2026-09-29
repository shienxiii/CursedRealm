using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProceduralInterior
{
    public class ProceduralInteriorSettings : ScriptableObject
    {
        [Header("Assets")]
        [SerializeField]private List<Interior> _interiorPrefabs;
        [SerializeField]private RoomPrefab _defaultGround;
        [SerializeField]private RoomPrefab _defaultWall;
        [SerializeField]private RoomPrefab _defaultDoorWall;
        [SerializeField]private RoomPrefab _defaultCeiling;

        [Header("Sampler Settings")]
        [SerializeField] private Vector2 _sampleDimension = new Vector2(2.0f, 3.0f);
        [SerializeField] private int _minRoomCount = 5;
        [SerializeField] private int _maxRoomCount = 10;
        [SerializeField] private int _roomSizeOffset = 5;
        [SerializeField] private int _minSamplesPerRoom = 4;
        [SerializeField] private bool _offsetWallNormal = false;
        [SerializeField] private bool _offsetWallEndToConnection = true;
        [SerializeField] private float _offsetLength = 0.075f;

        public List<Interior> InteriorPrefabs => _interiorPrefabs;
        public RoomPrefab DefaultGround => _defaultGround;
        public RoomPrefab DefaultWall => _defaultWall;
        public RoomPrefab DefaultDoorWall => _defaultDoorWall;
        public RoomPrefab DefaultCeiling => _defaultCeiling;
        
        public Vector2 SampleDimension => _sampleDimension;
        public int MinRoomCount => _minRoomCount;
        public int MaxRoomCount => _maxRoomCount;
        public int RoomSizeOffset => _roomSizeOffset;
        public int MinSamplesPerRoom => _minSamplesPerRoom;
        public bool OffsetWallNormal => _offsetWallNormal;
        public bool OffsetWallEndToConnection => _offsetWallEndToConnection;
        public float OffsetLength => _offsetLength;

        #region ProjectSettings
        private const string _subDir = "Settings/";
        private const string _asset = "ProceduralInteriorSettings";

        public static ProceduralInteriorSettings TryGetSettings()
        {
            ProceduralInteriorSettings setting = (ProceduralInteriorSettings)Resources.Load(_subDir + _asset);
            return setting;
        }
        #endregion

#if (UNITY_EDITOR)
        private const string _directory = "Assets/Resources/";
        public const string Path = _directory + _subDir + _asset + ".asset";

        internal static ProceduralInteriorSettings GetOrCreateSettings()
        {
            ProceduralInteriorSettings settings;

            if (!File.Exists(Path))
            {
                if (!Directory.Exists(_directory + _subDir))
                    Directory.CreateDirectory(_directory + _subDir);
                settings = CreateInstance<ProceduralInteriorSettings>();
                AssetDatabase.CreateAsset(settings, Path);
                AssetDatabase.SaveAssets();
            }
            else
                settings = AssetDatabase.LoadAssetAtPath<ProceduralInteriorSettings>(Path);

            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif
    }

#if (UNITY_EDITOR)
    static class ProceduralInteriorSettingsIMGUI
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/Procedural Interior Settings", SettingsScope.Project)
            {
                label = "Procedural Interior",
                guiHandler = _ =>
                {
                    SerializedObject serializedObject = ProceduralInteriorSettings.GetSerializedSettings();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_interiorPrefabs"), new GUIContent("Interior Prefabs"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_defaultGround"), new GUIContent("Default Ground"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_defaultWall"), new GUIContent("Default Wall"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_defaultDoorWall"), new GUIContent("Default Door Wall"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_defaultCeiling"), new GUIContent("Default Ceiling"));
                    
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_sampleDimension"), new GUIContent("Sample Dimension"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_minRoomCount"), new GUIContent("Min Room Count"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxRoomCount"), new GUIContent("Max Room Count"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_roomSizeOffset"), new GUIContent("Offset"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_minSamplesPerRoom"), new GUIContent("MinSamplesPerRoom"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_offsetWallNormal"), new GUIContent("Offset Wall Normal"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_offsetWallEndToConnection"), new GUIContent("Offset Wall End Points To Connection"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_offsetLength"), new GUIContent("Offset Length"));
                    serializedObject.ApplyModifiedProperties();
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Number" })
            };
        }
    }
#endif
}