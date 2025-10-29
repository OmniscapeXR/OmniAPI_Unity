// OmniAPI/Runtime/Realtime/LocationSocket.cs
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MiniSocketIO;
using Omniscape.API.Models.Realtime;
using UnityEngine;

namespace Omniscape.API.Realtime
{
    public sealed class LocationSocket : IDisposable
    {
        [Serializable]
        public class NearPlayerReq
        {
            public double longitude;
            public double latitude;
            public int maxDistance;
        }
        
        public enum State { Closed, Connecting, Open }
        public State CurrentState { get; private set; } = State.Closed;

        public event Action OnOpen;
        public event Action OnClose;
        public event Action<string> OnError;
        public event Action<string, string[]> OnEvent;   // (eventName, args[])
        public event Action OnReady;                     // namespace (Socket.IO) ready
        public event Action<string> OnNearPlayer;        // server payload

        readonly string _baseWsUrl;
        MiniSocketIOClient _client;
        bool _nspConnected;                              // set once after 40

        public bool IsConnected => _client?.State == ConnState.Open;

        public LocationSocket(string baseWsUrl)
        {
            _baseWsUrl = baseWsUrl.TrimEnd('/');
        }
        
        static string BuildJson(LocationUpdateMsg m)
        {
            return $"{{\"longitude\":{m.longitude.ToString("0.########", CultureInfo.InvariantCulture)}," +
                   $"\"latitude\":{m.latitude.ToString("0.########", CultureInfo.InvariantCulture)}," +
                   $"\"maxDistance\":{m.maxDistance}}}";
        }

        // Escape a JSON string so it can be sent as a Socket.IO string arg
        static string AsJsonStringLiteral(string json)
        {
            // escape inner quotes for a single JSON string literal
            var inner = json.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $"\"{inner}\""; // <-- one level of quotes only
        }

        public void Connect()
        {
            if (CurrentState != State.Closed) return;

            CurrentState = State.Connecting;
            _client = new MiniSocketIOClient(
                _baseWsUrl,
                "/",
                authJson: null,
                reconnectMin: null,
                reconnectMax: null,
                backoffFactor: 2f
            )
            {
                EnableDebugLogs = true
                
            };
            _client.EnableDebugLogs = true;
            _client.OnRawSocketMessage = raw => Debug.Log($"[MiniSocketIO][RAW 4x] {raw}");
            
            _client.LogFrames = true;
            _client.OnRawIncoming = raw => Debug.Log($"[MiniSocketIO][RAW IN] {raw}");
            _client.OnRawOutgoing = raw => Debug.Log($"[MiniSocketIO][RAW OUT] {raw}");
            
            _client.OnEvent += (evt, args) =>
            {
                Debug.Log($"[MiniSocketIO] EVENT '{evt}' argsCount={args?.Length ?? 0}");
                if (args != null)
                    for (int i = 0; i < args.Length; i++)
                        Debug.Log($"  arg[{i}] = {args[i]}");
            };
            
            _client.OnEvent += (evt, args) =>
            {
                var body = (args != null && args.Length > 0) ? args[0] : "<no-args>";
                Debug.Log($"[LocationSocket] IN event='{evt}' body={body}");
            };

            _client.OnOpen += () =>
            {
                CurrentState = State.Open;
                Debug.Log("[LocationSocket] Connected.");
                
                // ✅ Mark “namespace connected” and fire existing OnReady for your wait
                _nspConnected = true;
                OnReady?.Invoke();
                
                OnOpen?.Invoke();
            };
            _client.OnClose += () =>
            {
                CurrentState = State.Closed;
                Debug.LogWarning("[LocationSocket] Disconnected.");
                OnClose?.Invoke();
            };
            _client.OnError += e =>
            {
                Debug.LogError("[LocationSocket] Error: " + e);
                OnError?.Invoke(e);
            };
            _client.OnEvent += (evt, args) =>
            {
                OnEvent?.Invoke(evt, args);

                // If the lib surfaces "connect" as an event, mark namespace ready.
                if (!_nspConnected && string.Equals(evt, "connect", StringComparison.Ordinal))
                {
                    _nspConnected = true;
                    Debug.Log("[LocationSocket] Namespace connected (Socket.IO).");
                    OnReady?.Invoke();
                }

                if (evt == "near-player" && args?.Length > 0)
                {
                    Debug.Log("[LocationSocket] ← " + args[0]);
                    OnNearPlayer?.Invoke(args[0]);
                }
            };

            Debug.Log($"[LocationSocket] Connecting → {_client.Uri}");
            _ = _client.ConnectAsync(); // fire-and-forget
        }

        /// Call each frame from a MonoBehaviour.
        public void Pump() => _client?.TickMainThread();

        /// Wait for Socket.IO namespace readiness (40). Uses Pump() + timeout.
        public async Task WaitUntilReadyAsync(int timeoutMs = 10000, CancellationToken ct = default)
        {
            if (_nspConnected) return;

            float start = Time.realtimeSinceStartup;
            var tcs = new TaskCompletionSource<bool>();
            void Handler() => tcs.TrySetResult(true);
            OnReady += Handler;

            try
            {
                for (;;)
                {
                    if (_nspConnected || tcs.Task.IsCompleted) return;

                    Pump(); // flush main-thread callbacks

                    if (ct.IsCancellationRequested ||
                        (timeoutMs > 0 && (Time.realtimeSinceStartup - start) * 1000f >= timeoutMs))
                        throw new TaskCanceledException("WaitUntilReadyAsync timed out.");

                    await Task.Yield();
                }
            }
            finally
            {
                OnReady -= Handler;
            }
        }

        /// Emit player location. If namespace isn't ready yet, wait briefly (no throws).
        public async Task SendLocation(LocationUpdateMsg m, int timeoutMs = 3000, CancellationToken ct = default)
        {
            if (!IsConnected) return;
            if (!_nspConnected) { try { await WaitUntilReadyAsync(timeoutMs, ct); } catch { return; } }

            var json = BuildJson(m);

            // IMPORTANT:
            // 1) Do NOT add quotes yourself.
            // 2) Add a single leading space so MiniSocket treats it as a *string* (not raw JSON).
            //    MiniSocket seems to "use-as-is" when the arg starts with '{'.
            //    Leading whitespace makes it quote the arg, and JSON.parse ignores the space.
            var arg = " " + json;

            Debug.Log($"[LocationSocket] OUT event='player-location' payload(string)={json}");

            try
            {
                await _client.EmitAsync("player-location", new[] { arg });
            }
            catch (TimeoutException)
            {
                await _client.EmitAsync("player-location", new[] { arg });
            }
        }
        
        public async Task CloseAsync()
        {
            try { if (_client != null) await _client.CloseAsync(); } catch { }
            _client?.Dispose();
            _client = null;
            CurrentState = State.Closed;
        }

        public void Dispose() => _ = CloseAsync();
    }
}
