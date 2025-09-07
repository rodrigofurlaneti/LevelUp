namespace LevelUpClone.Api.Contracts.Requests
{
    public sealed class UpsertUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
