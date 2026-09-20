using System.Collections.Generic;
using UnityEngine;
using ProceduralInterior.Types;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class Interior : MonoBehaviour
{
    // Components
    private SplineContainer _areas;

    [SerializeField] private SamplerSettings _samplerSettings;

    private Sample[,] _grid;

    [SerializeField] private List<Room> _rooms = new List<Room>();
    private List<WallSample> _wallSamples = new List<WallSample>();

    [SerializeField] private Vector3 _start, _end;

    [Header("Debug")]
    [SerializeField] private bool _drawDebug = false;
    [SerializeField] private int _splineToDebug = 0;

    public bool applySeed = false;
    public int roomSeed = 5000;
    private System.Random _interiorRandom;

    /*** PUBLIC PROPERTIES ***/
    public SamplerSettings Settings => _samplerSettings;
    public Sample[,] Grid => _grid;
    public List<Room> Rooms => _rooms;
    public List<WallSample> WallSamples => _wallSamples;

    public System.Random InteriorRandom => _interiorRandom;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    [ContextMenu("Initialize Area")]
    public void InitializeArea()
    {
        if (!applySeed)
        {
            System.Random random = new System.Random();
            roomSeed = random.Next();
        }
        _interiorRandom = new System.Random(roomSeed);

        InitializeArea(_splineToDebug);
    }

    public void InitializeArea(int splineIndex)
    {
        if(_areas == null)
            _areas = GetComponent<SplineContainer>();

        if (!_samplerSettings || _areas.Splines.Count == 0 || splineIndex >= _areas.Splines.Count) return;

        // clear all room and wall information
        _rooms.Clear();
        _wallSamples.Clear();

        // Calculate the area that covers the samples
        Spline area = _areas.Splines[splineIndex];
        if (area.Count == 0) return;

        // Holds the spline points in world space of the area to be initialized
        List<Vector2> polygon = new List<Vector2>();

        _start = _end = _areas.transform.TransformPoint(area[0].Position);


        for (int i = 0; i < area.Count; i++)
        {
            Vector3 point = _areas.transform.TransformPoint(area[i].Position);
            polygon.Add(new Vector2(point.x, point.z));

            _start.x = point.x < _start.x ? point.x : _start.x;
            _start.z = point.z < _start.z ? point.z : _start.z;

            _end.x = point.x > _end.x ? point.x : _end.x;
            _end.z = point.z > _end.z ? point.z : _end.z;
        }

        int sizeX = Mathf.CeilToInt((_end.x - _start.x) / _samplerSettings.SampleDimension.x);
        int sizeY = Mathf.CeilToInt((_end.z - _start.z) / _samplerSettings.SampleDimension.x);

        // Create the samples
        _grid = new Sample[sizeX,sizeY];


        List<Vector2Int> validSamples = new List<Vector2Int>();
        for(int u = 0; u < sizeX; u++)
        {
            for(int v = 0; v < sizeY; v++)
            {
                Vector3 worldPoint = GridPointToWorldPoint(u,v);

                // if grid point is vaild, create the Sample instance and add the grid point to validSamples
                if (SamplerHelperFunctions.IsPointInsidePolygon(polygon, new Vector2(worldPoint.x, worldPoint.z)))
                {
                    _grid[u, v] = new Sample();
                    validSamples.Add(new Vector2Int(u, v));
                }
            }
        }

        RoomGenerator.GenerateRooms(this, validSamples);
        RoomGenerator.SampleWallAndRoomConnections(this);
        RoomGenerator.SampleDoor(this);

        for (int i = 0; i < _rooms.Count; i++)
            WallGenerator.GenerateWallForRoom(this, i);
    }

    public Vector3 GridPointToWorldPoint(int x, int z, bool bCenterH = true, bool bCenterV = false)
    {
        if (!_samplerSettings) return Vector3.zero;

        Vector3 worldPoint = _start;

        if (bCenterH)
        {
            worldPoint.x += (x * _samplerSettings.SampleDimension.x) + (_samplerSettings.SampleDimension.x / 2);
            worldPoint.z += (z * _samplerSettings.SampleDimension.x) + (_samplerSettings.SampleDimension.x / 2);
        }
        else
        {
            worldPoint.x += (x * _samplerSettings.SampleDimension.x);
            worldPoint.z += (z * _samplerSettings.SampleDimension.x);
        }

        if (bCenterV)
            worldPoint.y += (_samplerSettings.SampleDimension.y / 2);

        return worldPoint;
    }


    private void OnDrawGizmos()
    {
        if (_areas == null)
            _areas = GetComponent<SplineContainer>();

        if (!_samplerSettings || (_grid?.Length ?? 0) == 0 || !_drawDebug || _areas.Splines.Count == 0 || _splineToDebug >= _areas.Splines.Count) return;

        foreach(Room room in _rooms)
        {
            Gizmos.color = room.debugColor;
            foreach (GridSpan space in room.Spaces)
            {
                Vector2Int start = space.A;
                Vector2Int end = space.B;

                Vector3 startPoint = GridPointToWorldPoint(start.x, start.y);
                Vector3 endPoint = GridPointToWorldPoint(end.x, end.y);
                Vector3 center = (startPoint + endPoint) / 2;

                Vector2Int dimension = space.GetDimension();
                Vector3 size = new Vector3((_samplerSettings.SampleDimension.x * dimension.x) - 0.1f, 0.0f, (_samplerSettings.SampleDimension.x * dimension.y) - 0.1f);

                Gizmos.DrawCube(center, size);
            }

            List<Wall> walls = room.Walls;
            foreach (Wall wall in walls)
            {
                Gizmos.color = wall.IsDoor ? Color.green : Color.white;

                Vector3 size = wall.End - wall.Start;
                Vector3 normalized = size.normalized;

                float x = Mathf.Abs(size.x) - (normalized.x * 0.075f) + (normalized.z * 0.05f);
                float y = 1.0f;
                float z = Mathf.Abs(size.z) - (normalized.z * 0.075f) + (normalized.x * 0.05f);

                Vector3 center = wall.Start + wall.End;
                center.x /= 2;
                center.y += 0.5f;
                center.z /= 2;
                center += (wall.Normal * 0.1f);

                Gizmos.DrawCube(center, new Vector3(x, y, z));
            }
        }
    }
}
