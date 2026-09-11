using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class Interior : MonoBehaviour
{
    [SerializeField] private SamplerSettings _samplerSettings;

    private SplineContainer _areas;
    private Sample[,] _samples;
    private int _sizeX, _sizeY;

    [SerializeField]
    private List<Room> _rooms = new List<Room>();
    private List<Wall> _walls = new List<Wall>();

    [SerializeField] private Vector3 _start, _end;

    [Header("Debug")]
    [SerializeField] private bool _drawDebug = false;
    [SerializeField] private int _splineToDebug = 0;

    public bool applySeed = false;
    public int roomSeed = 5000;
    public int doorSeed = 5000;

    private System.Random _roomRandom;
    public System.Random RoomRandom => _roomRandom;

    private System.Random _doorRandom;
    public System.Random DoorRandom => _doorRandom;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    [ContextMenu("Initialize Area")]
    public void InitializeArea()
    {
        if (applySeed)
        {
            _roomRandom = new System.Random(roomSeed);
            _doorRandom = new System.Random(doorSeed);
        }
        else
        {
            _roomRandom = new System.Random();
            _doorRandom = new System.Random();
        }

        InitializeArea(_splineToDebug);
    }

    public void InitializeArea(int splineIndex)
    {
        if(_areas == null)
            _areas = GetComponent<SplineContainer>();

        if (!_samplerSettings || _areas.Splines.Count == 0 || splineIndex >= _areas.Splines.Count) return;

        // clear all room and wall information
        _rooms.Clear();
        _walls.Clear();

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

        // Create the samples
        _samples = new Sample[_sizeX,_sizeY];


        List<Vector2Int> validSamples = new List<Vector2Int>();
        for(int u = 0; u < _sizeX; u++)
        {
            for(int v = 0; v < _sizeY; v++)
            {
                Vector3 worldPoint = GridPointToWorldPoint(u,v);

                // if grid point is vaild, create the Sample instance and add the grid point to validSamples
                if (SamplerHelperFunctions.IsPointInsidePolygon(polygon, new Vector2(worldPoint.x, worldPoint.z)))
                {
                    _samples[u, v] = new Sample(-1);
                    validSamples.Add(new Vector2Int(u, v));
                }
            }
        }

        GenerateRooms(validSamples);
        CalculateWallsAndRoomConnections();
        GenerateDoors();
    }

    public void GenerateRooms(List<Vector2Int> validSamples)
    {
        if (!_samplerSettings || (_samples?.Length ?? 0) == 0 || validSamples.Count == 0) return;

        int roomCount = _roomRandom.Next(_samplerSettings.MinRoomCount, _samplerSettings.MaxRoomCount + 1);
        int roomSizeOffset = Mathf.Abs(_samplerSettings.Offset);

        // We want to cache the room samples before creating the Room objects
        List<List<Vector2Int>> roomCache = new List<List<Vector2Int>>();

        int targetSize = 0;

        while (validSamples.Count > 0)
        {
            if (roomCount - _rooms.Count <= 0 || validSamples.Count < _samplerSettings.MinSamplesPerRoom)
                targetSize = validSamples.Count;
            else
                targetSize = Math.Clamp((validSamples.Count / roomCount) + _roomRandom.Next(-roomSizeOffset, roomSizeOffset + 1),
                            _samplerSettings.MinSamplesPerRoom,
                            validSamples.Count);

            List<Vector2Int> roomSamples = new List<Vector2Int>(); // samples to be cached as a room
            List<Vector2Int> cardinalSamples = new List<Vector2Int>(); // samples directly next to the samples in roomSamples that are currently -1

            int roomIndex = roomCache.Count;

            // get a starting point to generate the room and add to the cardinalSamples list
            cardinalSamples.Add(SamplerHelperFunctions.GetRandomElement(validSamples, _roomRandom, false));

            // Generate the room recursively
            GenerateRoom_Recursive(validSamples, roomSamples, cardinalSamples, targetSize, roomIndex);

            if (roomSamples.Count < _samplerSettings.MinSamplesPerRoom)
            {
                int result = TryMeldSamplesToExistingRoom(roomSamples, roomCache, roomIndex);
                if(result >= 0)
                    continue;
            }

            roomCache.Add(roomSamples);

        }

        foreach(var room in roomCache)
            _rooms.Add(new Room(room));
    }

    private void GenerateRoom_Recursive(List<Vector2Int> validSamples, List<Vector2Int> roomSamples, List<Vector2Int> cardinalSamples, int targetSize, int roomIndex)
    {
        // Get a random sample from cardinalSamples and remove the sample from both roomSamples and cardinalSamples
        Vector2Int currentSample = SamplerHelperFunctions.GetRandomElement(cardinalSamples, _roomRandom, true);
        validSamples.Remove(currentSample);

        // Assign roomIndex to the position on _samples and assign currentSample to roomSamples
        _samples[currentSample.x, currentSample.y].RoomIndex = roomIndex;
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
        _samples[currentSample.x, currentSample.y].RoomIndex = roomIndex;
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
    private int TryMeldSamplesToExistingRoom(List<Vector2Int> roomSamples, List<List<Vector2Int>> inRoomCache, int currentIndex)
    {
        List<int> roomCandidates = new List<int>();

        void GetCardinalSampleValue(Vector2Int sample)
        {
            // make the sample is within _samples coverage
            if (sample.x < 0 || sample.y < 0 || sample.x >= _sizeX || sample.y >= _sizeY || _samples[sample.x, sample.y] == null) return;

            int index = _samples[sample.x, sample.y].RoomIndex;

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

        int newIndex = SamplerHelperFunctions.GetRandomElement(roomCandidates, _roomRandom);

        foreach (Vector2Int sample in roomSamples)
        {
            _samples[sample.x, sample.y].RoomIndex = newIndex;
            inRoomCache[newIndex].Add(sample);
        }

        return newIndex;
    }
    
    private void CalculateWallsAndRoomConnections()
    {
        if (_samples == null || _samplerSettings == null)
            return;

        _walls.Clear();

        int width = _samples.GetLength(0);
        int height = _samples.GetLength(1);

        float cellSize = _samplerSettings.SampleDimension.x;
        float halfSize = cellSize * 0.5f;

        HashSet<Wall> walls = new HashSet<Wall>();

        // Test if there is a wall between 2 sample point and add the wall
        void ParseWall(in Vector3 start, in Vector3 end, in int x0, in int z0, in int x1, in int z1, in int roomA)
        {
            // test to see if next sample is within spline
            bool isInside = x1 >= 0 && z1 >= 0 && x1 < width && z1 < height && _samples[x1, z1] != null;

            int roomB = isInside ? _samples[x1, z1].RoomIndex : -1;

            if (roomA == roomB) return;

            Wall newWall = new Wall(roomA, roomB, new Vector2Int(x0, z0), new Vector2Int(x1, z1), start, end);
            if (!walls.Add(newWall)) return;

            _walls.Add(newWall);

            int wallIndex = _walls.Count - 1;

            // if roomA and roomB are index to actual room, add room connection here
            if(roomA > -1 && roomB > -1)
            {
                _rooms[roomA].AddDoorCandidateForRoom(roomB, wallIndex);
                _rooms[roomB].AddDoorCandidateForRoom(roomA, wallIndex);
            }

        }

        // test from index [-1, -1] to remove the need to test for wall in left or rear direction
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int roomA = x > -1 && z > -1 && _samples[x, z] != null ? _samples[x, z].RoomIndex : -1;

                /** Grid Shape
                 *  tl-----tr    
                 *  |      |
                 *  |      |
                 *  bl-----br
                 */
                Vector3 tl = GridPointToWorldPoint(x, z + 1, false);
                Vector3 br = GridPointToWorldPoint(x + 1, z, false);
                Vector3 tr = GridPointToWorldPoint(x + 1, z + 1, false);

                ParseWall(br, tr, x, z, x + 1, z, roomA);
                ParseWall(tl, tr, x, z, x, z + 1, roomA);

                if(x == 0 || z == 0)
                {
                    Vector3 bl = GridPointToWorldPoint(x, z, false);
                    if(x == 0)
                        ParseWall(bl, tl, x, z, x - 1, z, roomA);

                    if(z == 0)
                        ParseWall(bl, br, x, z, x, z - 1, roomA);
                }
            }
        }

        for (int i = 0; i < _rooms.Count; i++)
        {
            string message = string.Format($"Room {i} : Connected Rooms {_rooms[i].DoorCandidates.Count} |");
            foreach (KeyValuePair<int, List<int>> connection in _rooms[i].DoorCandidates)
                message = string.Format($"{message} [{connection.Key} : {connection.Value.Count} walls]");

            Debug.Log(message);
        }
    }

    private void GenerateDoors()
    {
        for(int roomIndex = 0; roomIndex < _rooms.Count;roomIndex++)
        {
            Room current = _rooms[roomIndex];
            Dictionary<int, List<int>> doorCandidates = current.DoorCandidates;
            List<int> nextRooms = current.DoorCandidates.Keys.ToList();

            foreach (int nextIndex in nextRooms)
            {
                Room next = _rooms[nextIndex];
                int currentPathCount = current.DoorCandidates.Count + current.Doors.Count;
                int nextPathCount = next.DoorCandidates.Count + next.Doors.Count;

                // Check if we want to establish path with this room or break the path
                bool canBreakPath = currentPathCount > 1 && nextPathCount > 1;
                bool keepPath = (_roomRandom.Next(100) % 4) > 2;

                if(canBreakPath && !keepPath)
                {
                    // Remove both rooms from each others ConnectingWalls Dictionary
                    _rooms[roomIndex].RemoveDoorCandidatesForRoom(nextIndex);
                    _rooms[nextIndex].RemoveDoorCandidatesForRoom(roomIndex);
                    continue;
                }

                int wallIndex = SamplerHelperFunctions.GetRandomElement(doorCandidates[nextIndex], _doorRandom);

                // flag the selected wall for a door
                Wall newDoor = _walls[wallIndex];
                newDoor.Door = true;

                // flag the samples on both sides of the door as reserved
                _samples[newDoor.RoomA.Value.x, newDoor.RoomA.Value.y].State = SampleState.RESERVED;
                _samples[newDoor.RoomB.Value.x, newDoor.RoomB.Value.y].State = SampleState.RESERVED;

                // Remove both rooms from each others ConnectingWalls Dictionary
                _rooms[roomIndex].RemoveDoorCandidatesForRoom(nextIndex);
                _rooms[nextIndex].RemoveDoorCandidatesForRoom(roomIndex);

                // Add reference to door
                _rooms[roomIndex].AddDoor(nextIndex, wallIndex);
                _rooms[roomIndex].AddDoor(roomIndex, wallIndex);
            }
        }

        foreach(Wall wall in _walls)
        {
            Debug.Log(wall);
        }
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
        for(int i = 0; i < _rooms.Count; i++)
        {
            Room room = _rooms[i];
            Vector2Int start = room.Start;

            for (int u = room.Start.x; u <= room.End.x; u++)
            {
                for (int v = room.Start.y; v <= room.End.y; v++)
                {
                    Vector3 worldPoint = GridPointToWorldPoint(u, v);
                    if (_samples != null && _samples[u, v]?.RoomIndex == i)
                    {
                        Gizmos.color = _samples[u, v].State == SampleState.RESERVED ? Color.aquamarine : room.debugColor;
                        Gizmos.DrawCube(worldPoint, new Vector3(_samplerSettings.SampleDimension.x - 0.075f, 0.0f, _samplerSettings.SampleDimension.x - 0.075f));
                    }
                }
            }
        }


        foreach (Wall wall in _walls)
        {
            Color c = wall.RoomA.Key == -1 || wall.RoomB.Key == -1 ? Color.yellow : Color.white;
            c = wall.Door ? Color.red : c;
            c.a = 1;
            Gizmos.color = c;

            float x = Mathf.Abs(wall.Start.x - wall.End.x) - 0.075f;
            float y = 2.0f;
            float z = Mathf.Abs(wall.Start.z - wall.End.z) - 0.075f;

            Vector3 center = wall.Start + wall.End;
            center.x /= 2;
            center.y += 1.0f;
            center.z /= 2;

            Gizmos.DrawCube(center, new Vector3(x, y, z));
            //Gizmos.DrawLine(wall.Start, wall.End);
        }
    }
}
