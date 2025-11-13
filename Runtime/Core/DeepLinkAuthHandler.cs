using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeepLinkAuthHandler : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Name of the scene to load after successful deep link auth.")]
    private string targetSceneName = "MainScene";

    void Awake()
    {
        if (!string.IsNullOrEmpty(Application.absoluteURL))
            OnDeepLinkActivated(Application.absoluteURL);

        Application.deepLinkActivated += OnDeepLinkActivated;
    }

    void OnDestroy() => Application.deepLinkActivated -= OnDeepLinkActivated;

    static string GetQueryValue(Uri uri, string key)
    {
        var q = uri.Query;
        if (string.IsNullOrEmpty(q)) return null;
        var span = q.AsSpan(q.StartsWith("?") ? 1 : 0);
        while (!span.IsEmpty)
        {
            var amp = span.IndexOf('&');
            var part = amp >= 0 ? span[..amp] : span;
            var eq = part.IndexOf('=');
            if (eq > 0)
            {
                var k = Uri.UnescapeDataString(part[..eq].ToString());
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                    return Uri.UnescapeDataString(part[(eq + 1)..].ToString());
            }
            if (amp < 0) break;
            span = span[(amp + 1)..];
        }
        return null;
    }

    public async void OnDeepLinkActivated(string url)
    {
        Debug.Log("[DeepLinkAuth] deepLinkActivated: " + url);

        try
        {
            var uri = new Uri(url);
            if (!uri.Scheme.Equals("omniscape", StringComparison.OrdinalIgnoreCase)) return; // or your temp scheme

            var token = GetQueryValue(uri, "token");
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning("[DeepLinkAuth] No token param.");
                return;
            }

            // Persist token so HttpClientU will send Authorization header
            Omniscape.OmniscapeAPI.SetAccessToken(token);

// Decode sub from JWT
var userId = Omniscape.JwtUtils.TryGetSub(token);
if (string.IsNullOrEmpty(userId))
{
Debug.LogError("[DeepLinkAuth] JWT missing 'sub'; cannot resolve user.");
return;
}

// Use the existing OmniscapeAPI helper
Omniscape.UserRecord me = null;
try
{
me = await Omniscape.OmniscapeAPI.FetchUserById(userId);
}
catch (Exception e)
{
Debug.LogError("[DeepLinkAuth] GetById failed: " + e.Message);
}

if (me == null)
{
Debug.LogWarning("[DeepLinkAuth] GetById returned null for sub=" + userId);
return;
}

Omniscape.GlobalUserCache.Set(me);
            Omniscape.GlobalUserCache.Set(me);
            Debug.Log($"[DeepLinkAuth] ✅ Logged in as {me.username} ({me.email})");

            // Advance your app now
            if (!string.IsNullOrWhiteSpace(targetSceneName))
            {
                Debug.Log($"[DeepLinkAuth] Loading scene '{targetSceneName}'");
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("[DeepLinkAuth] targetSceneName is not set; staying in current scene.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[DeepLinkAuth] Error handling deep link: " + ex);
        }
    }
}
