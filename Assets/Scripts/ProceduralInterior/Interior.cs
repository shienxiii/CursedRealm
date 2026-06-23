using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class Interior : MonoBehaviour
{
    [SerializeField] private SamplerSettings _samplerSettings;

    private SplineContainer _areas;
    private int[,] _samples;
    int sizeX, sizeY;

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

        _samples = new int[sizeX,sizeY];

        for(int u = 0; u < sizeX; u++)
        {
            for(int v = 0; v < sizeY; v++)
            {
                Vector3 worldPoint = SamplePointToWorldPoint(u,v);
                Debug.Log(worldPoint);
                _samples[u, v] = SamplerHelperFunctions.IsPointInsidePolygon(polygon, new Vector2(worldPoint.x, worldPoint.z)) ? 0 : -1;
            }
        }
    }

    public Vector3 SamplePointToWorldPoint(int x, int z, bool bCenterH = true)
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

        return worldPoint;
    }

    private void OnDrawGizmos()
    {
        if (_areas == null)
            _areas = GetComponent<SplineContainer>();

        if ((_samples?.Length ?? 0) == 0 || !_drawDebug || _areas.Splines.Count == 0 || _splineToDebug >= _areas.Splines.Count) return;

        Spline area = _areas.Splines[_splineToDebug];

        Gizmos.DrawLine(new Vector3(start.x, start.y + 0.5f, start.z), new Vector3(start.x, start.y - 0.5f, start.z));
        Gizmos.DrawLine(new Vector3(end.x, end.y + 0.5f, end.z), new Vector3(end.x, end.y - 0.5f, end.z));

        for(int i = 0; i < area.Count; i++)
        {
            Gizmos.DrawSphere(_areas.transform.TransformPoint(area[i].Position), 0.05f);
        }

        for (int u = 0; u < sizeX; u++)
        {
            for (int v = 0; v < sizeY; v++)
            {
                Debug.Log(_samples[u, v]);
                Gizmos.color = _samples[u, v] > -1 ? Color.green : Color.red;

                Vector3 worldPoint = SamplePointToWorldPoint(u, v);
                Gizmos.DrawSphere(_areas.transform.TransformPoint(worldPoint), 0.1f);
            }
        }
    }
}
