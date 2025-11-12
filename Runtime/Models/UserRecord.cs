using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Omniscape
{
    [System.Serializable]
    public class UserRecord
    {
        [JsonProperty("_id")] public string _id;
        public string UserId =>
            !string.IsNullOrEmpty(id)  ? id  :
            !string.IsNullOrEmpty(_id) ? _id :
            uid; // fallback
        public string id, email, username, uid, profile_pic, maptheme, world_status, ts_created, ts_updated;
        public bool blocked, deleted;
        public int credits;
        public List<string> friends, roles;
        public List<CampaignSub> campaigns;
        public List<OnboardingItem> onboarding;
        public List<int> interactions_per_session;
    }
    
    public class CampaignSub { public string id; [JsonProperty("is_subscribed_to")] public bool is_subscribed_to; }
    public class OnboardingItem { public string id_name; public bool is_complete; }
    class UserWrapper { public UserRecord message; public bool success; }
}

