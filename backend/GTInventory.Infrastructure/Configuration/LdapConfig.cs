namespace GTInventory.Infrastructure.Configuration
{
    public class LdapConfig
    {
        public required string Server { get; set; }
        public required int Port { get; set; }
        public required string BaseDn { get; set; }
        public required string Domain { get; set; }
        public required bool UseSSL { get; set; }
        public required bool IgnoreCertificateErrors { get; set; }
        public required string UserNameFormat { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Server))
                throw new ArgumentNullException(nameof(Server));
            
            if (Port is < 1 or > 65535)
                throw new ArgumentOutOfRangeException(nameof(Port));
        }
    }
}