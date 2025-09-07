namespace LevelUpClone.Api.Contracts.Requests
{
    public sealed class LogFundamentalRequest
    {
        public int UserId { get; set; }
        public string FundamentalCode { get; set; } = string.Empty;
        public string ActivityDate { get; set; } = string.Empty;
        public string? NotesText { get; set; }
    }
}
