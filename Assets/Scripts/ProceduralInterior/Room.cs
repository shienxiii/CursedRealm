using System;
using System.Collections.Generic;
using ProceduralInterior.Types;
using UnityEngine;


namespace ProceduralInterior
{
    /// <summary>
    /// This class hold a single room in an Interior.
    /// The values in this class all refers to an index in the owning Interior
    /// </summary>
    [Serializable]
    public class Room
    {
        // Starting and ending grid point of the rectangle making up this room
        private GridSpan _span;
        
        private List<GridSpan> _spaces       = new(); // collection of rectangles made up of all the samples of this room
        private List<int> _borders           = new(); // indexes the border samples in owning Interior
        private List<Wall> _walls            = new(); // walls making up and facing this room
        private List<Vector2Int> _reserved   = new(); // indexes to samples within this room marked as reserved
        private List<GameObject> _structures = new(); // GameObjects making up this Room, mainly the walls, floors and ceiling
        private Dictionary<int, int> _doors  = new(); // KVP of all Rooms sharing a door marked border with this Room and index to that border
        
        public Vector2Int Start           => _span.A;
        public Vector2Int End             => _span.B;
        public Vector2Int Dimension       => _span.GetDimension();
        public List<GridSpan> Spaces      => _spaces;
        public List<int> Borders          => _borders;
        public List<Wall> Walls           => _walls;
        public List<Vector2Int> Reserved  => _reserved;
        public Dictionary<int, int> Doors => _doors;

        // Temporary Dictionary to the list of walls that can be converted to a door to another room
        // Key: Next room index | Value: Walls separating this room from the next room
        // NOTE: Should be empty after door assignment
        private Dictionary<int, List<int>> _neighbours = new();
        public Dictionary<int, List<int>> Neighbours   => _neighbours;

        #if UNITY_EDITOR
            public Color debugColor;
        #endif

        public Room(List<Vector2Int> samples)
        {
            #if UNITY_EDITOR
                System.Random rand = new System.Random();
                debugColor = new Color(((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255);
            #endif

            if (samples.Count == 0)
            {
                _span = new GridSpan();
                return;
            }

            // Generate Space data
            Vector2Int start = samples[0];
            Vector2Int end = samples[0];

            for (int i = 1; i < samples.Count; i++)
            {
                Vector2Int p = samples[i];

                start.x = p.x < start.x ? p.x : start.x;
                start.y = p.y < start.y ? p.y : start.y;

                end.x = p.x > end.x ? p.x : end.x;
                end.y = p.y > end.y ? p.y : end.y;
            }

            _span = new GridSpan(start, end);
        }

        public void AddNeighbourForRoom(int connectingRoom, int wallIndex)
        {
            if (!_neighbours.ContainsKey(connectingRoom))
                _neighbours.Add(connectingRoom, new List<int> { wallIndex });
            else
                _neighbours[connectingRoom].Add(wallIndex);
        }

        public void RemoveNeighbourForRoom(int connectingRoom)
        {
            _neighbours.Remove(connectingRoom);
        }

        public void AddBorder(int wallIndex)
        {
            _borders.Add(wallIndex);
        }

        public bool AddDoor(int nextRoomIndex, int wallIndex)
        {
            if (_doors.ContainsKey(nextRoomIndex)) return false;

            _doors.Add(nextRoomIndex, wallIndex);
            return true;
        }

        public void AddReservedSample(Vector2Int inSampleIndex)
        {
            _reserved.Add(inSampleIndex);
        }

        /// <summary>
        /// Convert a Vector2Int from Room's relative point to the owning Interior's relative point
        /// i.e: (u, v) will be converted to (Start.x + u, Start.Y +v)
        /// </summary>
        /// <param name="inPoint"></param>
        /// <returns>Owning Interior's relative point</returns>
        public Vector2Int RoomPointToInteriorPoint(Vector2Int inPoint)
        {
            return inPoint + Start;
        }

        public void AddStructure(GameObject inStructure)
        {
            _structures.Add(inStructure);
        }
        
        public void AddStructures(List<GameObject> inStructures)
        {
            _structures.AddRange(inStructures);
        }

        public void ClearStructures()
        {
            foreach (GameObject structure in _structures)
            {
                #if UNITY_EDITOR
                    GameObject.DestroyImmediate(structure);
                #else
                    GameObject.Destroy(structure);
                #endif
            }
            
            _structures.Clear();
        }
    }
}