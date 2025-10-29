// Plugins/Runtime/Models/Inventory/OmniscapeInventoryDoc.cs
using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace Omniscape.API.Models.Inventory
{
    [Serializable]
    public class OmniscapeInventoryDoc
    {
        [JsonProperty("_id")] public string _id;
        [JsonProperty("id")] public string id;
        [JsonProperty("owner_id")] public string owner_id;
        [JsonProperty("catalog_id")] public string catalog_id;
        [JsonProperty("gltf_id")] public string gltf_id;
        [JsonProperty("order_id")] public string order_id;
        [JsonProperty("tx_id")] public string tx_id;

        [JsonProperty("serial_no")] public int serial_no;
        [JsonProperty("map_scale")] public float map_scale;
        [JsonProperty("model_rotation")] public float[] model_rotation;
        [JsonProperty("collider_scale")] public float[] collider_scale;
        [JsonProperty("collider_data")] public List<OmniCollider> collider_data;

        [JsonProperty("ts_created")] public DateTime? ts_created;
        [JsonProperty("ts_updated")] public DateTime? ts_updated;
        [JsonProperty("ts_transferred")] public DateTime? ts_transferred;

        [JsonProperty("spawnable")] public bool spawnable;
        [JsonProperty("dropped")] public bool dropped;
        [JsonProperty("only_admins")] public bool only_admins;
        [JsonProperty("coming_soon")] public bool coming_soon;

        [JsonProperty("title")] public string title;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("type")] public string type;
        [JsonProperty("category")] public string category;
        [JsonProperty("tags")] public List<string> tags;

        [JsonProperty("file_format")] public string file_format;
        [JsonProperty("file_storage_reference")] public string file_storage_reference;
        [JsonProperty("gs_file_storage_reference")] public string gs_file_storage_reference;
        [JsonProperty("file_storage_zip")] public string file_storage_zip;
        [JsonProperty("url")] public string url;

        [JsonProperty("thumbnail_url")] public string thumbnail_url;
        [JsonProperty("thumbnail_storage_reference")] public string thumbnail_storage_reference;
        [JsonProperty("gs_thumbnail_storage_reference")] public string gs_thumbnail_storage_reference;

        [JsonProperty("size")] public long size;
        [JsonProperty("size_formatted")] public string size_formatted;

        [JsonProperty("app_categories")] public List<string> app_categories;
        [JsonProperty("animations")] public List<string> animations;

        [JsonProperty("ordinal_id")] public string ordinal_id;
        [JsonProperty("ordinal_origin")] public string ordinal_origin;
        [JsonProperty("ordinal_rarity")] public string ordinal_rarity;

        [JsonProperty("ar_scale")] public float? ar_scale;
        [JsonProperty("color")] public string color;
    }

    [Serializable]
    public class OmniCollider
    {
        [JsonProperty("rotation")] public float[] rotation;
        [JsonProperty("scale")] public float[] scale;
        [JsonProperty("position")] public float[] position;
    }

    // Light, game-facing projection
    [Serializable]
    public class InventoryItem
    {
        public string Id;
        public string Title;
        public string TypeId;      // catalog_id / gltf_id
        public string Thumbnail;
        public string AssetUrl;    // GLB
        public int    Serial;
        public string Rarity;      // ordinal_rarity
        public bool   Spawnable;
        public float  MapScale;
        public float  ARScale;
        public float[] Rotation;   // yaw/pitch/roll if needed
    }
}
