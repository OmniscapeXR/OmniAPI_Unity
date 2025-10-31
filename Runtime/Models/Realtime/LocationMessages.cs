// OmniAPI/Runtime/Models/Realtime/LocationMessages.cs
using System;

namespace Omniscape.API.Models.Realtime
{
    [Serializable]
    public class LocationUpdateMsg
    {
        public string type = "location.update";
        public string uid;
        public double latitude;
        public double longitude;
        public int maxDistance;
        public double? alt;
        public double? accuracy;
        public double? heading;
        public double? speed;
        public string ts; // ISO8601

        public static LocationUpdateMsg Create(string uid, UnityEngine.LocationInfo li)
        {
            return new LocationUpdateMsg {
                uid = uid,
                latitude = li.latitude,
                longitude = li.longitude,
                maxDistance = 1000,
                alt = li.altitude,
                accuracy = li.horizontalAccuracy,
                heading = null,
                speed = li.timestamp > 0 ? (double?)0 : null,
                ts = System.DateTimeOffset.UtcNow.ToString("o")
            };
        }
    }

    [Serializable]
    public class PingMsg { public string type = "ping"; public long t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); }
}