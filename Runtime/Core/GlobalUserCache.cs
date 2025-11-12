using UnityEngine;


namespace Omniscape
{
    public static class GlobalUserCache
    {
        public static UserRecord Current;

        const string KeyUserId = "omni.user.id";

        public static void Set(UserRecord u)
        {
            Current = u;
            var id = u?.UserId;
            if (!string.IsNullOrEmpty(id))
            {
                PlayerPrefs.SetString(KeyUserId, id);
                PlayerPrefs.Save();
            }
        }

        public static string CurrentUserId =>
            Current?.UserId ?? PlayerPrefs.GetString(KeyUserId, null);

        public static void Clear()
        {
            Current = null;
            PlayerPrefs.DeleteKey(KeyUserId);
        }
    }
}
