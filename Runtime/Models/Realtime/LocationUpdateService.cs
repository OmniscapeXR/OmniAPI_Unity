// OmniAPI/Runtime/Realtime/LocationUpdateService.cs
using System.Threading.Tasks;
using Omniscape.API.Models.Realtime;
using UnityEngine;

namespace Omniscape.API.Realtime
{
    /// <summary>
    /// Starts Unity GPS and streams updates to LocationSocket with distance/time throttling.
    /// </summary>
    public sealed class LocationUpdateService : MonoBehaviour
    {
        [Header("SDK Config Asset")]
        public OmniscapeAPIConfig config;
        
        public float minMetersDelta = 5f;     // send only if moved this far
        public float minSeconds     = 3f;     // or this much time elapsed
        public int   desiredAccuracyMeters = 10;
        public int   updateDistanceMeters  = 5;
        


        LocationSocket _socket;
        string _uid;
        Vector2d _lastLatLon;
        float _lastSentTime;

        struct Vector2d { public double x, y; public Vector2d(double X,double Y){x=X;y=Y;} }

        public async Task Initialize(LocationSocket socket, string uid)
        {
            _socket = socket;
            _uid = uid;

#if UNITY_EDITOR
            Debug.Log("[LocationUpdateService] Editor mode — skipping GPS initialization.");
            _lastLatLon = new Vector2d(0, 0);
            _lastSentTime = 0;
            enabled = true;
            return;
#endif

            // --- Runtime (mobile/standalone) only ---
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("[LocationUpdateService] Location not enabled by user.");
                return;
            }

            Input.location.Start(desiredAccuracyMeters, updateDistanceMeters);

            // Wait for service to start
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                await Task.Delay(1000);
                maxWait--;
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning($"[LocationUpdateService] Location status: {Input.location.status}");
                return;
            }

            _lastLatLon = new Vector2d(
                Input.location.lastData.latitude,
                Input.location.lastData.longitude
            );
            _lastSentTime = 0;
            enabled = true;
        }


        void OnDisable()
        {
            if (Input.location.isEnabledByUser) Input.location.Stop();
            _ = _socket?.CloseAsync();  
        }

        void Update()
        {
            HeartBeat();
            
            if (_socket == null || _socket.CurrentState != LocationSocket.State.Open) return;
            if (Input.location.status != LocationServiceStatus.Running) return;

            var li = Input.location.lastData;
            var now = Time.time;

            var cur = new Vector2d(li.latitude, li.longitude);
            var dist = HaversineMeters(_lastLatLon, cur);
            var elapsed = now - _lastSentTime;

            if (_lastSentTime == 0 || dist >= minMetersDelta || elapsed >= minSeconds)
            {
                var msg = LocationUpdateMsg.Create(_uid, li);
                _ = _socket.SendLocation(msg); // fire-and-forget
                _lastLatLon = cur;
                _lastSentTime = now;
            }
            

        }

        private void HeartBeat()
        {
            _socket.Pump();
            Debug.LogWarning($"[LocationUpdateService] HeartBeat!");
        }

        static double HaversineMeters(Vector2d a, Vector2d b)
        {
            if (a.x == 0 && a.y == 0) return double.MaxValue;
            double R = 6371000.0;
            double dLat = Mathf.Deg2Rad * (float)(b.x - a.x);
            double dLon = Mathf.Deg2Rad * (float)(b.y - a.y);
            double lat1 = Mathf.Deg2Rad * (float)a.x;
            double lat2 = Mathf.Deg2Rad * (float)b.x;

            double sinDLat = System.Math.Sin(dLat / 2);
            double sinDLon = System.Math.Sin(dLon / 2);
            double h = sinDLat*sinDLat + System.Math.Cos(lat1)*System.Math.Cos(lat2)*sinDLon*sinDLon;
            double c = 2 * System.Math.Atan2(System.Math.Sqrt(h), System.Math.Sqrt(1 - h));
            return R * c;
        }
    }
}
