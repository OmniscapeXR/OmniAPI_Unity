using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Omniscape {
    public sealed class WorldsService {
        readonly HttpClientU _http;
        public WorldsService(HttpClientU http) { _http = http; }

        // POST /worlds/pickup-item
        public async Task<ApiResult> PickupItem(string userId, string worldId, string worldItemId) {
            var payload = new {
                user_id_picking_up = userId,
                world_id = worldId,
                world_item_id = worldItemId
            };
            var res = await _http.PostJson("/worlds/pickup-item", JsonConvert.SerializeObject(payload));
            return JsonConvert.DeserializeObject<ApiResult>(res) ?? new ApiResult{ success = true };
        }

        // POST /worlds/drop-item
        public async Task<ApiResult> DropItem(string userId, string inventoryItemId, string worldId, float x, float y, float z) {
            var payload = new {
                user_id = userId,
                inventory_item_id = inventoryItemId,
                world_id = worldId,
                coords = new []{ x, y, z }
            };
            var res = await _http.PostJson("/worlds/drop-item", JsonConvert.SerializeObject(payload));
            return JsonConvert.DeserializeObject<ApiResult>(res) ?? new ApiResult{ success = true };
        }
    }

    public class ApiResult {
        public bool success;
        public object message; // keep loose unless you have a response schema
    }
}
