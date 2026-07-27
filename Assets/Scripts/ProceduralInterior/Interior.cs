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

    private Dictionary<int, Room> _rooms;

    [SerializeField] private Vector3 _start, _end;

    [Header("Debug")]
    [SerializeField] private bool _drawDebug = false;
    [SerializeField] private int _splineToDebug = 0;

    public bool applySeed = false;
    public int seedValue = 5000;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    [ContextMenu("Initialize Area")]
    public void InitializeArea()
    {
        if(applySeed)
            UnityEngine.Random.InitState(seedValue);

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
        _rooms = new Dictionary<int, Room>();

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
    }

    public void GenerateRooms(List<Vector2Int> validSamples)
    {
        if (!_samplerSettings || (_samples?.Length ?? 0) == 0 || validSamples.Count == 0) return;

        int roomCount = UnityEngine.Random.Range(_samplerSettings.MinRoomCount, _samplerSettings.MaxRoomCount + 1);
        int roomSizeOffset = Mathf.Abs(_samplerSettings.Offset);

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

            List<Vector2Int> roomSamples = new List<Vector2Int>();
            List<Vector2Int> cardinalSamples = new List<Vector2Int>();

            int roomIndex = roomCache.Count;

            // get a starting point to generate the room and add to the cardinalSamples list
            cardinalSamples.Add(SamplerHelperFunctions.GetRandomElement(validSamples, false));

            // Generate the room recursively
            GenerateRoom_Recursive(validSamples, roomSamples, cardinalSamples, targetSize, roomIndex);

            if (roomSamples.Count < _samplerSettings.MinSamplesPerRoom)
            {
                int result = TryMeldSamplesToExistingRoom(roomSamples, roomCache, roomIndex);
                if(result >= 0)
                {
                    Debug.Log($"Meld attempt {roomIndex} returned {result}");
                    continue;
                }

                Debug.LogWarning($"Meld attempt {roomIndex} failed: returned {result}");
            }

            Debug.Log($"Add new room {roomIndex} sized {roomSamples.Count}");
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

    private void CalculateWalls()
    {
        if (_samples.Length == 0) return;

        void TestForWall()
        {

        }
    }
    /*void AInterior::CalculateWalls()
    {
        if (Samples.Points.empty())
            return;

        auto TestForWall = [&](const FVector&WallStart, const FVector&WallEnd, FIntVector2 P0, FIntVector2 P1)
	{
            const bool bInside = P1.X >= 0 && P1.Y >= 0 && P1.X < Samples.X && P1.Y < Samples.Y;

            int32 CurrentValue = Samples.Points[P0.X][P0.Y];
            int32 NeighborValue = bInside ? Samples.Points[P1.X][P1.Y] : INDEX_NONE;

            FIntVector2 CurrentCell = FIntVector2(P0.X, P0.Y);
            FIntVector2 NeighborCell = bInside ? FIntVector2(P1.X, P1.Y) : FIntVector2(INDEX_NONE, INDEX_NONE);

            if (!bInside || NeighborValue != CurrentValue)
            {
                FWall NewWall = FWall(WallStart, WallEnd,
                                    CurrentValue, NeighborValue,
                                    CurrentCell, NeighborCell);

                if (Walls.Contains(NewWall) && NeighborValue != INDEX_NONE) return;

                int WallIndex = Walls.AddUnique(NewWall);

                AddRoomConnection(CurrentValue, NeighborValue, WallIndex);
            }
        }
        ;

        for (auto Room : Rooms)
        {
            for (FIntVector2 Sample : Room.Value.Samples)
            {
                // Convert corners to world-space
                FVector TL = GetWorldPointForSample(FIntVector2(Sample.X, Sample.Y), false);
                FVector TR = GetWorldPointForSample(FIntVector2(Sample.X, Sample.Y + 1), false);
                FVector BL = GetWorldPointForSample(FIntVector2(Sample.X + 1, Sample.Y), false);
                FVector BR = GetWorldPointForSample(FIntVector2(Sample.X + 1, Sample.Y + 1), false);

                TestForWall(BL, BR, Sample, FIntVector2(Sample.X + 1, Sample.Y));
                TestForWall(TL, TR, Sample, FIntVector2(Sample.X - 1, Sample.Y));
                TestForWall(TR, BR, Sample, FIntVector2(Sample.X, Sample.Y + 1));
                TestForWall(TL, BL, Sample, FIntVector2(Sample.X, Sample.Y - 1));
            }
        }
    }*/

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

        for (int u = 0; u < _sizeX; u++)
        {
            for (int v = 0; v < _sizeY; v++)
            {
                if (_samples[u, v] == -1) continue;
                Gizmos.color = _rooms[_samples[u, v]].debugColor;
                Vector3 worldPoint = SamplePointToWorldPoint(u, v);
                Gizmos.DrawCube(worldPoint, new Vector3(_samplerSettings.SampleDimension.x - 0.075f, 0.0f, _samplerSettings.SampleDimension.x - 0.075f));

                Vector3 testPoint = SamplePointToWorldPoint(u, v, false);
                Gizmos.DrawCube(testPoint, new Vector3(0.3f, 0.3f, 0.3f));
            }
        }

        foreach (Room room in _rooms.Values)
        {
            for (int u = 0; u < room.size.x; u++)
            {
                for (int v = 0; v < room.size.y; v++)
                {
                    Vector3 worldPoint = SamplePointToWorldPoint(room.start.x + u, room.start.y + v);
                    if (room.roomArea[u, v])
                    {
                        Color negative = Color.white - room.debugColor;
                        negative.a = 1;
                        Gizmos.color = negative;
                        Gizmos.DrawSphere(worldPoint, _samplerSettings.SampleDimension.x / 4.0f);
                    }
                    else if (_samples[u, v] >= 0)
                    {
                        worldPoint.y += 1;
                        Gizmos.color = _rooms[_samples[u, v]].debugColor;
                        Gizmos.DrawSphere(worldPoint, _samplerSettings.SampleDimension.x / 4.0f);
                    }
                }
            }
        }
    }
}
