using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Omniscape
{
    public static class OmniscapeAPI
    {
        static WorldsService _worlds;
        static OmniscapeAPIConfig _cfg;
        static AuthService _auth;
        static UsersService _users;

        public static void Initialize(OmniscapeAPIConfig config, ITokenStore tokenStore = null)
        {
            _cfg = config;
            // ✅ Use platform store (Keychain/Keystore/PlayerPrefs)
            _auth = new AuthService(tokenStore ?? TokenStoreFactory.Create());

            var http = new HttpClientU(_cfg.ApiBase, () => _auth.AccessToken);
            _users = new UsersService(http);
            _worlds = new WorldsService(http);

            Debug.Log($"[OmniscapeAPI] Initialized with base: {_cfg.ApiBase}");
        }

        public static string ApiBase => _cfg?.ApiBase;
        public static string WebLoginBase => _cfg?.WebLoginBase;

        public static WorldsService Worlds => _worlds;

        public static void SetAccessToken(string token) => _auth.SetAccessToken(token);
        public static void SignOut() => _auth.SignOut();

        // ✅ Expose TokenStore so you can sanity-check the token in OnLoginSuccess
        public static ITokenStore TokenStore => _auth?.TokenStore;

        public static Task<UserRecord> FetchMe() => _users.GetMe();
        public static Task<UserRecord> FetchUserById(string id) => _users.GetById(id);

        public static event Action<string> OnTokenChanged
        {
            add { if (_auth != null) _auth.OnTokenChanged += value; }
            remove { if (_auth != null) _auth.OnTokenChanged -= value; }
        }
    }
}

