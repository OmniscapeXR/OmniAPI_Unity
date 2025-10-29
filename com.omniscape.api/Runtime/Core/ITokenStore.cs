using UnityEngine;

public interface ITokenStore
{
    string AccessToken { get; set; }     // short-lived; still persisted if that's all you have
    string RefreshToken { get; set; }    // if your backend provides one; else leave null
    string UserId { get; set; }          // convenience cache
    void Clear();
}
