using System.Collections.Generic;
using ProceduralInterior.Types;
using UnityEngine;

namespace ProceduralInterior.FunctionLib
{
    public static class WallSampler
    {
        /// <summary>
        /// Generate a list of Wall for a room based on the WallSample stored in the owning Interior
        /// </summary>
        /// <param name="inInterior">The Interior that owns the room</param>
        /// <param name="inRoom">The room which walls to be generated</param>
        public static void GenerateWallForRoom(Interior inInterior, int roomIndex)
        {
            // null check and index check
            if (inInterior == null || roomIndex < 0 || roomIndex >= inInterior.Rooms.Count) return;

            Room room = inInterior.Rooms[roomIndex];
            if (room == null) return;

            List<WallGroup> wallGroups = SortBorders(inInterior.Borders, roomIndex, room.Borders, room.Walls);
            if (wallGroups == null) return;

            foreach (WallGroup wallGroup in wallGroups)
                MergeWalls(wallGroup, room);
        }

        /// <summary>
        /// Sorts out the Borders into WallGroups based on their direction and linearity 
        /// </summary>
        /// <param name="inSamples">Border samples from the owning Interior</param>
        /// <param name="inRoomIndex">Index of the room we are sorting for</param>
        /// <param name="inWallIndexes">index of the borders making up the target room we're sorting for</param>
        /// <param name="outDoors">If a Border sample is marked door, we're taking it our of the sorted list and create the Wall early here</param>
        private static List<WallGroup> SortBorders(List<Border> inSamples, int inRoomIndex, List<int> inWallIndexes, List<Wall> outDoors)
        {
            if (inSamples == null || inWallIndexes == null || outDoors == null) return null;

            List<WallGroup> walls = new List<WallGroup>();

            foreach (int wallIndex in inWallIndexes)
            {
                Border sample = inSamples[wallIndex];

                // if the WallSample is for a door, add to outDooe and move to next
                if (sample.IsDoor)
                {
                    outDoors.Add(new Wall(sample, sample.GetWallNormal(inRoomIndex)));
                    continue;
                }

                // Sort into a WallGroup
                bool wallAdded = false;

                // parse against every existing WallGroups
                for (int i = 0; i < walls.Count && !wallAdded; i++)
                    wallAdded = walls[i].TryAdd(sample);

                if (!wallAdded)
                    walls.Add(new WallGroup(sample, inRoomIndex));
            }
            return walls;
        }

        /// <summary>
        /// Merge all continuous walls and create the final Wall
        /// </summary>
        /// <param name="inWallGroup">Wall group we're parsing and merging for</param>
        /// <param name="inRoom">The room to own the created Walls</param>
        private static void MergeWalls(WallGroup inWallGroup, Room inRoom)
        {
            if (inWallGroup == null || inWallGroup.Walls == null || inRoom == null) return;

            // Sort the walls so they're in ascending order based on their direction
            if (inWallGroup.Direction == Vector3.right)
                inWallGroup.Walls.Sort((a, b) => (a.A.x.CompareTo(b.A.x)));
            else
                inWallGroup.Walls.Sort((a, b) => (a.B.z.CompareTo(b.B.z)));

            List<Vector3Range> walls = inWallGroup.Walls;

            // And generate the final wall
            int start = 0;
            int end = 0;

            for (int i = 1; i < walls.Count; i++)
            {
                if (!CustomVectorMath.EqualWithTolerance(walls[end].B, walls[i].A))
                {
                    // if not continuous, create a new wall with current start and end
                    inRoom.Walls.Add(new Wall(walls[start].A, walls[end].B, inWallGroup.Normal, false));

                    // reassign start
                    start = i;
                }

                // update end to latest i
                end = i;
            }

            inRoom.Walls.Add(new Wall(walls[start].A, walls[end].B, inWallGroup.Normal, false));
            
            SortWallsAndOffsetEndpoints(inRoom.Walls);
        }

