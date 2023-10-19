using APICatalogo_minimal.Models;

namespace APICatalogo_minimal.Services;

public interface ITokenService
{
    string Createtoken(string key, string issuer, string audience, UserModel user);
}
