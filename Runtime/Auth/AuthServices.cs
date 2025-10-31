using System;
using UnityEngine;

namespace Omniscape
{
    public sealed class AuthService
    {
        private readonly ITokenStore _store;
        public event Action<string> OnTokenChanged;

        public AuthService(ITokenStore store) => _store = store;

        // 👇 Add this public property
        public ITokenStore TokenStore => _store;

        public string AccessToken => _store.AccessToken;

        public void SetAccessToken(string token)
        {
            _store.AccessToken = token;
            OnTokenChanged?.Invoke(token);
        }

        public void SignOut()
        {
            _store.Clear();
            OnTokenChanged?.Invoke(null);
        }
    }
}