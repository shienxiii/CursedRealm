using System.Collections.Generic;
using UnityEngine;
using ProceduralInterior.Types;
using ProceduralInterior.FunctionLib;
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
        
        private List<Room> _rooms     = new List<Room>();
        private List<Border> _borders = new List<Border>();
        private System.Random _random = null;

        /*** PUBLIC PROPERTIES ***/
        public Sample[,] Grid       => _grid;
        public List<Room> Rooms     => _rooms;
        public List<Border> Borders => _borders;
        public System.Random Random => _random;

        #if UNITY_EDITOR
            [Header("Debug")]
            [SerializeField] private int _splineToDebug = 0;
            [SerializeField] public bool _applySeed     = false;
            [SerializeField] public int _debugSeed      = 5000;
            [SerializeField] private bool _drawFloor   = false;
            [SerializeField] private bool _drawWall     = false;
            [SerializeField] private bool _drawReserved = false;
            [SerializeField] private bool _spawnStructures = false;
        #endif
        
        /// <summary>
        /// Sample the grid based on the selected spline and generate the interior layout, rooms, and walls
        /// </summary>
        /// <param name="splineIndex">index of the spline to use, defaults to 0</param>
        /// <returns>List of indexes of valid samples on the grid</returns>
        public void SampleInterior(int splineIndex = 0)
        {
            Clear();
            
            // Should only be called after seeding
            if (_random == null) return;
            
            if (_areas == null)
                _areas = GetComponent<SplineContainer>();

            ProceduralInteriorSettings settings = InteriorManager.Settings;
            
            if (!settings || _areas.Splines.Count == 0 ||
                splineIndex < 0 || splineIndex >= _areas.Splines.Count) return;

            // Calculate the area that covers the samples
            Spline area = _areas.Splines[splineIndex];
            if (area.Count == 0) return;
            
            ParseSpline(area, settings, out List<Vector2> polygon, out Vector2Int gridSize);
            ParseGrid(polygon, gridSize, out List<Vector2Int> validSamples);
            
            RoomSampler.GenerateRooms(this, validSamples);
            RoomSampler.SampleBorder(this);
            RoomSampler.SampleDoor(this);

            for (int i = 0; i < _rooms.Count; i++)
                WallSampler.GenerateWallForRoom(this, i);
            
            #if UNITY_EDITOR
                _splineToDebug = splineIndex;
            #endif
            
        }

        private void ParseSpline(Spline spline, ProceduralInteriorSettings settings, out List<Vector2> edges, out Vector2Int gridSize)
        {
            edges = new List<Vector2>();
            
            if (!settings || spline.Count == 0)
            {
                gridSize = Vector2Int.zero;
                return;
            }
            
            // Holds the spline points in world space of the area to be initialized
            _span = new Vector3Range(_areas.transform.TransformPoint(spline[0].Position));

            for (int i = 0; i < spline.Count; i++)
            {
                Vector3 point = _areas.transform.TransformPoint(spline[i].Position);
                edges.Add(new Vector2(point.x, point.z));

                _span.A.x = Mathf.Min(point.x, _span.A.x);
                _span.A.z = Mathf.Min(point.z, _span.A.z);
                _span.B.x   = Mathf.Max(point.x, _span.B.x);
                _span.B.z   = Mathf.Max(point.z, _span.B.z);
            }

            gridSize = new Vector2Int(Mathf.CeilToInt((_span.B.x - _span.A.x) / settings.SampleDimension.x),
                                        Mathf.CeilToInt((_span.B.z - _span.A.z) / settings.SampleDimension.x));
        }

        private void ParseGrid(List<Vector2> edges, Vector2Int gridSize, out List<Vector2Int> validSamples)
        {
            // Create the grid
            _grid = new Sample[gridSize.x, gridSize.y];
            
            validSamples = new List<Vector2Int>();
            
            for (int u = 0; u < gridSize.x; u++)
            {
                for (int v = 0; v < gridSize.y; v++)
                {
                    Vector3 worldPoint = GridPointToWorldPoint(u, v);

                    // if grid point is valid, create the Sample instance and add the grid point to validSamples
                    if (SamplerHelperFunctions.IsPointWithinPolygon(edges, new Vector2(worldPoint.x, worldPoint.z)))
                    {
                        _grid[u, v] = new Sample();
                        validSamples.Add(new Vector2Int(u, v));
                    }
                }
            }
        }
        
        public Vector3 GridPointToWorldPoint(Vector2Int point, bool bCenterH = true, bool bCenterV = false)
        {
            return GridPointToWorldPoint(point.x, point.y, bCenterH, bCenterV);
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
            _random = new System.Random(inSeed);
        }

        public void Clear()
        {
            // clear all room and wall information
            
            foreach (Room room in _rooms)
                room.ClearStructures();
            
            _rooms.Clear();
            _borders.Clear();
        }
        
        #if UNITY_EDITOR
            [ContextMenu("Debug Gen")]
            public void DebugGenerate()
            {
                if (!_applySeed)
                    _debugSeed = unchecked((int)System.DateTime.Now.Ticks);

                SeedInterior(_debugSeed);
                SampleInterior(_splineToDebug);
                    
                if(_spawnStructures)
                    InteriorSpawner.SpawnStructures(this);
            }

            private void DrawRoomDebug(ProceduralInteriorSettings settings)
            {
                if (!settings) return;
                
                foreach (Room room in _rooms)
                {
                    Gizmos.color = room.debugColor;
                    
                    if(_drawFloor)
                        foreach (GridSpan space in room.Spaces)
                        {
                            Vector3 startPoint = GridPointToWorldPoint(space.A);
                            Vector3 endPoint   = GridPointToWorldPoint(space.B);
                            Vector3 center     = (startPoint + endPoint) / 2;

                            Vector2Int dimension = space.GetDimension();
                            Vector3 size = new Vector3((settings.SampleDimension.x * dimension.x) - 0.1f, 0.0f, (settings.SampleDimension.x * dimension.y) - 0.1f);

                            Gizmos.DrawCube(center, size);
                        }


                    if (_drawWall)
                        foreach (Wall wall in room.Walls)
                        {
                            Gizmos.color = wall.IsDoor ? Color.green : Color.white;

                            Vector3 size       = wall.End - wall.Start;
                            Vector3 sizeNormal = size.normalized;

                            float x = Mathf.Abs(size.x) - (sizeNormal.x * 0.075f) + (sizeNormal.z * 0.05f);
                            float y = 3.0f;
                            float z = Mathf.Abs(size.z) - (sizeNormal.z * 0.075f) + (sizeNormal.x * 0.05f);

                            Vector3 center = wall.Start + wall.End;
                            center.x /= 2;
                            center.y += 1.5f;
                            center.z /= 2;
                            center += (wall.Normal * 0.1f);

                            Gizmos.DrawCube(center, new Vector3(x, y, z));
                        }
                    
                    if(_drawReserved)
                        foreach (Vector2Int reserved in room.Reserved)
                        {
                            Sample sample = _grid[reserved.x, reserved.y];
                            if(sample == null) continue;
                            
                            Vector3 point = GridPointToWorldPoint(reserved);
                            Vector3 size = new Vector3(settings.SampleDimension.x - 0.35f, 0.2f, settings.SampleDimension.x - 0.35f);

                            Gizmos.color = sample.State == SampleState.RESERVED ? Color.green : Color.red;
                            Gizmos.DrawCube(point, size);
                            Gizmos.color = room.debugColor;
                            Gizmos.DrawSphere(point, 0.4f);
                        }
                }
            }
            
            private void OnDrawGizmos()
            {
                if (_areas == null)
                    _areas = GetComponent<SplineContainer>();

                ProceduralInteriorSettings settings = InteriorManager.Settings;

                if (!settings || (_grid?.Length ?? 0) == 0 ||
                    _areas.Splines.Count == 0 || _splineToDebug < 0 ||
                    _splineToDebug >= _areas.Splines.Count) return;

                if(_drawFloor || _drawWall || _drawReserved)
                    DrawRoomDebug(settings);
                
                
                
            }

            
        #endif
    }
}