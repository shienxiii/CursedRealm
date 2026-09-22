using System.IO;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "InteractionSystemSettings", menuName = "Scriptable Objects/InteractionSystemSettings")]
public class InteractionSystemSettings : ScriptableObject
{
    // This is the layer for all interactable object
    [SerializeField] private LayerMask _layerMask;

    public LayerMask LayerMask { get { return _layerMask; } }

    private const string _subDir = "Settings/";
    private const string _asset = "InteractionSystemSettings";

    public static InteractionSystemSettings TryGetSettings()
    {
        InteractionSystemSettings setting = (InteractionSystemSettings)Resources.Load(_asset);
        return setting;
    }

#if (UNITY_EDITOR)
    private const string _directory = "Assets/Resources/";
    public const string Path = _directory + _subDir + _asset + ".asset";

    internal static InteractionSystemSettings GetOrCreateSettings()
    {
        InteractionSystemSettings settings;

        if (!File.Exists(Path))
        {
            if (!Directory.Exists(_directory + _subDir))
                Directory.CreateDirectory(_directory + _subDir);
            settings = CreateInstance<InteractionSystemSettings>();
            AssetDatabase.CreateAsset(settings, Path);
            AssetDatabase.SaveAssets();
        }
        else
            settings = AssetDatabase.LoadAssetAtPath<InteractionSystemSettings>(Path);

        return settings;
    }

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
#endif
}

#if ( UNITY_EDITOR )
static class InteractionSystemSettingsIMGUI
{
    [SettingsProvider]
    public static SettingsProvider CreateProvider()
    {
        return new SettingsProvider("Project/Interaction System Settings", SettingsScope.Project)
        {
            label = "Interaction System",
            guiHandler = _ =>
            {
                SerializedObject serializedObject = InteractionSystemSettings.GetSerializedSettings();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_layerMask"), new GUIContent("Layer Mask"));
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            },
            keywords = new System.Collections.Generic.HashSet<string>(new[] { "Number" })
        };
    }
}
#endif