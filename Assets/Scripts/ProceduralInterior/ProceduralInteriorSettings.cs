using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ProceduralInteriorSettings", menuName = "Scriptable Objects/Procedural Interior/ProceduralInteriorSettings")]
public class ProceduralInteriorSettings : ScriptableObject
{
    [Header("Sampler Settings")]
    /// <summary>
    /// Dimension of each sample when sampling.
    /// x = Horizontal dimension
    /// y = Vertical dimension
    /// </summary>
    [SerializeField]
    private Vector2 _sampleDimension = new Vector2(2.0f, 3.0f);
    [SerializeField] private int _minRoomCount = 5;
    [SerializeField] private int _maxRoomCount = 10;
    [SerializeField] private int _offset = 5;
    [SerializeField] private int _minSamplesPerRoom = 4;

    public Vector2 SampleDimension => _sampleDimension;
    public int MinRoomCount => _minRoomCount;
    public int MaxRoomCount => _maxRoomCount;
    public int Offset => _offset;
    public int MinSamplesPerRoom => _minSamplesPerRoom;

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

#if ( UNITY_EDITOR )
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
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_sampleDimension"), new GUIContent("Sample Dimension"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_minRoomCount"), new GUIContent("Min Room Count"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxRoomCount"), new GUIContent("Max Room Count"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_offset"), new GUIContent("Offset"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_minSamplesPerRoom"), new GUIContent("MinSamplesPerRoom"));
                serializedObject.ApplyModifiedProperties();
            },
            keywords = new System.Collections.Generic.HashSet<string>(new[] { "Number" })
        };
    }
}
#endif