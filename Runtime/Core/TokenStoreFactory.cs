using UnityEngine;

public static class TokenStoreFactory {
    public static ITokenStore Create() {
#if UNITY_IOS && !UNITY_EDITOR
    return new KeychainTokenStore();
#elif UNITY_ANDROID && !UNITY_EDITOR
    return new AndroidKeystoreTokenStore();
#else
        return new PlayerPrefsTokenStore(); // Editor/Standalone fallback
#endif
    }
}