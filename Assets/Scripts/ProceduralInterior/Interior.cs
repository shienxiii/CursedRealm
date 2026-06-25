using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class Interior : MonoBehaviour
{
    [SerializeField] private SamplerSettings _samplerSettings;

    private SplineContainer _areas;
    private int[,] _samples;
    int sizeX, sizeY;

    private Dictionary<int, Room> _rooms;

    [SerializeField] private Vector3 start, end;

    [Header("Debug")]
    [SerializeField] private bool _drawDebug = false;
    [SerializeField] private int _splineToDebug = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnityEngine.Random.InitState(5000);
    }

    [ContextMenu("Initialize Area")]
    public void InitializeArea()
    {
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

        start = end = _areas.transform.TransformPoint(area[0].Position);


        for (int i = 0; i < area.Count; i++)
        {
            Vector3 point = _areas.transform.TransformPoint(area[i].Position);
            polygon.Add(new Vector2(point.x, point.z));

            start.x = point.x < start.x ? point.x : start.x;
            start.z = point.z < start.z ? point.z : start.z;

            end.x = point.x > end.x ? point.x : end.x;
            end.z = point.z > end.z ? point.z : end.z;
        }

        sizeX = Mathf.CeilToInt((end.x - start.x) / _samplerSettings.SampleDimension.x);
        sizeY = Mathf.CeilToInt((end.z - start.z) / _samplerSettings.SampleDimension.x);

        // Create the samples and determine the valid samples
        _samples = new int[sizeX,sizeY];
        _rooms = new Dictionary<int, Room>();

        List<Vector2Int> validSamples = new List<Vector2Int>();
        for(int u = 0; u < sizeX; u++)
        {
            for(int v = 0; v < sizeY; v++)
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

        while (validSamples.Count > 0 && roomCount - _rooms.Count > 0)
        {
            int targetSize = roomCount - _rooms.Count > 0 ? (validSamples.Count/ roomCount) + UnityEngine.Random.Range(-roomSizeOffset, roomSizeOffset) : validSamples.Count;

            List<Vector2Int> roomSamples = new List<Vector2Int>();
            List<Vector2Int> cardinalSamples = new List<Vector2Int>();

            int roomIndex = _rooms.Count;

            // get a starting point to generate the room and add to the cardinalSamples list
            cardinalSamples.Add(SamplerHelperFunctions.GetRandomSample(validSamples, false));

            // Generate the room recursively
            GenerateRoom_Recursive(validSamples, roomSamples, cardinalSamples, targetSize, roomIndex);

            // TODO: Check if need to meld room

            // Create room
            _rooms.Add(roomIndex, new Room());
        }
    }

    private void GenerateRoom_Recursive(List<Vector2Int> validSamples, List<Vector2Int> roomSamples, List<Vector2Int> cardinalSamples, int targetSize, int roomIndex)
    {
        // Get a random sample from cardinalSamples and remove the sample from both roomSamples and cardinalSamples
        Vector2Int currentSample = SamplerHelperFunctions.GetRandomSample(cardinalSamples, true);
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

    public Vector3 SamplePointToWorldPoint(int x, int z, bool bCenterH = true, bool bCenterV =false)
    {
        if (!_samplerSettings) return Vector3.zero;

        Vector3 worldPoint = start;

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

        Gizmos.DrawLine(new Vector3(start.x, start.y + 0.5f, start.z), new Vector3(start.x, start.y - 0.5f, start.z));
        Gizmos.DrawLine(new Vector3(end.x, end.y + 0.5f, end.z), new Vector3(end.x, end.y - 0.5f, end.z));

        Gizmos.color = Color.yellow;
        for(int i = 0; i < area.Count; i++)
        {
            Gizmos.DrawWireSphere(_areas.transform.TransformPoint(area[i].Position), 0.05f);
        }

        for (int u = 0; u < sizeX; u++)
        {
            for (int v = 0; v < sizeY; v++)
            {
                if (_samples[u, v] == -1) continue;
                Gizmos.color = _rooms[_samples[u, v]].debugColor;
                Vector3 worldPoint = SamplePointToWorldPoint(u, v);
                Gizmos.DrawWireCube(worldPoint, new Vector3(_samplerSettings.SampleDimension.x - 0.3f, 0.0f, _samplerSettings.SampleDimension.x - 0.3f));
            }
        }
    }
}
