using System.Collections.Generic;
using UnityEngine;
using ProceduralInterior.Types;
using ProceduralInterior.FunctionLib;
using UnityEngine.Serialization;
using UnityEngine.Splines;


namespace ProceduralInterior
{
    [RequireComponent(typeof(SplineContainer))]
    public class Interior : MonoBehaviour
    {
        // Components
        private SplineContainer _areas;
        
        // Sample and rooms
        private Vector3Range _span;
        private Sample[,] _grid;
        private List<Room> _rooms = new List<Room>();
        private List<WallSample> _wallSamples = new List<WallSample>();
        private System.Random _interiorRandom = null;
        
        /*** PUBLIC PROPERTIES ***/
        public Sample[,] Grid => _grid;
        public List<Room> Rooms => _rooms;
        public List<WallSample> WallSamples => _wallSamples;
        public System.Random InteriorRandom => _interiorRandom;

        #if UNITY_EDITOR
            [Header("Debug")]
            [SerializeField] private bool _drawDebug = false;
            [SerializeField] private int _splineToDebug = 0;
            public bool applySeed = false;
            public int debugSeed = 5000;

            [ContextMenu("Debug Gen")]
            public void DebugGenerate()
            {
                if (applySeed)
                    SeedInterior(debugSeed);
                else
                    SeedInterior(UnityEngine.Random.Range(0, 1000000));
                GenerateInterior(_splineToDebug);
            }
        #endif
        
        /// <summary>
        /// Sample the grid based on the selected spline and generate the interior layout, rooms, and walls
        /// </summary>
        /// <param name="splineIndex">index of the spline to use, defaults to 0</param>
        /// <returns>List of indexes of valid samples on the grid</returns>
        public void GenerateInterior(int splineIndex = 0)
        {
            // Should only be called after seeding
            if (_interiorRandom == null) return;
            
            if (_areas == null)
                _areas = GetComponent<SplineContainer>();

            ProceduralInteriorSettings settings = InteriorManager.Settings;
            
            if (!settings || _areas.Splines.Count == 0 || splineIndex < 0 || splineIndex >= _areas.Splines.Count) return;

            // clear all room and wall information
            _rooms.Clear();
            _wallSamples.Clear();

            // Calculate the area that covers the samples
            Spline area = _areas.Splines[splineIndex];
            if (area.Count == 0) return;
            
            // Holds the spline points in world space of the area to be initialized
            List<Vector2> polygon = new List<Vector2>();

            _span = new Vector3Range(_areas.transform.TransformPoint(area[0].Position));

            for (int i = 0; i < area.Count; i++)
            {
                Vector3 point = _areas.transform.TransformPoint(area[i].Position);
                polygon.Add(new Vector2(point.x, point.z));

                _span.A.x = Mathf.Min(point.x, _span.A.x);
                _span.A.z = Mathf.Min(point.z, _span.A.z);
                _span.B.x   = Mathf.Max(point.x, _span.B.x);
                _span.B.z   = Mathf.Max(point.z, _span.B.z);
            }

            int sizeX = Mathf.CeilToInt((_span.B.x - _span.A.x) / settings.SampleDimension.x);
            int sizeY = Mathf.CeilToInt((_span.B.z - _span.A.z) / settings.SampleDimension.x);

            // Create the grid
            _grid = new Sample[sizeX, sizeY];
            
            List<Vector2Int> validSamples = new List<Vector2Int>();
            
            for (int u = 0; u < sizeX; u++)
            {
                for (int v = 0; v < sizeY; v++)
                {
                    Vector3 worldPoint = GridPointToWorldPoint(u, v);

                    // if grid point is valid, create the Sample instance and add the grid point to validSamples
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
            
            #if UNITY_EDITOR
                _splineToDebug = splineIndex;
            #endif
            
        }
        
        public Vector3 GridPointToWorldPoint(int x, int z, bool bCenterH = true, bool bCenterV = false)
        {
            ProceduralInteriorSettings settings = InteriorManager.Settings;

            if (!settings) return Vector3.zero;

            Vector3 worldPoint = _span.A;
            
            if (bCenterH)
            {
                worldPoint.x += (x * settings.SampleDimension.x) + (settings.SampleDimension.x / 2);
                worldPoint.z += (z * settings.SampleDimension.x) + (settings.SampleDimension.x / 2);
            }
            else
            {
                worldPoint.x += (x * settings.SampleDimension.x);
                worldPoint.z += (z * settings.SampleDimension.x);
            }

            if (bCenterV)
                worldPoint.y += (settings.SampleDimension.y / 2);

            return worldPoint;
        }

        public void SeedInterior(int inSeed)
        {
            _interiorRandom = new System.Random(inSeed);
        }

        #if UNITY_EDITOR
            private void OnDrawGizmos()
            {
                if (_areas == null)
                    _areas = GetComponent<SplineContainer>();

                ProceduralInteriorSettings settings = InteriorManager.Settings;

                if (!_drawDebug || !settings ||
                    (_grid?.Length ?? 0) == 0 || _areas.Splines.Count == 0 ||
                    _splineToDebug < 0 || _splineToDebug >= _areas.Splines.Count) return;

                foreach (Room room in _rooms)
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
                        Vector3 size = new Vector3((settings.SampleDimension.x * dimension.x) - 0.1f, 0.0f, (settings.SampleDimension.x * dimension.y) - 0.1f);

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
        #endif
    }
}