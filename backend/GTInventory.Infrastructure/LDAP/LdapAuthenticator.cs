using Novell.Directory.Ldap;

namespace GTInventory.Infrastructure.LDAP;

public class LdapAuthenticator
{
    public bool Authenticate(string ldapHost, int ldapPort, string userDn, string password)
    {
        try
        {
            using var connection = new LdapConnection();
            connection.Connect(ldapHost, ldapPort);
            connection.Bind(userDn, password);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
