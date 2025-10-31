#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

public sealed class AndroidKeystoreTokenStore : ITokenStore {
    AndroidJavaObject _plugin;

    public AndroidKeystoreTokenStore() {
        var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var ctx = unity.GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getApplicationContext");
        var clazz = new AndroidJavaClass("com.omniscape.secure.OmniSecure");
        clazz.CallStatic("init", ctx);
        _plugin = clazz;
    }

    string Get(string k) => _plugin.CallStatic<string>("get", k);
    void Set(string k, string v) { if (string.IsNullOrEmpty(v)) _plugin.CallStatic("del", k); else _plugin.CallStatic("set", k, v); }

    public string AccessToken { get => Get("access"); set => Set("access", value); }
    public string RefreshToken{ get => Get("refresh"); set => Set("refresh", value); }
    public string UserId      { get => Get("userid");  set => Set("userid",  value); }
    public void Clear() { _plugin.CallStatic("del", "access"); _plugin.CallStatic("del", "refresh"); _plugin.CallStatic("del", "userid"); }
}
#endif