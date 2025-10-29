using System;
using Omniscape;
using UnityEngine;
using UnityEngine.Events;

// Payload event so you can wire it from the Inspector
[Serializable] public class UserEvent : UnityEvent<UserRecord> { }


namespace Omniscape.UI
{
  public class OmniscapeLoginView : MonoBehaviour
  {
    [Header("URL")]
    [Tooltip("Relative path after WebLoginBase, e.g. \"login?from=unity\"")]
    public string loginPath = "login?from=unity";
    [Tooltip("Optional iOS-only query append, e.g. \"ios=1\"")]
    public string iosAddition = "";

    [Header("Events")]
    public UserEvent OnLoginSuccess;
    public UnityEvent OnLogout;
    public UnityEvent OnClosed;
    public UnityEvent OnError;

    [Header("Options")]
    public bool openLinksExternally = true;   // allow external links in the page to open system browser

#if OMNI_USE_UNIWEBVIEW
    private UniWebView _webView;
#endif

    string BuildUrl()
    {
      var baseUrl = OmniscapeAPI.WebLoginBase?.TrimEnd('/') ?? "";
      var lp = loginPath?.TrimStart('/') ?? "";
#if UNITY_IOS
      if (!string.IsNullOrEmpty(iosAddition)) {
        var sep = lp.Contains("?") ? "&" : "?";
        lp = $"{lp}{sep}{iosAddition.TrimStart('?')}";
      }
#endif
      return $"{baseUrl}/{lp}";
    }

    public void Show()
    {
      var url = BuildUrl();

#if UNITY_EDITOR || UNITY_STANDALONE
      Debug.Log($"[OmniscapeLoginView] Editor/Desktop: opening system browser → {url}");
      Application.OpenURL(url);
      OnClosed?.Invoke();
      return;
#else
  #if OMNI_USE_UNIWEBVIEW
      if (_webView != null) Close();

      var go = new GameObject("OmniscapeWebView");
      _webView = go.AddComponent<UniWebView>();
      _webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
      _webView.SetVerticalScrollBarEnabled(false);
      _webView.SetOpenLinksInExternalBrowser(openLinksExternally);
      _webView.AddUrlScheme("omniscape"); // expect omniscape://auth?token=...

      _webView.OnShouldClose += (view) => { Close(); return true; };
      _webView.OnMessageReceived += HandleMessage;

      Debug.Log($"[OmniscapeLoginView] Loading {url}");
      _webView.Load(url);
      _webView.Show();
  #else
      Debug.LogWarning("[OmniscapeLoginView] OMNI_USE_UNIWEBVIEW not defined. Opening system browser.");
      Application.OpenURL(url);
      OnClosed?.Invoke();
  #endif
#endif
    }

#if OMNI_USE_UNIWEBVIEW
    private async void HandleMessage(UniWebView view, UniWebViewMessage message)
    {
      if (!message.Path.Equals("auth", StringComparison.OrdinalIgnoreCase))
        return;

      try
      {
        if (message.Args.TryGetValue("token", out var token) && !string.IsNullOrEmpty(token))
        {
          // 1) Hand token to SDK
          OmniscapeSdk.SetAccessToken(token);

          // 2) Fetch user
          UserRecord me = null;

          try { me = await OmniscapeSdk.FetchMe(); }
          catch { /* /auth/me may not exist yet */ }

          if (me == null && message.Args.TryGetValue("id", out var id) && !string.IsNullOrEmpty(id))
          {
            try { me = await OmniscapeSdk.FetchUserById(id); } catch { }
          }

          if (me != null) OnLoginSuccess?.Invoke(me);
          else OnError?.Invoke();

          Close();
          return;
        }

        // Optional: logout signal from page
        if (message.Args.TryGetValue("logout", out var _))
        {
          OmniscapeSdk.SignOut();
          OnLogout?.Invoke();
          Close();
          return;
        }
      }
      catch (Exception e)
      {
        Debug.LogError("[OmniscapeLoginView] Error handling auth message: " + e);
        OnError?.Invoke();
        Close();
      }
    }
#endif

    public void Close()
    {
#if OMNI_USE_UNIWEBVIEW
      if (_webView != null) {
        var go = _webView.gameObject;
        _webView = null;
        Destroy(go);
      }
#endif
      OnClosed?.Invoke();
    }

    private void OnDisable() => Close();
  }
}
