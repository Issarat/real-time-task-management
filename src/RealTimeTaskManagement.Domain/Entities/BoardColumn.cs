namespace RealTimeTaskManagement.Domain.Entities;

public sealed class BoardColumn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
