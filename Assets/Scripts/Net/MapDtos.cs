using System;
using System.Collections.Generic;

namespace NullBorn.Game.Net
{
    [Serializable]
    public class ZoneDto
    {
        public string id;
        public string name;
        public float minX;
        public float maxX;
        public float minZ;
        public float maxZ;
        public string mapImageUrl;
    }

    [Serializable]
    public class MarkerDto
    {
        public string id;
        public string zoneId;
        public string label;
        public float x;
        public float z;
        public string color;
        public string type;
        public bool isActive;
        public string updatedAt;
    }

    [Serializable]
    public class MarkerListDto
    {
        public MarkerDto[] items;
    }

    [Serializable]
    public class PlayerPoseDto
    {
        public string playerId;
        public float x;
        public float z;
        public float yaw;
        public string updatedAt;
    }

    [Serializable]
    public class UpdatePlayerPoseRequest
    {
        public float x;
        public float z;
        public float yaw;
    }

    public static class JsonHelper
    {
        // Unity JsonUtility cannot deserialize top-level arrays.
        public static MarkerDto[] FromJsonArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "[]")
                return Array.Empty<MarkerDto>();

            var wrapped = "{\"items\":" + json + "}";
            var list = UnityEngine.JsonUtility.FromJson<MarkerListDto>(wrapped);
            return list?.items ?? Array.Empty<MarkerDto>();
        }
    }
}
