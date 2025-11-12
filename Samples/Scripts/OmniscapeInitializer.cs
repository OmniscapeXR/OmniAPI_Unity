using System;
using UnityEngine;
using Omniscape;
using Omniscape.UI;

/// <summary>
/// Boots the Omniscape SDK, starts the login flow (WebView or external), and caches the logged-in user.
/// In the Editor, you can show a paste-token panel instead of launching WebView.
/// </summary>
public class OmniscapeInitializer : MonoBehaviour
{
    [Header("SDK Config Asset")]
    public OmniscapeAPIConfig config;

    [Header("Optional Login View Prefab")]
    [Tooltip("If left empty, the initializer will try to find an OmniscapeLoginView in the scene.")]
    public OmniscapeLoginView loginViewPrefab;

    [Header("Editor-only Dev Login UI")]
    public GameObject editorLoginPanel;

#if UNITY_EDITOR
    [Tooltip("When true in the Editor, shows the paste-token panel instead of launching the web login.")]
    public bool useEditorPanelInEditor = true;
#endif

    void Awake()
    {
        if (config == null)
        {
            Debug.LogError("[OmniscapeInitializer] Missing OmniscapeAPIConfig. Assign it on the component.");
            return;
        }

        // Initialize the SDK + token store
        OmniscapeAPI.Initialize(config, TokenStoreFactory.Create());
        Debug.Log("[OmniscapeInitializer] Omniscape SDK initialized with base: " + config.ApiBase);

#if UNITY_EDITOR
        // If an editor login panel is present and enabled, use it and DO NOT start web login
        if (useEditorPanelInEditor && editorLoginPanel != null)
        {
            editorLoginPanel.SetActive(true);
            return;
        }
#endif
        
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            Debug.Log("[Initializer] Launched via deep link; skipping external login.");
            return;
        }

        var existing = Omniscape.OmniscapeAPI.TokenStore?.AccessToken;
        if (!string.IsNullOrEmpty(existing))
        {
            Debug.Log("[Initializer] Token exists; resolving user and continuing.");
            _ = ContinueFromExistingToken();
            return;
        }
        
        StartLoginFlow();
    }

    /// <summary>
    /// Starts the login flow. If a login view exists, it will be used; otherwise falls back to external browser.
    /// </summary>
    public void StartLoginFlow()
    {
        var view = FindObjectOfType<OmniscapeLoginView>();
        if (view == null && loginViewPrefab != null)
        {
            view = Instantiate(loginViewPrefab);
        }

        if (view != null)
        {
            view.OnLoginSuccess.AddListener(OnLoginSuccess);
            view.Show();
        }
        else
        {
            Debug.LogWarning("[OmniscapeInitializer] No OmniscapeLoginView found. Opening external login URL.");
            OpenWebLogin();
        }
    }
    
    private void OpenWebLogin()
    {
        // Resolve the correct web login base (honor staging toggle when available on config)
        var baseUrl = config?.WebLoginBase;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            Debug.LogWarning("[OmniscapeInitializer] WebLoginBase not set in config.");
            return;
        }

        // Normalize and ensure scheme
        baseUrl = baseUrl.Trim();
        if (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "https://" + baseUrl.TrimStart('/');
        }

        // Always hit the explicit Unity entry so the site knows to deep-link back
        var loginUrl = baseUrl.TrimEnd('/') + "/login?from=unity";
        Debug.Log("[OmniscapeInitializer] Opening Web Login: " + loginUrl);
        Application.OpenURL(loginUrl);
    }
    
    private async System.Threading.Tasks.Task ContinueFromExistingToken()
    {
        var token = Omniscape.OmniscapeAPI.TokenStore?.AccessToken;
        if (string.IsNullOrEmpty(token))
        {
            OpenWebLogin();
            return;
        }

        // decode sub from JWT
        var userId = Omniscape.JwtUtils.TryGetSub(token);
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[Initializer] Token missing sub; signing out and relaunching login.");
            Omniscape.OmniscapeAPI.SignOut();
            OpenWebLogin();
            return;
        }

        try
        {
            var me = await Omniscape.OmniscapeAPI.FetchUserById(userId);
            if (me == null)
            {
                Debug.LogWarning("[Initializer] FetchUserById returned null; signing out.");
                Omniscape.OmniscapeAPI.SignOut();
                OpenWebLogin();
                return;
            }

            // good to go
            Omniscape.GlobalUserCache.Set(me);
            Debug.Log($"[Initializer] ✅ Resumed as {me.username} ({me.email})");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Initializer] ContinueFromExistingToken failed: " + e.Message);
            Omniscape.OmniscapeAPI.SignOut();
            OpenWebLogin();
        }
    }

    /// <summary>
    /// Called when the user has successfully logged in.
    /// </summary>
    private void OnLoginSuccess(UserRecord me)
    {
        if (me == null)
        {
            Debug.LogError("[OmniscapeInitializer] Login callback returned a null user record.");
            return;
        }

        GlobalUserCache.Set(me);
        var uid = me.UserId;
        Debug.Log($"[OmniscapeInitializer] ✅ Logged in as {me.username} ({me.email}) | id={uid}");

        var token = OmniscapeAPI.TokenStore?.AccessToken;
        if (string.IsNullOrEmpty(token))
            Debug.LogWarning("[OmniscapeInitializer] ⚠️ No access token found after login. Check WebView token handoff or token persistence.");
        else
            Debug.Log($"[OmniscapeInitializer] Token present (len {token.Length}) • head: {token.Substring(0, Math.Min(10, token.Length))}…");

        // TODO: Load your main scene here if desired
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only helper to bypass WebView. Paste a bearer token and optional user id.
    /// </summary>
    public async void ContinueWithDevCredentials(string bearerToken, string userId)
    {
        if (string.IsNullOrEmpty(bearerToken))
        {
            Debug.LogError("[OmniscapeInitializer] Dev token is empty.");
            return;
        }
        OmniscapeAPI.SetAccessToken(bearerToken);

        var http = new HttpClientU(config.ApiBase, () => OmniscapeAPI.TokenStore?.AccessToken);
        var users = new Omniscape.UsersService(http);

        try
        {
            UserRecord me = null;
            if (!string.IsNullOrEmpty(userId))
            {
                Debug.Log($"[OmniscapeInitializer] FetchUserById → {config.ApiBase}/user?id={UnityEngine.Networking.UnityWebRequest.EscapeURL(userId)}");
                me = await users.GetById(userId);
            }
            else
            {
                me = await users.GetMe();
            }
            if (me != null) OnLoginSuccess(me);
            else Debug.LogError("[OmniscapeInitializer] Dev login did not return a user record.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[OmniscapeInitializer] Fetch by id failed: {e.GetType().Name} → {e.Message}");
        }
    }
#else
    /// <summary>
    /// Dev-only login helper is not available at runtime builds. This stub exists to keep callers compiling.
    /// </summary>
    public void ContinueWithDevCredentials(string bearerToken, string userId)
    {
        Debug.LogWarning("[OmniscapeInitializer] ContinueWithDevCredentials is Editor-only. Ignoring call in player builds.");
    }
#endif
}
