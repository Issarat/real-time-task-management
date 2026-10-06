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

    Task<CreatedProjectInvite?> CreateInviteAsync(
        CreateProjectInviteRequest request,
        CancellationToken cancellationToken = default);

    Task<ProjectBoard?> GetBoardAsync(
        string projectSlug,
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateProjectNameAsync(
        UpdateProjectNameRequest request,
        CancellationToken cancellationToken = default);

    Task<CreatedProjectTask?> CreateTaskAsync(
        CreateProjectTaskRequest request,
        CancellationToken cancellationToken = default);

    Task<ProjectTaskDetails?> GetTaskAsync(
        string projectSlug,
        Guid taskId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateTaskStatusAsync(
        UpdateProjectTaskStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateTaskDueDateAsync(
        UpdateProjectTaskDueDateRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateTaskDetailsAsync(
        UpdateProjectTaskDetailsRequest request,
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

public sealed record CreateProjectInviteRequest(
    string ProjectSlug,
    int ExpirationDays,
    int MaxUses,
    string CreatedByUserId);

public sealed record CreatedProjectInvite(
    string Code,
    DateTimeOffset ExpiresAtUtc,
    int MaxUses);

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

public sealed record UpdateProjectNameRequest(
    string ProjectSlug,
    string Name,
    string UserId);

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

public sealed record ProjectTaskDetails(
    Guid ProjectId,
    string ProjectName,
    string ProjectSlug,
    Guid TaskId,
    string Title,
    string? Description,
    TaskPriority Priority,
    string ColumnName,
    string ColumnKey,
    Guid BoardColumnId,
    string AssigneeName,
    string AssigneeEmail,
    string CreatedByName,
    DateTimeOffset? DueAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    IReadOnlyList<ProjectTaskStatusOption> StatusOptions);

public sealed record ProjectTaskStatusOption(
    Guid BoardColumnId,
    string Name,
    string Key,
    int SortOrder);

public sealed record UpdateProjectTaskStatusRequest(
    string ProjectSlug,
    Guid TaskId,
    Guid BoardColumnId,
    string UserId);

public sealed record UpdateProjectTaskDueDateRequest(
    string ProjectSlug,
    Guid TaskId,
    DateTimeOffset? DueAtUtc,
    string UserId);

public sealed record UpdateProjectTaskDetailsRequest(
    string ProjectSlug,
    Guid TaskId,
    string Title,
    string? Description,
    TaskPriority Priority,
    string UserId);
