using Novell.Directory.Ldap;
using GTInventory.Domain.Interfaces;
using GTInventory.Application.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GTInventory.Application.Services
{
    public class LdapUserService : IUserService
    {
        private readonly IConfiguration _config;

        public LdapUserService(IConfiguration config)
        {
            _config = config;
        }

        public AuthenticationResult Authenticate(string username, string password)
        {
            try
            {
                var ldapConfig = _config.GetSection("Ldap");
                var domainUser = FormatUsername(username, ldapConfig["Domain"]);

                using var connection = new LdapConnection();
                connection.Connect(ldapConfig["Server"], int.Parse(ldapConfig["Port"]));
                connection.Bind(domainUser, password);

                var searchFilter = $"(sAMAccountName={username.Split('\\').Last()})";
                var result = connection.Search(
                    ldapConfig["BaseDn"],
                    LdapConnection.ScopeSub,
                    searchFilter,
                    new[] { "displayName", "mail" },
                    false
                );

                if (!result.HasMore())
                    return new AuthenticationResult("Usuário não encontrado");

                var userEntry = result.Next();
                var token = GenerateJwtToken(userEntry);

                return new AuthenticationResult(
                    new UserResult(
                        userEntry.GetAttribute("displayName").StringValue,
                        userEntry.GetAttribute("mail").StringValue
                    ),
                    token
                );
            }
            catch (LdapException ex)
            {
                return new AuthenticationResult(ex.ResultCode switch
                {
                    49 => "Credenciais inválidas",
                    81 => "Servidor AD indisponível",
                    _ => "Erro na autenticação"
                });
            }
        }

        private string GenerateJwtToken(LdapEntry userEntry)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, userEntry.GetAttribute("displayName").StringValue),
                    new Claim(ClaimTypes.Email, userEntry.GetAttribute("mail").StringValue)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenDescriptor);
        }

        private string FormatUsername(string username, string domain)
        {
            return username.Contains('@') ? username : $"{domain}\\{username}";
        }
    }

    public record AuthenticationResult(
        bool Authenticated = true,
        UserResult? User = null,
        string? Token = null,
        string? Message = null
    );

    public record UserResult(
        string DisplayName,
        string Email
    );
}