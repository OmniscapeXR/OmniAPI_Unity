using System;
using System.Text;

namespace Omniscape
{
    public static class JwtUtils
    {
        public static string TryGetSub(string jwt)
        {
            try
            {
                if (string.IsNullOrEmpty(jwt)) return null;
                var parts = jwt.Split('.');
                if (parts.Length < 2) return null;

                // Base64Url decode payload (parts[1])
                var payload = Base64UrlDecode(parts[1]);

                // super-light parse: look for "sub":"..."
                const string marker = "\"sub\":\"";
                var i = payload.IndexOf(marker, StringComparison.Ordinal);
                if (i < 0) return null;
                i += marker.Length;
                var j = payload.IndexOf('"', i);
                if (j < 0) return null;
                return payload.Substring(i, j - i);
            }
            catch { return null; }
        }

        static string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
            var bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}