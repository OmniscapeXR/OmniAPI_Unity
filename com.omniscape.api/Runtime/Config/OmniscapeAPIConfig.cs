using UnityEngine;

[CreateAssetMenu(fileName = "OmniscapeAPIConfig", menuName = "Scriptable Objects/OmniscapeAPIConfig")]
public class OmniscapeAPIConfig : ScriptableObject
{
    [Header("Base URLs")]
    public string apiBase = "https://prod.core.omniscape.com";
    public string webLoginBase = "https://marketplace.omniscape.com/";
    public string marketplaceApiBase = "https://marketplace.omniscape.com"; 

    [Header("Environment")]
    public bool useStaging;
    public string apiBaseStaging = "https://dev.core.omniscape.com";
    public string webLoginBaseStaging = "https://marketplace-staging.omniscape.com";
    public string marketplaceApiBaseStaging = "https://marketplace-staging.omniscape.com";
    
    [Header("Realtime (WebSocket)")]
    public string locationWsUrl = "wss://objects-near-player-459029072327.us-east1.run.app/";
    public string locationWsUrlStaging = "wss://objects-near-player-459029072327.us-east1.run.app/";
    public string LocationWsUrl => useStaging ? locationWsUrlStaging : locationWsUrl;
    
    public string ApiBase => useStaging ? apiBaseStaging : apiBase;
    public string WebLoginBase => useStaging ? webLoginBaseStaging : webLoginBase;
    public string MarketplaceApiBase => useStaging ? marketplaceApiBaseStaging : marketplaceApiBase;
}
