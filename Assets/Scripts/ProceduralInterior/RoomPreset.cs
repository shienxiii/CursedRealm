using System;
using Unity.VisualScripting;
using UnityEngine;

namespace ProceduralInterior
{
    [Serializable]
    public struct RoomPrefab
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Vector3 _dimension;

        // Leave null if not overriding the material of the prefab
        [SerializeField] private Material _material;

        public GameObject Prefab => _prefab;
        public Vector3 Dimension => _dimension;
        public Material Material => _material;
    }

    /// <summary>
    /// This class is used to define things such as the mesh and/or material to use for a room's
    /// wall, floor and ceiling.
    /// Also used to setup static objects like furnitures, decorations, etc
    /// </summary>
    [CreateAssetMenu(fileName = "RoomPreset", menuName = "Scriptable Objects/RoomPreset")]
    public class RoomPreset : ScriptableObject
    {
        [Header("Overrides")]
        [SerializeField] private bool _overrideFloor;
        [SerializeField] private RoomPrefab _floor;
        [SerializeField] private bool _overrideWall;
        [SerializeField] private RoomPrefab _wall;
        [SerializeField] private bool _overrideCeiling;
        [SerializeField] private RoomPrefab _ceiling;

        [SerializeField, Tooltip("Minimum spatial dimension required for this RoomPreset to be assigned to a Room")]
        private Vector2Int _minSpaceDimension;
        [SerializeField, Tooltip("Minimum area for this RoomPreset to be assigned to a Room")]
        private int _minArea;

        public bool OverrideFloor => _overrideFloor;
        public RoomPrefab Floor => _floor;
        public bool OverrideWall => _overrideWall;
        public RoomPrefab Wall => _wall;
        public bool OverrideCeiling => _overrideCeiling;
        public RoomPrefab Ceiling => _ceiling;
        public Vector2Int MinSpaceDimension => _minSpaceDimension;
        public int MinArea => _minArea;
    }
}