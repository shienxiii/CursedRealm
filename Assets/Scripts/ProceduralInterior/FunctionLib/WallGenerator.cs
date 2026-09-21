using System.Collections.Generic;
using ProceduralInterior.Types;
using UnityEngine;

namespace ProceduralInterior.FunctionLib
{
    public static class WallGenerator
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

            List<WallGroup> wallGroups = SortWallByDirectionAndLinearity(inInterior.WallSamples, roomIndex, room.WallSamples, room.Walls);
            if (wallGroups == null) return;

            foreach (WallGroup wallGroup in wallGroups)
                MergeWalls(wallGroup, room);
        }

        /// <summary>
        /// Sorts out the WallSamples into 
        /// </summary>
        /// <param name="inSamples"></param>
        /// <param name="inWallIndexes"></param>
        private static List<WallGroup> SortWallByDirectionAndLinearity(List<WallSample> inSamples, int inRoomIndex, List<int> inWallIndexes, List<Wall> outDoors)
        {
            if (inSamples == null || inWallIndexes == null || outDoors == null) return null;

            List<WallGroup> walls = new List<WallGroup>();

            foreach (int wallIndex in inWallIndexes)
            {
                WallSample sample = inSamples[wallIndex];

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

            public WallGroup(WallSample inWallSample, int inRoomIndex)
            {
                _roomIndex = inRoomIndex;
                _normal = inWallSample.GetWallNormal(_roomIndex);
                _direction = inWallSample.Direction;

                Vector3Range firstWall = inWallSample.GetAsVectorRange(); ;

                _walls = new() { firstWall };

                _linearity = _direction == Vector3.right
                    ? firstWall.A.z
                    : firstWall.A.x;
            }

            private bool CanContain(WallSample sample)
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

            public bool TryAdd(WallSample sample)
            {
                if (!CanContain(sample)) return false;

                _walls.Add(sample.GetAsVectorRange());

                return true;
            }
        }
    }

}