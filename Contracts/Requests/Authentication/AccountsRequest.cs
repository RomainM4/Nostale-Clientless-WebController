namespace NosCore.AuthApi.Contracts
{
    public class AccountsRequest
    {
        public required string Token { get; set; }
        public required string InstallationId { get; set; }
    }
}
