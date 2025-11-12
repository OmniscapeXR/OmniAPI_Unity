using UnityEngine;
using UnityEngine.Events;

namespace Omniscape.UI
{
    /// <summary>
    /// OmniscapeLoginView
    /// Minimal login view bridge that triggers the web login flow and raises an event on success.
    /// Your project can wire this to an in-app WebView; by default we fall back to opening the login URL.
    /// </summary>
    public class OmniscapeLoginView : MonoBehaviour
    {
        [System.Serializable]
        public class UserRecordEvent : UnityEvent<Omniscape.UserRecord> {}

        [Header("Events")]
        public UserRecordEvent OnLoginSuccess = new UserRecordEvent();

        /// <summary>
        /// Starts the login flow. Default behavior opens the web login URL in an external browser.
        /// If you have an in-app WebView, call your WebView here and route the token to CompleteLogin.
        /// </summary>
        public void Show()
        {
            var baseUrl = OmniscapeAPI.WebLoginBase;
            if (!string.IsNullOrEmpty(baseUrl))
            {
                Application.OpenURL(baseUrl);
            }
            else
            {
                Debug.LogWarning("[OmniscapeLoginView] WebLoginBase not set in config.");
            }
        }

        /// <summary>
        /// Call this after your WebView receives a valid token (and optional user id) to complete login.
        /// This method fetches the user record and fires OnLoginSuccess.
        /// </summary>
        public async void CompleteLogin(string accessToken, string userId = null)
        {
            if (!string.IsNullOrEmpty(accessToken))
                OmniscapeAPI.SetAccessToken(accessToken);

            Omniscape.UserRecord me = null;
            try
            {
                // Prefer /auth/me
                me = await OmniscapeAPI.FetchMe();
                if (me == null && !string.IsNullOrEmpty(userId))
                    me = await OmniscapeAPI.FetchUserById(userId);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[OmniscapeLoginView] Fetch user failed: {e.GetType().Name} {e.Message}");
            }

            if (me != null)
            {
                OnLoginSuccess?.Invoke(me);
            }
            else
            {
                Debug.LogWarning("[OmniscapeLoginView] Login not completed; user record could not be fetched.");
            }
        }
    }
}
