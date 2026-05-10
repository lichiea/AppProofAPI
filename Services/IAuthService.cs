using System.Net.Http.Headers;
namespace ProofAPI.Services
{
    public interface IAuthService
    {
        AuthenticationHeaderValue GetAuthHeader();
        void SetBasicAuth(string username, string password);
        void SetBearerToken(string token);
    }
}