        /// <summary>
        /// Sort the wall to ensure a continuous connections between the walls
        /// endpoints and offset the endpoints to ensure the walls are connected
        /// </summary>
        /// <param name="walls">list of walls to sort</param>
        private static void SortWallsAndOffsetEndpoints(List<Wall> walls)
        {
            ProceduralInteriorSettings settings = InteriorManager.Settings;
            
            if (!settings || walls == null || walls.Count < 2) return;

            for (int currentIndex = 0; currentIndex < walls.Count; currentIndex++)
            {
                int nextIndex = (currentIndex + 1) % walls.Count;
                int swapIndex = FindNextConnectedWall(walls, currentIndex, nextIndex);

                // return as something else had gone wrong if we get -1 at this point
                if (swapIndex == -1) return;
                
                if (swapIndex != nextIndex)
                    (walls[nextIndex], walls[swapIndex]) = (walls[swapIndex], walls[nextIndex]);
            }
            
            // Now that the wall is sorted, we want to apply the offsets
            if(!settings.OffsetWallEndPoints) return;
            
            for (int u = 0; u < walls.Count; u++)
            {
                int v = (u + 1) % walls.Count;
                
                if(!settings.OffsetWallEndPoints) continue;
                
                Wall current = walls[u];
                Wall next = walls[v];

                if(CustomVectorMath.EqualWithTolerance(current.Normal, next.Normal)) continue;
                
                current.OffsetEnd(next.Normal, settings.OffsetLength);
                next.OffsetStart(current.Normal, settings.OffsetLength);

                walls[u] = current;
                walls[v] = next;
            }
        }

        /// <summary>
        /// Get the index of the wall with a Start point that matches
        /// the End point of the Wall pointed by the provided index 
        /// </summary>
        /// <param name="walls">the list of walls</param>
        /// <param name="index">index of the wall to be tested against</param>
        /// <param name="nextIndex">(index + 1) % walls.Count</param>
        /// <returns></returns>
        private static int FindNextConnectedWall(List<Wall> walls, int index, int nextIndex)
        {
            Wall current = walls[index];

            for (int i = nextIndex; i < walls.Count; i++)
            {
                if(!CustomVectorMath.EqualWithTolerance(current.End, walls[i].Start)) continue;

                return i;
            }

            return -1;
        }

        /// <summary>
        ///  Used to store a group of wall samples with matching direction, normal and linearity
        ///  in the context of the stored room index
        /// </summary>
        private class WallGroup
        {
            // Need to store the room index to query the wall sample when checking wall's normal
            private int _roomIndex;
            private List<Vector3Range> _walls;
            private Vector3 _direction;
            private Vector3 _normal;
            private float _linearity;

            public List<Vector3Range> Walls => _walls;
            public Vector3 Direction => _direction;
            public Vector3 Normal => _normal;

            public WallGroup(Border inBorder, int inRoomIndex)
            {
                _roomIndex = inRoomIndex;
                _normal = inBorder.GetWallNormal(_roomIndex);
                _direction = inBorder.Direction;

                Vector3Range firstWall = inBorder.GetAsVectorRange(); ;

                _walls = new() { firstWall };

                _linearity = _direction == Vector3.right
                    ? firstWall.A.z
                    : firstWall.A.x;
            }

            private bool CanContain(Border sample)
            {
                if (sample.Direction != _direction)
                    return false;

                Vector3 normal = sample.GetWallNormal(_roomIndex);

                if (Vector3.Dot(normal, _normal) < 0.9999f) return false;

                float linearity = _direction == Vector3.right
                    ? sample.Start.z
                    : sample.Start.x;

                return Mathf.Approximately(_linearity, linearity);
            }

            public bool TryAdd(Border sample)
            {
                if (!CanContain(sample)) return false;

                _walls.Add(sample.GetAsVectorRange());

                return true;
            }
        }
    }

}