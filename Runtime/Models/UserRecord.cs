// Plugins/Runtime/Models/UserRecord.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Omniscape
{
    /// <summary>
    /// User record model for Omniscape API, compatible with Unity's JsonUtility.
    /// </summary>
    [Serializable]
    public class UserRecord
    {
        // Primary identifiers
        public string _id;
        public string id;
        public string uid;

        // Computed property (not serialized)
        public string UserId =>
            !string.IsNullOrEmpty(id)  ? id  :
            !string.IsNullOrEmpty(_id) ? _id :
            uid; // fallback

        // General user fields
        public string email;
        public string username;
        public string profile_pic;
        public string maptheme;
        public string world_status;
        public string ts_created;
        public string ts_updated;

        // State
        public bool blocked;
        public bool deleted;
        public int credits;

        // Lists
        public List<string> friends;
        public List<string> roles;
        public List<CampaignSub> campaigns;
        public List<OnboardingItem> onboarding;
        public List<int> interactions_per_session;
    }

    [Serializable]
    public class CampaignSub
    {
        public string id;
        public bool is_subscribed_to;
    }

    [Serializable]
    public class OnboardingItem
    {
        public string id_name;
        public bool is_complete;
    }

    [Serializable]
    public class UserWrapper
    {
        public UserRecord message;
        public bool success;
    }
}