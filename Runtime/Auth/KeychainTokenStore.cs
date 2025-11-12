#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;

public sealed class KeychainTokenStore : ITokenStore {
    const string Svc = "com.omniscape.sdk";

    public string AccessToken { get => kc_get(Svc, "access"); set => kc_set(Svc, "access", value); }
    public string RefreshToken { get => kc_get(Svc, "refresh"); set => kc_set(Svc, "refresh", value); }
    public string UserId      { get => kc_get(Svc, "userid");  set => kc_set(Svc, "userid", value); }
    public void Clear() { kc_del(Svc,"access"); kc_del(Svc,"refresh"); kc_del(Svc,"userid"); }

    [DllImport("__Internal")] static extern void kc_set(string service, string key, string value);
    [DllImport("__Internal")] static extern string kc_get(string service, string key);
    [DllImport("__Internal")] static extern void kc_del(string service, string key);
}
#endif