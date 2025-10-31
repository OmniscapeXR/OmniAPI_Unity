// OmniAPI/Runtime/Worlds/WorldsService.cs
using System.Threading.Tasks;
using Omniscape.API.Core;
using UnityEngine;

namespace Omniscape
{
    /// <summary>
    /// Handles world item pickup/drop API calls using Unity's JsonUtility (via JsonUtils).
    /// </summary>
    public sealed class WorldsService
    {
        private readonly HttpClientU _http;
        public WorldsService(HttpClientU http) { _http = http; }

        // POST /worlds/pickup-item
        public async Task<ApiResult> PickupItem(string userId, string worldId, string worldItemId)
        {
            var payload = new
            {
                user_id_picking_up = userId,
                world_id = worldId,
                world_item_id = worldItemId
            };

            var res = await _http.PostJson("/worlds/pickup-item", JsonUtils.ToJson(payload));
            return JsonUtils.FromJson<ApiResult>(res) ?? new ApiResult { success = true };
        }

        // POST /worlds/drop-item
        public async Task<ApiResult> DropItem(string userId, string inventoryItemId, string worldId, float x, float y, float z)
        {
            var payload = new
            {
                user_id = userId,
                inventory_item_id = inventoryItemId,
                world_id = worldId,
                coords = new[] { x, y, z }
            };

            var res = await _http.PostJson("/worlds/drop-item", JsonUtils.ToJson(payload));
            return JsonUtils.FromJson<ApiResult>(res) ?? new ApiResult { success = true };
        }
    }

    /// <summary>
    /// Generic API result wrapper.
    /// </summary>
    [System.Serializable]
    public class ApiResult
    {
        public bool success;
        public object message; // keep loose unless you define a schema
    }
}