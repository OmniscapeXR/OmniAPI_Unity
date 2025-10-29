// OmniAPI/Runtime/Models/Inventory/InventoryService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Omniscape.API.Models.Inventory;
using UnityEngine;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Omniscape.API.Inventory
{
    public sealed class InventoryService
    {
        private readonly HttpClientU _http;
        public InventoryService(HttpClientU http) { _http = http; }

        /// <summary>
        /// GET https://marketplace.omniscape.com/api/user-inventory?uid={userId}
        /// Returns either a raw array or an envelope (items/data).
        /// </summary>
public async Task<List<OmniscapeInventoryDoc>> GetUserInventoryAsync(string userId)
{
    var path = "/api/user-inventory?uid=" + UnityWebRequest.EscapeURL(userId);
    Debug.Log($"[InventoryService] GET {path}");

    var json = await _http.Get(path);
    var preview = json.Length > 240 ? json.Substring(0, 240) + "…" : json;
    Debug.Log($"[InventoryService] body preview: {preview}");

    var tok = JToken.Parse(json);

    // Case 1: raw array at root
    if (tok.Type == JTokenType.Array)
        return tok.ToObject<List<OmniscapeInventoryDoc>>() ?? new List<OmniscapeInventoryDoc>();

    // Case 2: envelope object
    if (tok.Type == JTokenType.Object)
    {
        var obj = (JObject)tok;

        // common envelopes
        if (obj.TryGetValue("items", out var itemsTok) && itemsTok.Type == JTokenType.Array)
            return itemsTok.ToObject<List<OmniscapeInventoryDoc>>() ?? new List<OmniscapeInventoryDoc>();

        if (obj.TryGetValue("data", out var dataTok) && dataTok.Type == JTokenType.Array)
            return dataTok.ToObject<List<OmniscapeInventoryDoc>>() ?? new List<OmniscapeInventoryDoc>();

        // your endpoint: array serialized as a STRING in 'message'
        if (obj.TryGetValue("message", out var msgTok))
        {
            var s = msgTok.Type == JTokenType.String ? (string)msgTok : msgTok.ToString();
            // is it JSON? try to parse again
            s = s?.Trim();
            if (!string.IsNullOrEmpty(s) && (s.StartsWith("[") || s.StartsWith("{")))
            {
                var inner = JToken.Parse(s);
                if (inner.Type == JTokenType.Array)
                    return inner.ToObject<List<OmniscapeInventoryDoc>>() ?? new List<OmniscapeInventoryDoc>();
                if (inner.Type == JTokenType.Object && ((JObject)inner).TryGetValue("items", out var innerItems) && innerItems.Type == JTokenType.Array)
                    return innerItems.ToObject<List<OmniscapeInventoryDoc>>() ?? new List<OmniscapeInventoryDoc>();
            }

            throw new System.Exception($"Inventory API message: {s}");
        }

        throw new System.Exception("Inventory API returned an unrecognized object envelope.");
    }

    throw new System.Exception($"Inventory API returned unexpected token type: {tok.Type}");
}
        public static InventoryItem ToItem(OmniscapeInventoryDoc d) => new InventoryItem
        {
            Id        = d.id ?? d._id,
            Title     = string.IsNullOrEmpty(d.title) ? d.name : d.title,
            TypeId    = string.IsNullOrEmpty(d.catalog_id) ? d.gltf_id : d.catalog_id,
            Thumbnail = d.thumbnail_url,
            AssetUrl  = d.url,
            Serial    = d.serial_no,
            Rarity    = d.ordinal_rarity,
            Spawnable = d.spawnable,
            MapScale  = d.map_scale,
            ARScale   = d.ar_scale ?? 1f,
            Rotation  = d.model_rotation
        };
    }
}
