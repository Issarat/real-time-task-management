using System.Globalization;
using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RealTimeTaskManagement.Application.Projects;
using RealTimeTaskManagement.Domain.Entities;
using RealTimeTaskManagement.Domain.Enums;
using RealTimeTaskManagement.Infrastructure.Persistence;

namespace RealTimeTaskManagement.Infrastructure.Projects;

internal sealed class ProjectService(ApplicationDbContext dbContext)
    : IProjectService
{
    public async Task<IReadOnlyList<ProjectSummary>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ProjectMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .OrderByDescending(member => member.Project.CreatedAtUtc)
            .Select(member => new ProjectSummary(
                member.ProjectId,
                member.Project.Name,
                member.Project.Slug,
                member.Project.Description,
                member.Role,
                member.Project.Members.Count,
                member.Project.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<CreatedProject> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var name = request.Name.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        var project = new Project
        {
            Id = projectId,
            Name = name,
            Description = description,
            Slug = CreateSlug(name, projectId),
            CreatedByUserId = request.CreatedByUserId,
            CreatedAtUtc = now
        };

        project.Members.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = request.CreatedByUserId,
            Role = ProjectRole.Owner,
            JoinedAtUtc = now
        });

        project.Columns.Add(new BoardColumn
        {
            ProjectId = projectId,
            Name = "Backlog",
            Key = "backlog",
            SortOrder = 1000
        });
        project.Columns.Add(new BoardColumn
        {
            ProjectId = projectId,
            Name = "To do",
            Key = "todo",
            SortOrder = 2000
        });
        project.Columns.Add(new BoardColumn
        {
            ProjectId = projectId,
            Name = "In progress",
            Key = "in-progress",
            SortOrder = 3000
        });
        project.Columns.Add(new BoardColumn
        {
            ProjectId = projectId,
            Name = "Done",
            Key = "done",
            SortOrder = 4000
        });

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreatedProject(project.Id, project.Name, project.Slug);
    }

    public async Task<JoinProjectResult> JoinAsync(
        JoinProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var codeHash = ProjectInviteCode.Hash(request.InviteCode);
        var invite = await dbContext.ProjectInvites
            .Include(projectInvite => projectInvite.Project)
            .SingleOrDefaultAsync(
                projectInvite => projectInvite.CodeHash == codeHash,
                cancellationToken);

        if (invite is null)
        {
            return new JoinProjectResult(JoinProjectStatus.InvalidCode);
        }

        if (invite.RevokedAtUtc is not null)
        {
            return new JoinProjectResult(
                JoinProjectStatus.Revoked,
                invite.ProjectId,
                invite.Project.Name);
        }

        if (invite.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return new JoinProjectResult(
                JoinProjectStatus.Expired,
                invite.ProjectId,
                invite.Project.Name);
        }

        var isAlreadyMember = await dbContext.ProjectMembers.AnyAsync(
            member => member.ProjectId == invite.ProjectId
                && member.UserId == request.UserId,
            cancellationToken);

        if (isAlreadyMember)
        {
            return new JoinProjectResult(
                JoinProjectStatus.AlreadyMember,
                invite.ProjectId,
                invite.Project.Name);
        }

        if (invite.MaxUses.HasValue && invite.UsedCount >= invite.MaxUses.Value)
        {
            return new JoinProjectResult(
                JoinProjectStatus.UsageLimitReached,
                invite.ProjectId,
                invite.Project.Name);
        }

        dbContext.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = invite.ProjectId,
            UserId = request.UserId,
            Role = invite.Role,
            JoinedAtUtc = DateTimeOffset.UtcNow
        });
        invite.UsedCount++;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new JoinProjectResult(
            JoinProjectStatus.Joined,
            invite.ProjectId,
            invite.Project.Name);
    }

    public async Task<ProjectBoard?> GetBoardAsync(
        string projectSlug,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Members)
            .Include(item => item.Columns)
                .ThenInclude(column => column.Tasks)
            .SingleOrDefaultAsync(
                item => item.Slug == projectSlug
                    && item.Members.Any(member => member.UserId == userId),
                cancellationToken);

        if (project is null)
        {
            return null;
        }

        var projectTasks = project.Columns.SelectMany(column => column.Tasks).ToArray();
        var relatedUserIds = project.Members
            .Select(member => member.UserId)
            .Concat(projectTasks.Select(task => task.CreatedByUserId))
            .Concat(projectTasks
                .Where(task => task.AssignedToUserId != null)
                .Select(task => task.AssignedToUserId!))
            .Distinct()
            .ToArray();

        var users = await dbContext.Users
            .AsNoTracking()
            .Where(user => relatedUserIds.Contains(user.Id))
            .Select(user => new
            {
                user.Id,
                user.DisplayName,
                user.Email
            })
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        string GetDisplayName(string relatedUserId)
        {
            if (!users.TryGetValue(relatedUserId, out var relatedUser))
            {
                return "สมาชิก";
            }

            return string.IsNullOrWhiteSpace(relatedUser.DisplayName)
                ? relatedUser.Email ?? "สมาชิก"
                : relatedUser.DisplayName;
        }

        return new ProjectBoard(
            project.Id,
            project.Name,
            project.Slug,
            project.Description,
            project.Members
                .OrderBy(member => member.Role)
                .ThenBy(member => member.JoinedAtUtc)
                .Select(member => new ProjectBoardMember(
                    member.UserId,
                    GetDisplayName(member.UserId),
                    users.GetValueOrDefault(member.UserId)?.Email ?? string.Empty,
                    member.Role))
                .ToArray(),
            project.Columns
                .OrderBy(column => column.SortOrder)
                .Select(column => new ProjectBoardColumn(
                    column.Id,
                    column.Name,
                    column.Key,
                    column.SortOrder,
                    column.Tasks
                        .OrderBy(task => task.SortOrder)
                        .Select(task => new ProjectBoardTask(
                            task.Id,
                            task.Title,
                            task.Description,
                            task.Priority,
                            task.SortOrder,
                            GetDisplayName(task.AssignedToUserId ?? task.CreatedByUserId),
                            task.DueAtUtc))
                        .ToArray()))
                .ToArray());
    }

    public async Task<CreatedProjectTask?> CreateTaskAsync(
        CreateProjectTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var column = await dbContext.BoardColumns
            .SingleOrDefaultAsync(
                item => item.Id == request.BoardColumnId
                    && item.Project.Slug == request.ProjectSlug
                    && item.Project.Members.Any(member => member.UserId == request.UserId),
                cancellationToken);

        if (column is null)
        {
            return null;
        }

        var lastSortOrder = await dbContext.TaskItems
            .Where(task => task.BoardColumnId == column.Id)
            .MaxAsync(task => (decimal?)task.SortOrder, cancellationToken)
            ?? 0m;

        var task = new TaskItem
        {
            ProjectId = column.ProjectId,
            BoardColumnId = column.Id,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Priority = request.Priority,
            SortOrder = lastSortOrder + 1000m,
            CreatedByUserId = request.UserId,
            AssignedToUserId = request.UserId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.TaskItems.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreatedProjectTask(task.Id, task.Title, task.BoardColumnId);
    }

    private static string CreateSlug(string name, Guid projectId)
    {
        var normalized = name.Normalize(NormalizationForm.FormD);
        var slugBuilder = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character)
                == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                slugBuilder.Append(char.ToLowerInvariant(character));
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator && slugBuilder.Length > 0)
            {
                slugBuilder.Append('-');
                previousWasSeparator = true;
            }
        }

        var slugBase = slugBuilder.ToString().Trim('-');
        if (string.IsNullOrWhiteSpace(slugBase))
        {
            slugBase = "project";
        }

        const int suffixLength = 9;
        var maxBaseLength = 140 - suffixLength;
        if (slugBase.Length > maxBaseLength)
        {
            slugBase = slugBase[..maxBaseLength].TrimEnd('-');
        }

        return $"{slugBase}-{projectId:N}"[..(slugBase.Length + suffixLength)];
    }
}
