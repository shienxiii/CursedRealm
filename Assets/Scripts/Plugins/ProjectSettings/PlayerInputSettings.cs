using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSettings : ScriptableObject
{
    // The InputAction asset to be used for player input
    [SerializeField] private InputActionAsset _inputActionAsset = null;
    [SerializeField] private string _actionMap_Play = "Player";
    [SerializeField] private string _actionMap_UI = "UI";

    public InputActionAsset InputActionAsset {  get { return _inputActionAsset; } }
    public string ActionMap_Play { get { return _actionMap_Play; } }
    public string ActionMap_UI { get { return _actionMap_UI; } }

    private const string _subDir = "Settings/";
    private const string _asset = "PlayerInputSettings";

    public static PlayerInputSettings TryGetSettings()
    {
        PlayerInputSettings setting = (PlayerInputSettings)Resources.Load(_subDir + _asset);
        return setting;
    }

#if (UNITY_EDITOR)
    private const string _directory = "Assets/Resources/";
    public const string Path = _directory + _subDir + _asset + ".asset";

    internal static PlayerInputSettings GetOrCreateSettings()
    {
        PlayerInputSettings settings;

        if(!File.Exists(Path))
        {
            if (!Directory.Exists(_directory + _subDir))
                Directory.CreateDirectory(_directory + _subDir);
            settings = CreateInstance<PlayerInputSettings>();
            AssetDatabase.CreateAsset(settings, Path);
            AssetDatabase.SaveAssets();
        }
        else
            settings = AssetDatabase.LoadAssetAtPath<PlayerInputSettings>(Path);

        return settings;
    }

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
#endif
}


#if ( UNITY_EDITOR )
static class PlayerInputSettingsIMGUI
{
    [SettingsProvider]
    public static SettingsProvider CreateProvider()
    {
        return new SettingsProvider("Project/Player Input Settings", SettingsScope.Project)
        {
            label = "Player Input",
            guiHandler = _ =>
            {
                SerializedObject serializedObject = PlayerInputSettings.GetSerializedSettings();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_inputActionAsset"), new GUIContent("Input Asset"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_actionMap_Play"), new GUIContent("Action Map(Gameplay)"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_actionMap_UI"), new GUIContent("Action Map(UI)"));
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            },
            keywords = new System.Collections.Generic.HashSet<string>(new[] { "Number" })
        };
    }
}
#endif