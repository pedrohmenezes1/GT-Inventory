using GTInventory.Application.DTOs.Auth;
using GTInventory.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GTInventory.Application.Services;

public class LdapUserService : IUserService
{
    private readonly IConfiguration _config;

    public LdapUserService(IConfiguration config)
    {
        _config = config;
    }

    public LoginResponse Authenticate(LoginRequest request)
    {
        var domain = _config["Ldap:Domain"];
        var ldapServer = _config["Ldap:Host"];
        var ldapPort = int.Parse(_config["Ldap:Port"]);

        try
        {
            using var connection = new LdapConnection();
            connection.Connect(ldapServer, ldapPort);
            connection.Bind($"{domain}\\{request.Username}", request.Password);

            var token = GenerateJwtToken(request.Username);

            return new LoginResponse
            {
                Username = request.Username,
                Token = token
            };
        }
        catch
        {
            throw new UnauthorizedAccessException("Usuário ou senha inválidos no domínio");
        }
    }

    private string GenerateJwtToken(string username)
    {
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
