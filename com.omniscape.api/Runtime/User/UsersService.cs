using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Omniscape {
    public sealed class UsersService {
        readonly HttpClientU _http;
        public UsersService(HttpClientU http) { _http = http; }

        public async Task<UserRecord> GetMe() {
            var json = await _http.Get("/auth/me");        // preferred
            return JsonConvert.DeserializeObject<UserWrapper>(json)?.message;
        }
        public async Task<UserRecord> GetById(string id) {
            var json = await _http.Get("/user?id=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(id));
            return JsonConvert.DeserializeObject<UserWrapper>(json)?.message;
        }
    }
}