namespace LevelUpClone.Api.Contracts.Responses
{
    public sealed class ActivityResponse 
    { 
        public int ActivityId { get; set; } 
        public string ActivityName { get; set; } = string.Empty; 
        public int ActivityKind { get; set; } 
        public int DefaultPoints { get; set; } 
        public bool IsActive { get; set; } 
    }

}
