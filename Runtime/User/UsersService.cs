// OmniAPI/Runtime/User/UsersService.cs
using System.Threading.Tasks;
using Omniscape.API.Core;
using UnityEngine;

namespace Omniscape
{
    /// <summary>
    /// User API service using Unity's JsonUtility (via JsonUtils).
    /// </summary>
    public sealed class UsersService
    {
        private readonly HttpClientU _http;
        public UsersService(HttpClientU http) { _http = http; }

        // GET /auth/me
        public async Task<UserRecord> GetMe()
        {
            var json = await _http.Get("/auth/me");
            var wrapper = JsonUtils.FromJson<UserWrapper>(json);
            return wrapper?.message;
        }

        // GET /user?id={id}
        public async Task<UserRecord> GetById(string id)
        {
            var path = "/user?id=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(id);
            var json = await _http.Get(path);
            var wrapper = JsonUtils.FromJson<UserWrapper>(json);
            return wrapper?.message;
        }
    }
}