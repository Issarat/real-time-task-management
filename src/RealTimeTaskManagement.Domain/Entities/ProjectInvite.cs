using RealTimeTaskManagement.Domain.Enums;

namespace RealTimeTaskManagement.Domain.Entities;

public sealed class ProjectInvite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string CodeHash { get; set; } = string.Empty;
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
