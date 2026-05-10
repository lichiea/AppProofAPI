using System.Net.Http.Headers;
namespace ProofAPI.Services
{
    public class AuthService : IAuthService
    {
        private AuthenticationHeaderValue? _header;
        public AuthenticationHeaderValue GetAuthHeader() => _header;
        public void SetBasicAuth(string username, string password)
        {
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
            _header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
        public void SetBearerToken(string token) => _header = new AuthenticationHeaderValue("Bearer", token);
    }
}