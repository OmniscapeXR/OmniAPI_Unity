// OmniscapeAPI/Plugins/Runtime/Core/JsonUtils.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Omniscape.API.Core
{
    /// <summary>
    /// JSON helpers built on UnityEngine.JsonUtility.
    /// Works on Mono/IL2CPP without extra packages.
    /// </summary>
    public static class JsonUtils
    {
        // ----------- Basic object <-> JSON -----------
        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
            return JsonUtility.FromJson<T>(json);
        }

        public static void FromJsonOverwrite<T>(string json, T target) where T : class
        {
            if (string.IsNullOrEmpty(json) || target == null) return;
            JsonUtility.FromJsonOverwrite(json, target);
        }

        public static string ToJson(object obj, bool prettyPrint = false)
        {
            if (obj == null) return "{}";
            return JsonUtility.ToJson(obj, prettyPrint);
        }

        // ----------- Arrays / Lists (JsonUtility limitation workaround) -----------
        [Serializable]
        private class ListWrapper<T> { public List<T> items = new(); }

        /// <summary>Deserialize a JSON array (e.g., "[{...},{...}]") into a List&lt;T&gt;.</summary>
        public static List<T> FromJsonArray<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<T>();
            // JsonUtility needs an object at the root, so wrap/unwrap.
            var wrapped = $"{{\"items\":{json}}}";
            var lw = JsonUtility.FromJson<ListWrapper<T>>(wrapped);
            return lw?.items ?? new List<T>();
        }

        /// <summary>Serialize an IEnumerable&lt;T&gt; as a JSON array.</summary>
        public static string ToJsonArray<T>(IEnumerable<T> items, bool prettyPrint = false)
        {
            var lw = new ListWrapper<T> { items = items != null ? new List<T>(items) : new List<T>() };
            var wrapped = JsonUtility.ToJson(lw, prettyPrint);
            // unwrap: {"items":[...]} -> [...]
            var idx = wrapped.IndexOf(":");
            return idx > 0 ? wrapped.Substring(idx + 1, wrapped.Length - (idx + 2)) : "[]";
        }

        // ----------- Dictionaries (serialize as key/value pairs) -----------
        [Serializable]
        private class DictWrapper<TKey, TValue>
        {
            public List<Entry> entries = new();
            [Serializable] public class Entry { public TKey key; public TValue value; }
        }

        public static string ToJsonDict<TKey, TValue>(Dictionary<TKey, TValue> dict, bool prettyPrint = false)
        {
            var dw = new DictWrapper<TKey, TValue>();
            if (dict != null)
            {
                foreach (var kv in dict)
                    dw.entries.Add(new DictWrapper<TKey, TValue>.Entry { key = kv.Key, value = kv.Value });
            }
            return JsonUtility.ToJson(dw, prettyPrint);
        }

        public static Dictionary<TKey, TValue> FromJsonDict<TKey, TValue>(string json)
        {
            var result = new Dictionary<TKey, TValue>();
            if (string.IsNullOrEmpty(json)) return result;

            var dw = JsonUtility.FromJson<DictWrapper<TKey, TValue>>(json);
            if (dw?.entries == null) return result;

            foreach (var e in dw.entries)
                result[e.key] = e.value;

            return result;
        }
    }
}
