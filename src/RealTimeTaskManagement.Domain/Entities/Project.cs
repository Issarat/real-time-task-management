namespace RealTimeTaskManagement.Domain.Entities;

public sealed class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<ProjectInvite> Invites { get; set; } = [];
    public ICollection<BoardColumn> Columns { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
