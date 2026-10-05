using RealTimeTaskManagement.Domain.Enums;

namespace RealTimeTaskManagement.Application.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectSummary>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<CreatedProject> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default);

    Task<JoinProjectResult> JoinAsync(
        JoinProjectRequest request,
        CancellationToken cancellationToken = default);

    Task<ProjectBoard?> GetBoardAsync(
        string projectSlug,
        string userId,
        CancellationToken cancellationToken = default);

    Task<CreatedProjectTask?> CreateTaskAsync(
        CreateProjectTaskRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    string CreatedByUserId);

public sealed record CreatedProject(
    Guid Id,
    string Name,
    string Slug);

public sealed record JoinProjectRequest(
    string InviteCode,
    string UserId);

public sealed record JoinProjectResult(
    JoinProjectStatus Status,
    Guid? ProjectId = null,
    string? ProjectName = null);

public enum JoinProjectStatus
{
    Joined,
    AlreadyMember,
    InvalidCode,
    Expired,
    Revoked,
    UsageLimitReached
}

public sealed record ProjectSummary(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    ProjectRole Role,
    int MemberCount,
    DateTimeOffset CreatedAtUtc);

public sealed record ProjectBoard(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    IReadOnlyList<ProjectBoardMember> Members,
    IReadOnlyList<ProjectBoardColumn> Columns);

public sealed record ProjectBoardMember(
    string UserId,
    string DisplayName,
    string Email,
    ProjectRole Role);

public sealed record ProjectBoardColumn(
    Guid Id,
    string Name,
    string Key,
    int SortOrder,
    IReadOnlyList<ProjectBoardTask> Tasks);

public sealed record ProjectBoardTask(
    Guid Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    decimal SortOrder,
    string AssigneeName,
    DateTimeOffset? DueAtUtc);

public sealed record CreateProjectTaskRequest(
    string ProjectSlug,
    Guid BoardColumnId,
    string Title,
    string? Description,
    TaskPriority Priority,
    string UserId);

public sealed record CreatedProjectTask(
    Guid Id,
    string Title,
    Guid BoardColumnId);
