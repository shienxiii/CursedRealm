using System;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class Interior : MonoBehaviour
{
    [SerializeField] private SamplerSettings _samplerSettings;

    private SplineContainer _areas;
    private int[,] _samples;
    private int _sizeX, _sizeY;

    [SerializeField]
    private SerializableDictionary<int, Room> _rooms;
    private HashSet<Wall> walls = new HashSet<Wall>();

    [SerializeField] private Vector3 _start, _end;

    [Header("Debug")]
    [SerializeField] private bool _drawDebug = false;
    [SerializeField] private int _splineToDebug = 0;

    public bool applySeed = false;
    public int seedValue = 5000;

    private System.Random _interiorRandomness;
    public System.Random InteriorRandomness => _interiorRandomness;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    [ContextMenu("Initialize Area")]
    public void InitializeArea()
    {
        if(applySeed)
            UnityEngine.Random.InitState(seedValue);

        if(applySeed)
            _interiorRandomness = new System.Random(seedValue);

        InitializeArea(_splineToDebug);
    }

    public void InitializeArea(int splineIndex)
    {
        if(_areas == null)
            _areas = GetComponent<SplineContainer>();

        if (!_samplerSettings || _areas.Splines.Count == 0 || splineIndex >= _areas.Splines.Count) return;

        // Calculate that area that covers the samples

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

        _sizeX = Mathf.CeilToInt((_end.x - _start.x) / _samplerSettings.SampleDimension.x);
        _sizeY = Mathf.CeilToInt((_end.z - _start.z) / _samplerSettings.SampleDimension.x);

        // Create the samples and determine the valid samples
        _samples = new int[_sizeX,_sizeY];

        if (_rooms == null)
            _rooms = new SerializableDictionary<int, Room>();
        else
            _rooms.Clear();

        List<Vector2Int> validSamples = new List<Vector2Int>();
        for(int u = 0; u < _sizeX; u++)
        {
            for(int v = 0; v < _sizeY; v++)
            {
                Vector3 worldPoint = SamplePointToWorldPoint(u,v);

                // we want to invalidate every sample points first
                _samples[u, v] = -1;

                // and put valid samples points to a list to be parsed and converted to a valid point
                if (SamplerHelperFunctions.IsPointInsidePolygon(polygon, new Vector2(worldPoint.x, worldPoint.z)))
                    validSamples.Add(new Vector2Int(u ,v));
            }
        }

        GenerateRooms(validSamples);
        walls = CalculateWalls();
    }

    public void GenerateRooms(List<Vector2Int> validSamples)
    {
        if (!_samplerSettings || (_samples?.Length ?? 0) == 0 || validSamples.Count == 0) return;

        int roomCount = UnityEngine.Random.Range(_samplerSettings.MinRoomCount, _samplerSettings.MaxRoomCount + 1);
        int roomSizeOffset = Mathf.Abs(_samplerSettings.Offset);

        // Cache the room samples before creating the Room objects
        Dictionary<int, List<Vector2Int>> roomCache = new Dictionary<int, List<Vector2Int>>();

        int targetSize = 0;

        while (validSamples.Count > 0)
        {
            if (roomCount - _rooms.Count <= 0 || validSamples.Count < _samplerSettings.MinSamplesPerRoom)
                targetSize = validSamples.Count;
            else
                targetSize = Math.Clamp((validSamples.Count / roomCount) + UnityEngine.Random.Range(-roomSizeOffset, roomSizeOffset),
                            _samplerSettings.MinSamplesPerRoom,
                            validSamples.Count);

            List<Vector2Int> roomSamples = new List<Vector2Int>(); // samples to be cached as a room
            List<Vector2Int> cardinalSamples = new List<Vector2Int>(); // samples directly next to the samples in roomSamples that are currently -1

            int roomIndex = roomCache.Count;

            // get a starting point to generate the room and add to the cardinalSamples list
            cardinalSamples.Add(SamplerHelperFunctions.GetRandomElement(validSamples, false));

            // Generate the room recursively
            GenerateRoom_Recursive(validSamples, roomSamples, cardinalSamples, targetSize, roomIndex);

            if (roomSamples.Count < _samplerSettings.MinSamplesPerRoom)
            {
                int result = TryMeldSamplesToExistingRoom(roomSamples, roomCache, roomIndex);
                if(result >= 0)
                    continue;
            }

            roomCache.Add(roomIndex, roomSamples);

        }

        foreach(var room in roomCache)
            _rooms.Add(room.Key, new Room(room.Value));
    }

    private void GenerateRoom_Recursive(List<Vector2Int> validSamples, List<Vector2Int> roomSamples, List<Vector2Int> cardinalSamples, int targetSize, int roomIndex)
    {
        // Get a random sample from cardinalSamples and remove the sample from both roomSamples and cardinalSamples
        Vector2Int currentSample = SamplerHelperFunctions.GetRandomElement(cardinalSamples, true);
        validSamples.Remove(currentSample);

        // Assign roomIndex to the position on _samples and assign currentSample to roomSamples
        _samples[currentSample.x, currentSample.y] = roomIndex;
        roomSamples.Add(currentSample);

        ExtendRoomInDirection(validSamples, roomSamples, cardinalSamples, currentSample, new Vector2Int(1, 0), roomIndex);
        ExtendRoomInDirection(validSamples, roomSamples, cardinalSamples, currentSample, new Vector2Int(-1, 0), roomIndex);
        ExtendRoomInDirection(validSamples, roomSamples, cardinalSamples, currentSample, new Vector2Int(0, 1), roomIndex);
        ExtendRoomInDirection(validSamples, roomSamples, cardinalSamples, currentSample, new Vector2Int(0, -1), roomIndex);

        // collect the cardinal samples for currentSample
        SamplerHelperFunctions.GetCardinalSample(validSamples, cardinalSamples, currentSample);

        // Call this function recursively until roomSamples.Count equal or exceed targetSize or cardinalSamples is empty
        if (roomSamples.Count < targetSize && cardinalSamples.Count > 0)
            GenerateRoom_Recursive(validSamples, roomSamples, cardinalSamples, targetSize, roomIndex);

    }

    private void ExtendRoomInDirection(List<Vector2Int> validSamples, List<Vector2Int> roomSamples, List<Vector2Int> cardinalSamples, Vector2Int fromSample, Vector2Int direction, int roomIndex)
    {
        Vector2Int currentSample = fromSample + direction;

        if (!cardinalSamples.Contains(currentSample)) return;

        // we got a valid sample, include it in the room and remove from AvailableSamples and CardinalSamples
        _samples[currentSample.x, currentSample.y] = roomIndex;
        roomSamples.Add(currentSample);
        validSamples.Remove(currentSample);
        cardinalSamples.Remove(currentSample);

        // run this function recursively until reaching a sample point that's not in CardinalSamples
        ExtendRoomInDirection(validSamples, roomSamples, cardinalSamples, currentSample, direction, roomIndex);

        // collect the cardinal samples for currentSample
        SamplerHelperFunctions.GetCardinalSample(validSamples, cardinalSamples, currentSample);
    }

    /// <summary>
    /// Try to meld the provided roomSamples to the closest neighbouring room
    /// </summary>
    /// <param name="roomSamples">list of samples to meld</param>
    /// <param name="currentIndex">this is the roomIndex to ignore besides -1</param>
    private int TryMeldSamplesToExistingRoom(List<Vector2Int> roomSamples, Dictionary<int, List<Vector2Int>> inRoomCache, int currentIndex)
    {
        Debug.Log($"try meld {currentIndex}");
        List<int> roomCandidates = new List<int>();

        void GetCardinalSampleValue(Vector2Int sample)
        {
            // make the sample is within _samples coverage
            if (sample.x < 0 || sample.y < 0 || sample.x >= _sizeX || sample.y >= _sizeY) return;

            int index = _samples[sample.x, sample.y];

            if(index != currentIndex && index > -1 && !roomCandidates.Contains(index))
                roomCandidates.Add(index);
        };

        foreach (Vector2Int sample in roomSamples)
        {
            GetCardinalSampleValue(sample + new Vector2Int(1, 0));
            GetCardinalSampleValue(sample + new Vector2Int(-1, 0));
            GetCardinalSampleValue(sample + new Vector2Int(0, 1));
            GetCardinalSampleValue(sample + new Vector2Int(0, -1));
        }

        if (roomCandidates.Count == 0) return -1;

        int newIndex = SamplerHelperFunctions.GetRandomElement(roomCandidates);

        foreach (Vector2Int sample in roomSamples)
        {
            _samples[sample.x, sample.y] = newIndex;
            inRoomCache[newIndex].Add(sample);
        }

        Debug.Log($"new index {newIndex}, size updated to {inRoomCache[newIndex].Count}");

        return newIndex;
    }
    
    public Vector3 SamplePointToWorldPoint(int x, int z, bool bCenterH = true, bool bCenterV =false)
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

    public HashSet<Wall> CalculateWalls()
    {
        walls = new HashSet<Wall>();

        if (_samples == null || _samplerSettings == null)
            return walls;

        int width = _samples.GetLength(0);
        int height = _samples.GetLength(1);

        float cellSize = _samplerSettings.SampleDimension.x;
        float halfSize = cellSize * 0.5f;

        void TestForWall(in Vector3 start, in Vector3 end, in int x0, in int z0, in int x1, in int z1, in int roomA)
        {
            // test to see if next sample is within spline
            bool isInside = x1 >= 0 && z1 >= 0 && x1 < width && z1 < height;

            int roomB = isInside ? _samples[x1, z1] : -1;

            if(roomA != roomB)
                walls.Add(new Wall(roomA, roomB, start, end));

            // add room connection here to build tree
        }

        // test from index [-1, -1] to remove the need to test for wall in left or rear direction
        for (int x = -1; x < width; x++)
        {
            for (int z = -1; z < height; z++)
            {
                int roomA = x > -1 && z > -1 ? _samples[x, z] : -1;

                /**
                 * Grid Shape
                 *  tl-------tr    
                 *  |        |
                 *  |        |
                 *  |        |
                 *  bl-------br
                 */


                // Original: Algorithm checks tl-bl, tl-tr, bl-br, tr-br from x = 0, z = 0
                // NEW: Algorithm check tl-tr, tr-br from x = -1, z = -1 and reduced calling TestForWall() by over 45%
                Vector3 tl = SamplePointToWorldPoint(x, z + 1, false);
                Vector3 br = SamplePointToWorldPoint(x + 1, z, false);
                Vector3 tr = SamplePointToWorldPoint(x + 1, z + 1, false);

                TestForWall(br, tr, x, z, x + 1, z, roomA);
                TestForWall(tl, tr, x, z, x, z + 1, roomA);
            }
        }

        return walls;
    }

    private void OnDrawGizmos()
    {
        if (_areas == null)
            _areas = GetComponent<SplineContainer>();

        if (!_samplerSettings || (_samples?.Length ?? 0) == 0 || !_drawDebug || _areas.Splines.Count == 0 || _splineToDebug >= _areas.Splines.Count) return;

        Spline area = _areas.Splines[_splineToDebug];

        Gizmos.DrawLine(new Vector3(_start.x, _start.y + 0.5f, _start.z), new Vector3(_start.x, _start.y - 0.5f, _start.z));
        Gizmos.DrawLine(new Vector3(_end.x, _end.y + 0.5f, _end.z), new Vector3(_end.x, _end.y - 0.5f, _end.z));

        Gizmos.color = Color.yellow;
        for(int i = 0; i < area.Count; i++)
        {
            Gizmos.DrawWireSphere(_areas.transform.TransformPoint(area[i].Position), 0.05f);
        }

        // we want to display the debug of the room itself not the sample grid
        foreach (Room room in _rooms.Values)
        {
            Vector2Int start = room.start;

            for (int u = 0; u < room.size.x; u++)
            {
                for (int v = 0; v < room.size.y; v++)
                {
                    Vector2Int gridPoint = new Vector2Int(start.x + u, start.y + v);
                    Vector3 worldPoint = SamplePointToWorldPoint(gridPoint.x, gridPoint.y);
                    if (room.roomArea[u, v])
                    {
                        Gizmos.color = _rooms[_samples[gridPoint.x, gridPoint.y]].debugColor;
                        Gizmos.DrawCube(worldPoint, new Vector3(_samplerSettings.SampleDimension.x - 0.075f, 0.0f, _samplerSettings.SampleDimension.x - 0.075f));

                        Vector3 testPoint = SamplePointToWorldPoint(gridPoint.x, gridPoint.y, false);
                        Gizmos.DrawCube(testPoint, new Vector3(0.5f, 0.5f, 0.5f));
                    }
                }
            }
        }

        foreach (Wall wall in walls)
        {
            Gizmos.color = wall.RoomA == -1 || wall.RoomB == -1 ? Color.yellow : Color.white;
            Gizmos.DrawLine(wall.Start, wall.End);
        }
    }
}
