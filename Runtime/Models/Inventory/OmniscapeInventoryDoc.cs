// Plugins/Runtime/Models/Inventory/OmniscapeInventoryDoc.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Omniscape.API.Models.Inventory
{
    /// <summary>
    /// Raw doc as returned by the API, shaped for Unity's JsonUtility.
    /// Field names must match JSON keys exactly.
    /// NOTE: JsonUtility doesn't handle DateTime well, so timestamps are strings.
    /// </summary>
    [Serializable]
    public class OmniscapeInventoryDoc
    {
        public string _id;
        public string id;
        public string owner_id;
        public string catalog_id;
        public string gltf_id;
        public string order_id;
        public string tx_id;

        public int serial_no;
        public float map_scale;
        public float[] model_rotation;
        public float[] collider_scale;
        public List<OmniCollider> collider_data;

        // Use string for timestamps (ISO-8601) to keep JsonUtility happy
        public string ts_created;
        public string ts_updated;
        public string ts_transferred;

        public bool spawnable;
        public bool dropped;
        public bool only_admins;
        public bool coming_soon;

        public string title;
        public string name;
        public string description;
        public string type;
        public string category;
        public List<string> tags;

        public string file_format;
        public string file_storage_reference;
        public string gs_file_storage_reference;
        public string file_storage_zip;
        public string url;

        public string thumbnail_url;
        public string thumbnail_storage_reference;
        public string gs_thumbnail_storage_reference;

        public long size;
        public string size_formatted;

        public List<string> app_categories;
        public List<string> animations;

        public string ordinal_id;
        public string ordinal_origin;
        public string ordinal_rarity;

        public float? ar_scale;   // Nullable floats are supported as class fields
        public string color;
    }

    [Serializable]
    public class OmniCollider
    {
        public float[] rotation;
        public float[] scale;
        public float[] position;
    }

    /// <summary>Light, game-facing projection.</summary>
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
