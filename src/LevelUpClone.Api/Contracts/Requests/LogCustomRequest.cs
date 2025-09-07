namespace LevelUpClone.Api.Contracts.Requests
{
    public sealed class LogCustomRequest 
    { 
        public int UserId { get; set; } 
        public int ActivityId { get; set; } 
        public string ActivityDate { get; set; } = ""; 
        public string? NotesText { get; set; } 
    }

}
