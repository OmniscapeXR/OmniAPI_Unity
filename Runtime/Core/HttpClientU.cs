using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Omniscape
{
    public class HttpClientU
    {
        readonly string _base; readonly Func<string> _token;
        public string Base => _base;
        public HttpClientU(string baseUrl, Func<string> tokenAccessor) { _base = baseUrl.TrimEnd('/'); _token = tokenAccessor; }

        public async Task<string> Get(string path)
        {
            var url = _base + path;
            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("Accept", "application/json");
            var t = _token();
            if (!string.IsNullOrEmpty(t)) req.SetRequestHeader("Authorization", "Bearer " + t); // Bearer gets injected here. :contentReference[oaicite:1]{index=1}
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();
            if (req.result != UnityWebRequest.Result.Success || (req.responseCode / 100) != 2)
                throw new System.Exception($"HTTP {req.responseCode}: {req.error} {req.downloadHandler.text}");
            return req.downloadHandler.text;
        }

        public async Task<string> PostJson(string path, string jsonBody)
        {
            var url = _base + path;
            using var req = new UnityWebRequest(url, "POST");
            var body = Encoding.UTF8.GetBytes(jsonBody ?? "{}");
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");
            var t = _token();
            if (!string.IsNullOrEmpty(t)) req.SetRequestHeader("Authorization", "Bearer " + t);
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();
            if (req.result != UnityWebRequest.Result.Success || (req.responseCode / 100) != 2)
                throw new System.Exception($"HTTP {req.responseCode}: {req.error} {req.downloadHandler.text}");
            return req.downloadHandler.text;
        }
    }
}

