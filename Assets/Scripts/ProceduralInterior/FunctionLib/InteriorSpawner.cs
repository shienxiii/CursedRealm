using System.Collections.Generic;
using ProceduralInterior.Types;
using UnityEngine;

namespace ProceduralInterior.FunctionLib
{
    /// <summary>
    /// This class is used to handle instantiation of an Interior GameObject
    /// as well as the overall spawning of walls, floor and ceiling
    /// </summary>
    public static class InteriorSpawner
    {
        public static Interior SpawnInterior(Interior inPrefab, System.Random random, Vector3 inPosition)
        {
            Debug.Log("SpawnInterior()");
            if (inPrefab == null || random == null) return null;

            // apply random 90-degree rotation
            float yRotate = (random.Next() % 4) * 90.0f;
            
            Interior newInterior = Object.Instantiate(inPrefab, inPosition, Quaternion.Euler(0.0f, yRotate, 0.0f));
            Debug.Log("Spawn attempt " + newInterior != null);
            return newInterior;
        }

        public static void SpawnStructures(Interior interior)
        {
            if (!interior || interior.Rooms.Count == 0) return;

            ProceduralInteriorSettings settings = InteriorManager.Settings;
            if (!settings) return;

            List<Room> rooms = interior.Rooms;

            foreach (Room room in rooms)
            {
                SpawnWalls(interior, room, settings);
                SpawnGroundAndCeiling(interior, room, settings);
            }
        }

        private static void SpawnWalls(Interior interior, Room inRoom, ProceduralInteriorSettings inSettings)
        {
            if (!interior || inRoom == null || !inSettings) return;

            foreach (Wall wall in inRoom.Walls)
            {
                RoomPrefab prefab = wall.IsDoor ? inSettings.DefaultDoorWall : inSettings.DefaultWall;
                if(!prefab.Prefab) continue;
            
                // Assuming wall prefab is facing z-forward
                // figure out the wall rotation
                Quaternion rotation = Quaternion.LookRotation(wall.Normal, Vector3.up);
            
                // figure out the required scale on the x-axis
                float lengthScale = wall.GetLength() / prefab.Dimension.x;

                GameObject newWall = Object.Instantiate(prefab.Prefab, wall.GetCenter(), rotation);
                newWall.transform.localScale = new Vector3(lengthScale, 1.0f, 1.0f);
                newWall.transform.parent = interior.transform;
                inRoom.AddStructure(newWall);
            }
        
        }

        private static void SpawnGroundAndCeiling(Interior interior, Room inRoom, ProceduralInteriorSettings inSettings)
        {
            if (!interior || inRoom == null || !inSettings) return;

            RoomPrefab groundPrefab = inSettings.DefaultGround;
            RoomPrefab ceilingPrefab = inSettings.DefaultCeiling;

            if (!groundPrefab.Prefab && !ceilingPrefab.Prefab) return;

            float sampleX = inSettings.SampleDimension.x;

            foreach (GridSpan space in inRoom.Spaces)
            {
                Vector3 start = interior.GridPointToWorldPoint(space.A, false);
                Vector3 end = interior.GridPointToWorldPoint(space.B, false) + new Vector3(sampleX, 0.0f, sampleX);
                Vector3 center = (start + end) / 2;

                SpawnStructure(groundPrefab, space.GetDimension(), center);
                SpawnStructure(ceilingPrefab, space.GetDimension(), center);
            }

            void SpawnStructure(RoomPrefab prefab, Vector2Int spaceDimension, Vector3 point)
            {
                if (!prefab.Prefab) return;
            
                Vector3 scale = new Vector3(sampleX * spaceDimension.x / prefab.Dimension.x,
                    1.0f,
                    sampleX * spaceDimension.y / prefab.Dimension.z);
            
                GameObject newStructure = Object.Instantiate(prefab.Prefab, point, Quaternion.identity);
                newStructure.transform.localScale = scale;
                newStructure.transform.parent = interior.transform;
                inRoom.AddStructure(newStructure);
            }
        }
    
    }
}
