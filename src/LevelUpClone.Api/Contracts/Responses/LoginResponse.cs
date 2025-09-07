namespace LevelUpClone.Api.Contracts.Responses
{
    public sealed class LoginResponse 
    { 
        public string Token { get; set; } = string.Empty; 
        public string DisplayName { get; set; } = string.Empty; 
    }
}
