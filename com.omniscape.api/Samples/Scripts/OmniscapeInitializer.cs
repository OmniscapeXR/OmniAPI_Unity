using System;
using Omniscape;
using Omniscape.UI;
using Omniscape.API.Core;        // HttpClientU, Json utils, etc.
using Omniscape.API.Inventory;
using Omniscape.API.Models.Realtime; // InventoryService
using Omniscape.API.Realtime;
using UnityEngine;

public class OmniscapeInitializer : MonoBehaviour
{
    [Header("SDK Config Asset")]
    public OmniscapeAPIConfig config;

    [Header("Optional Login View Prefab")]
    [Tooltip("If left empty, the initializer will try to find an OmniscapeLoginView in the scene.")]
    public OmniscapeLoginView loginViewPrefab;

    [Header("Editor-only Dev Login UI")]
    public GameObject editorLoginPanel;

    [Tooltip("When ON (Editor only), shows the paste-token panel instead of opening the browser.")]
    public bool useEditorPanelInEditor = false;

    private OmniscapeLoginView _loginInstance;

    // keep a reference so we can close the socket on quit/destroy
    private LocationSocket _locSocket;

    void Awake()
    {
        if (config == null) { Debug.LogError("[OmniscapeInitializer] No config assigned!"); return; }

        OmniscapeAPI.Initialize(config);
        Debug.Log($"[OmniscapeInitializer] Omniscape SDK initialized with base: {OmniscapeAPI.ApiBase}");

#if UNITY_EDITOR
        if (useEditorPanelInEditor && editorLoginPanel != null)
        {
            editorLoginPanel.SetActive(true);
            StartLoginFlow();
            return;
        }
#endif
        StartLoginFlow();
    }

    private void StartLoginFlow()
    {
        if (_loginInstance != null) return;

        _loginInstance = FindObjectOfType<OmniscapeLoginView>();
        if (_loginInstance == null)
        {
            if (loginViewPrefab != null) _loginInstance = Instantiate(loginViewPrefab);
            else { Debug.LogWarning("[OmniscapeInitializer] No OmniscapeLoginView found or prefab assigned; cannot start login."); return; }
        }

        _loginInstance.OnLoginSuccess.AddListener(OnLoginSuccess);
        _loginInstance.Show();
    }

#if UNITY_EDITOR
    public async void ContinueWithDevCredentials(string bearerToken, string userId)
    {
        if (!string.IsNullOrEmpty(bearerToken))
            OmniscapeAPI.SetAccessToken(bearerToken);

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[OmniscapeInitializer] User ID is required (no /auth/me endpoint on backend).");
            return;
        }

        UserRecord me = null;
        try
        {
            var url = $"{OmniscapeAPI.ApiBase}/user?id={UnityEngine.Networking.UnityWebRequest.EscapeURL(userId)}";
            Debug.Log($"[OmniscapeInitializer] FetchUserById → {url}");
            me = await OmniscapeAPI.FetchUserById(userId);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[OmniscapeInitializer] Fetch by id failed: {e.GetType().Name} → {e.Message}");
        }

        if (me != null)
        {
            if (editorLoginPanel) editorLoginPanel.SetActive(false);
            OnLoginSuccess(me);
        }
        else
        {
            Debug.LogWarning("[OmniscapeInitializer] Could not validate dev credentials. Check token/user id.");
        }
    }
#endif

    /// <summary>
    /// Called when login completes. Sets user cache, verifies token, then:
    /// 1) fetches user inventory (Marketplace API)
    /// 2) connects location websocket and starts the Unity GPS sender
    /// </summary>
    private async void OnLoginSuccess(UserRecord me)
    {
        if (me == null) { Debug.LogError("[OmniscapeInitializer] Login callback returned a null user record."); return; }

        GlobalUserCache.Set(me);
        var uid = me.UserId;
        Debug.Log($"[OmniscapeInitializer] ✅ Logged in as {me.username} ({me.email}) | id={uid}");

        var token = OmniscapeAPI.TokenStore?.AccessToken;
        if (string.IsNullOrEmpty(token))
            Debug.LogWarning("[OmniscapeInitializer] ⚠️ No access token found after login. Check WebView token handoff or token persistence.");
        else
            Debug.Log($"[OmniscapeInitializer] Token present (len {token.Length}) • head: {token.Substring(0, Mathf.Min(10, token.Length))}…");

        // --- Inventory from Marketplace host -----------------------------------
        var httpMarket = new HttpClientU(
            config.MarketplaceApiBase,
            () => OmniscapeAPI.TokenStore?.AccessToken
        );

        try
        {
            var invService = new InventoryService(httpMarket);
            var docs = await invService.GetUserInventoryAsync(uid);
            var items = docs.ConvertAll(InventoryService.ToItem);
            Debug.Log($"[OmniscapeInitializer] 🧳 Loaded {items.Count} inventory items from Marketplace.");
            // TODO: hand 'items' to your UI/inventory system
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OmniscapeInitializer] ❌ Inventory fetch failed → {ex.Message}");
        }
        // --- Location WebSocket -------------------------------------------------
        try
        {
            Debug.Log($"[OmniscapeInitializer] © Using WS URL from config: {config.LocationWsUrl}");

            // Create socket
            _locSocket = new LocationSocket(config.LocationWsUrl);
            
            _locSocket.OnEvent += (name, args) => 
                Debug.Log($"[LocationSocket] evt={name} args0={(args != null && args.Length > 0 ? args[0] : "<none>")}");


            // Start connect (fire-and-forget; do NOT await)
            _locSocket.Connect();

            // Optionally gate the next steps until the socket is open
            await _locSocket.WaitUntilReadyAsync(100000); // 10s timeout

            Debug.Log($"[OmniscapeInitializer] We are connected to socket!");
            

            // Service GameObject (don’t call DontDestroyOnLoad in Edit Mode)
            Debug.Log($"[OmniscapeInitializer] Creating Location GameObject");
            var go = new GameObject("LocationUpdateService");
            if (Application.isPlaying) DontDestroyOnLoad(go);

            var svc = go.AddComponent<LocationUpdateService>();
            await svc.Initialize(_locSocket, me.UserId);
            
            // First request after CONNECT
            Debug.Log($"[OmniscapeInitializer] Making Location Call!");
            await _locSocket.SendLocation(new LocationUpdateMsg {
                longitude   = -0.1909603,
                latitude    = 51.4818435,
                maxDistance = 1000
            });

            Debug.Log("[OmniscapeInitializer] 📡 Location streaming started.");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[OmniscapeInitializer] Location stream failed: {ex.Message}");
        }


        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    // Ensure the socket stops reconnecting when play ends or this object is destroyed
    private void OnApplicationQuit()
    {
        if (Application.isPlaying)
            _locSocket?.CloseAsync();
    }
    private void OnDestroy()
    {
        if (_locSocket != null) _ = _locSocket.CloseAsync();
    }
    
}
