// OmniAPI/Runtime/Models/Inventory/InventoryService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omniscape.API.Core;
using Omniscape.API.Models.Inventory;
using UnityEngine;
using UnityEngine.Networking;

namespace Omniscape.API.Inventory
{
    /// <summary>
    /// Inventory service that parses responses using Unity's JsonUtility via JsonUtils.
    /// Handles: 
    ///  - raw array at root:            [ {...}, {...} ]
    ///  - envelope with items:          { "items": [ ... ] }
    ///  - envelope with data:           { "data":  [ ... ] }
    ///  - envelope with string message: { "message": "[ ... ]" }  // array as string
    /// </summary>
    public sealed class InventoryService
    {
        private readonly HttpClientU _http;
        public InventoryService(HttpClientU http) { _http = http; }

        [Serializable] private class ItemsEnvelope { public List<OmniscapeInventoryDoc> items; }
        [Serializable] private class DataEnvelope  { public List<OmniscapeInventoryDoc> data; }
        [Serializable] private class MsgEnvelope   { public string message; }

        public async Task<List<OmniscapeInventoryDoc>> GetUserInventoryAsync(string userId)
        {
            var path = "/api/user-inventory?uid=" + UnityWebRequest.EscapeURL(userId);
            Debug.Log($"[InventoryService] GET {path}");

            var json = await _http.Get(path);
            var preview = json.Length > 240 ? json.Substring(0, 240) + "…" : json;
            Debug.Log($"[InventoryService] body preview: {preview}");

            if (string.IsNullOrWhiteSpace(json))
                return new List<OmniscapeInventoryDoc>();

            // Quick sniff of the root
            var first = FirstNonWs(json);
            if (first == '[')
            {
                // Raw array at root
                return JsonUtils.FromJsonArray<OmniscapeInventoryDoc>(json) ?? new List<OmniscapeInventoryDoc>();
            }
            if (first == '{')
            {
                // Try common envelopes; JsonUtility ignores unknown fields, so this is safe.
                var items = JsonUtils.FromJson<ItemsEnvelope>(json)?.items;
                if (items != null) return items;

                var data = JsonUtils.FromJson<DataEnvelope>(json)?.data;
                if (data != null) return data;

                // message may be an array serialized as a string
                var msg = JsonUtils.FromJson<MsgEnvelope>(json)?.message;
                if (!string.IsNullOrEmpty(msg))
                {
                    msg = msg.Trim();
                    if (msg.StartsWith("[") || msg.StartsWith("{"))
                    {
                        var innerFirst = FirstNonWs(msg);
                        if (innerFirst == '[')
                            return JsonUtils.FromJsonArray<OmniscapeInventoryDoc>(msg) ?? new List<OmniscapeInventoryDoc>();

                        if (innerFirst == '{')
                        {
                            // Some APIs do { "items": [...] } in the message field
                            var innerItems = JsonUtils.FromJson<ItemsEnvelope>(msg)?.items;
                            if (innerItems != null) return innerItems;

                            var innerData = JsonUtils.FromJson<DataEnvelope>(msg)?.data;
                            if (innerData != null) return innerData;
                        }
                    }

                    // Not JSON array/string we recognize—surface the message
                    throw new Exception($"Inventory API message: {msg}");
                }

                throw new Exception("Inventory API returned an unrecognized object envelope.");
            }

            throw new Exception($"Inventory API returned unexpected root char: '{first}'");
        }

        private static char FirstNonWs(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (!char.IsWhiteSpace(c)) return c;
            }
            return '\0';
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
