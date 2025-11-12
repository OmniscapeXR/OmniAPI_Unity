using UnityEngine;

public sealed class PlayerPrefsTokenStore : ITokenStore {
    const string AK = "omni.at", RK = "omni.rt", UID = "omni.uid";
    public string AccessToken { get => PlayerPrefs.GetString(AK, null); set { Set(AK, value); } }
    public string RefreshToken { get => PlayerPrefs.GetString(RK, null); set { Set(RK, value); } }
    public string UserId       { get => PlayerPrefs.GetString(UID, null); set { Set(UID, value); } }
    public void Clear() { PlayerPrefs.DeleteKey(AK); PlayerPrefs.DeleteKey(RK); PlayerPrefs.DeleteKey(UID); PlayerPrefs.Save(); }
    static void Set(string k, string v){ if (string.IsNullOrEmpty(v)) PlayerPrefs.DeleteKey(k); else PlayerPrefs.SetString(k, v); PlayerPrefs.Save(); }
}