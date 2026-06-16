using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProofAPI.Services
{
    public class AuthService : IAuthService
    {
        private AuthenticationHeaderValue? _authHeader;

        public void SetBasicAuth(string username, string password)
        {
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
            _authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public void SetBearerToken(string token)
        {
            _authHeader = new AuthenticationHeaderValue("Bearer", token);
        }

        public Task AddAuthHeaderAsync(HttpRequestMessage request)
        {
            if (_authHeader != null)
                request.Headers.Authorization = _authHeader;
            return Task.CompletedTask;
        }
    }
}