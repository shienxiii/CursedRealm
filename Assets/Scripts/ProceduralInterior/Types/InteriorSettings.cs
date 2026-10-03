
using System;
using UnityEngine;

namespace ProceduralInterior.Types
{
    [Serializable]
    public struct InteriorSettings : ISerializationCallbackReceiver
    {
        public Interior Prefabs;
        public int MinRoomCount;
        public int MaxRoomCount;

        public void OnAfterDeserialize()
        {
            if (MinRoomCount == 0) MinRoomCount = 10;
            if (MaxRoomCount == 0) MaxRoomCount = 15;
        }

        public void OnBeforeSerialize() {}
    }
}
