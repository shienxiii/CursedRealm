
using System;
using UnityEngine;

namespace ProceduralInterior.Types
{
    [Serializable]
    public struct InteriorOptionEntry : ISerializationCallbackReceiver
    {
        public Interior Prefabs;
        public int MinRoomCount;
        public int MaxRoomCount;
        public int RoomSizeOffset;

        public void OnAfterDeserialize()
        {
            if (MinRoomCount == 0) MinRoomCount = 10;
            if (MaxRoomCount == 0) MaxRoomCount = 15;
            if (RoomSizeOffset == 0) RoomSizeOffset = 5;
        }

        public void OnBeforeSerialize() {}
    }
}
