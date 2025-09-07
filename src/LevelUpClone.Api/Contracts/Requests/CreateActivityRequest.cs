namespace LevelUpClone.Api.Contracts.Requests
{
    public sealed class CreateActivityRequest
    {
        public int UserId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string ActivityKind { get; set; } = "Task";
    }
}
