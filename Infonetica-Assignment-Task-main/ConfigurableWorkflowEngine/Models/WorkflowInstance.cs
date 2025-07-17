namespace ConfigurableWorkflowEngine.Models
{
    public class WorkflowInstance
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string WorkflowDefinitionId { get; set; } = string.Empty;
        public string CurrentStateId { get; set; } = string.Empty;
        public List<ActionHistory> History { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted => CompletedAt.HasValue;
    }

    public class ActionHistory
    {
        public string ActionId { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string FromStateId { get; set; } = string.Empty;
        public string ToStateId { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }
}
