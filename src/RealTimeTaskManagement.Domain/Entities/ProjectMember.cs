using RealTimeTaskManagement.Domain.Enums;

namespace RealTimeTaskManagement.Domain.Entities;

public sealed class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    public DateTimeOffset JoinedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
