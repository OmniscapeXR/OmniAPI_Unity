using UnityEngine;

public class WebViewHookExample : MonoBehaviour {
    public OmniscapeSdkConfig config;

    void Awake() {
        Omniscape.OmniscapeSdk.Initialize(config);
    }

    // Call this from your UniWebView OnMessageReceived handler:
    public async void OnWebViewAuthToken(string token, string userId = null) {
        Omniscape.OmniscapeSdk.SetAccessToken(token);

        Omniscape.UserRecord me = null;
        try {
            // Prefer /auth/me if available; else fall back to /user?id=...
            me = await Omniscape.OmniscapeSdk.FetchMe();
            if (me == null && !string.IsNullOrEmpty(userId))
                me = await Omniscape.OmniscapeSdk.FetchUserById(userId);
        } catch (System.Exception e) {
            Debug.LogError("[OmniscapeSDK] Fetch user failed: " + e);
        }

        // TODO: raise your project GameEvents here or assign to a global cache
        // Example: GlobalUserCache.Current = me;
    }
}