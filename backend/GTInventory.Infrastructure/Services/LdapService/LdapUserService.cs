using Novell.Directory.Ldap;
using GTInventory.Domain.Models;
using GTInventory.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace GTInventory.Infrastructure.Services.LdapService
{
    public class LdapUserService : IUserService
    {
        private readonly LdapConfig _config;

        public LdapUserService(IOptions<LdapConfig> config)
        {
            _config = config.Value;
        }

        public AuthenticationResult Authenticate(string username, string password)
        {
            try
            {
                var formattedUsername = FormatUsername(username, _config.Domain);
                
                using var connection = new LdapConnection();
                
                connection.SecureSocketLayer = _config.UseSSL;
                if (_config.IgnoreCertificateErrors)
                {
                    connection.UserDefinedServerCertValidationDelegate += (sender, certificate, chain, errors) => true;
                }

                connection.Connect(_config.Server, _config.Port);
                connection.Bind(formattedUsername, password);

                var searchFilter = $"(sAMAccountName={ExtractUsername(username)})";
                var result = connection.Search(
                    _config.BaseDn,
                    LdapConnection.ScopeSub,
                    searchFilter,
                    new[] { "displayName", "mail", "userPrincipalName", "memberOf" },
                    false
                );

                if (!result.HasMore())
                    return new AuthenticationResult("Usuário não encontrado no diretório");

                var userEntry = result.Next();
                var token = GenerateJwtToken(userEntry);

                return new AuthenticationResult(
                    new UserResult(
                        userEntry.GetAttribute("displayName")?.StringValue ?? string.Empty,
                        userEntry.GetAttribute("mail")?.StringValue ?? string.Empty,
                        userEntry.GetAttribute("userPrincipalName")?.StringValue ?? string.Empty
                    ),
                    token
                );
            }
            catch (LdapException ex)
            {
                return ex.ResultCode switch
                {
                    49 => new AuthenticationResult("Credenciais inválidas"),
                    81 => new AuthenticationResult("Servidor AD indisponível"),
                    _ => new AuthenticationResult($"Erro de autenticação: {ex.Message}")
                };
            }
        }

        private string FormatUsername(string username, string domain)
        {
            if (username.Contains('\\') || username.Contains('@'))
                return username;

            return _config.UserNameFormat switch
            {
                "UPN" => $"{username}@{domain.ToLower()}.intra.net",
                _ => $"{domain}\\{username}"
            };
        }

        private string ExtractUsername(string input)
        {
            if (input.Contains('\\')) return input.Split('\\')[1];
            if (input.Contains('@')) return input.Split('@')[0];
            return input;
        }

        private string GenerateJwtToken(LdapEntry userEntry)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.JwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, userEntry.GetAttribute("displayName")?.StringValue ?? ""),
                    new Claim(ClaimTypes.Email, userEntry.GetAttribute("mail")?.StringValue ?? ""),
                    new Claim("upn", userEntry.GetAttribute("userPrincipalName")?.StringValue ?? "")
                }),
                Expires = DateTime.UtcNow.AddHours(_config.JwtExpirationHours),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenDescriptor);
        }
    }

    public class LdapConfig
    {
        public string Server { get; set; } = "retaguarda.intra.net";
        public int Port { get; set; } = 636;
        public string BaseDn { get; set; } = "DC=retaguarda,DC=intra,DC=net";
        public string Domain { get; set; } = "RETAGUARDA";
        public bool UseSSL { get; set; } = true;
        public bool IgnoreCertificateErrors { get; set; } = false;
        public string UserNameFormat { get; set; } = "SAMAccount";
        public string JwtSecret { get; set; }
        public int JwtExpirationHours { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Server))
                throw new ArgumentNullException(nameof(Server));
            
            if (Port is < 1 or > 65535)
                throw new ArgumentOutOfRangeException(nameof(Port));
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
        string Email,
        string UserPrincipalName
    );
}