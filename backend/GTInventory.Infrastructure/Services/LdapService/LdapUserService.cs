using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using GTInventory.Domain.Models;
using GTInventory.Domain.Interfaces;
using GTInventory.Infrastructure.Configuration;

namespace GTInventory.Infrastructure.Services.LdapService
{
    public class LdapUserService : IUserService
    {
        private readonly LdapConfig _ldapConfig;
        private readonly JwtConfig _jwtConfig;
        private readonly ILogger<LdapUserService> _logger;

        public LdapUserService(
            IOptions<LdapConfig> ldapConfig,
            IOptions<JwtConfig> jwtConfig,
            ILogger<LdapUserService> logger)
        {
            _ldapConfig = ldapConfig.Value;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
            ValidateConfigurations();
        }

        private void ValidateConfigurations()
        {
            _ldapConfig.Validate();
            if (string.IsNullOrWhiteSpace(_jwtConfig.Secret))
                throw new ArgumentNullException(nameof(_jwtConfig.Secret), "JwtConfig.Secret não pode ser nulo ou vazio");
        }

        public AuthenticationResult Authenticate(string username, string password)
        {
            try
            {
                using var ldap = new LdapConnection(new LdapDirectoryIdentifier(_ldapConfig.Server, _ldapConfig.Port))
                {
                    AuthType = AuthType.Negotiate
                };

                var credential = new NetworkCredential(FormatUsername(username), password);

                if (_ldapConfig.IgnoreCertificateErrors)
                {
                    ldap.SessionOptions.VerifyServerCertificate += (conn, cert) => true;
                }

                ldap.Bind(credential);

                var searchFilter = $"(sAMAccountName={ExtractUsername(username)})";
                var request = new SearchRequest(
                    _ldapConfig.BaseDn,
                    searchFilter,
                    SearchScope.Subtree,
                    new[] { "displayName", "mail", "userPrincipalName" }
                );

                var response = (SearchResponse)ldap.SendRequest(request);

                if (response.Entries.Count == 0)
                    return new AuthenticationResult(false, null, null, "Usuário não encontrado");

                var entry = response.Entries[0];

                var user = new UserResult(
                    entry.Attributes["displayName"]?[0]?.ToString() ?? string.Empty,
                    entry.Attributes["mail"]?[0]?.ToString() ?? string.Empty,
                    entry.Attributes["userPrincipalName"]?[0]?.ToString() ?? string.Empty
                );

                var token = GenerateJwtToken(user);
                return new AuthenticationResult(true, user, token, null);
            }
            catch (LdapException ex)
            {
                _logger.LogError(ex, "Erro de autenticação LDAP para {Username}", username);
                return new AuthenticationResult(false, null, null, "Credenciais inválidas ou erro de conexão");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Erro crítico durante autenticação");
                return new AuthenticationResult(false, null, null, "Erro interno no servidor");
            }
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
        {
            return await Task.Run(() => Authenticate(username, password));
        }

        private string FormatUsername(string username)
        {
            if (username.Contains("\\") || username.Contains("@"))
                return username;

            return _ldapConfig.UserNameFormat switch
            {
                "UPN" => $"{username}@{_ldapConfig.Domain.ToLower()}.intra.net",
                _ => $"{_ldapConfig.Domain}\\{username}"
            };
        }

        private string ExtractUsername(string input)
        {
            if (input.Contains("\\")) return input.Split('\\')[1];
            if (input.Contains("@")) return input.Split('@')[0];
            return input;
        }

        private string GenerateJwtToken(UserResult user)
        {
            var secret = _jwtConfig.Secret ?? throw new InvalidOperationException("JwtConfig.Secret não foi configurado");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("upn", user.UserPrincipalName)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtConfig.ExpirationHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
