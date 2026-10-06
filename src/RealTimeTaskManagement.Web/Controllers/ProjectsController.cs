using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimeTaskManagement.Application.Projects;
using RealTimeTaskManagement.Domain.Enums;
using RealTimeTaskManagement.Infrastructure.Identity;
using RealTimeTaskManagement.Web.Models.Board;

namespace RealTimeTaskManagement.Web.Controllers;

[Authorize]
public sealed class ProjectsController(
    UserManager<ApplicationUser> userManager,
    IProjectService projectService) : Controller
{
    private static readonly string[] MemberTones =
        ["coral", "blue", "purple", "amber", "green", "pink"];
    private static readonly TimeZoneInfo ThailandTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok");

    [HttpGet("/projects/{slug}/board")]
    public async Task<IActionResult> Board(
        string slug,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var board = await projectService.GetBoardAsync(
            slug,
            user.Id,
            cancellationToken);

        if (board is null)
        {
            return NotFound();
        }

        var generatedInviteCode = TempData["GeneratedInviteCode"] as string;
        var generatedInviteExpiresAt = TempData["GeneratedInviteExpiresAt"] as string;
        var generatedInviteMaxUses = int.TryParse(
            TempData["GeneratedInviteMaxUses"] as string,
            out var parsedMaxUses)
            ? parsedMaxUses
            : (int?)null;

        return View(CreateBoardViewModel(
            board,
            user,
            generatedInviteCode: generatedInviteCode,
            generatedInviteExpiresAt: generatedInviteExpiresAt,
            generatedInviteMaxUses: generatedInviteMaxUses,
            openInviteModal: generatedInviteCode is not null));
    }

    [HttpGet("/projects/{slug}/tasks/{taskId:guid}")]
    public async Task<IActionResult> TaskDetails(
        string slug,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var task = await projectService.GetTaskAsync(
            slug,
            taskId,
            user.Id,
            cancellationToken);

        if (task is null)
        {
            return NotFound();
        }

        return View(CreateTaskDetailsViewModel(task, user));
    }

    [HttpPost("/projects/{slug}/tasks/{taskId:guid}/details")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTaskDetails(
        string slug,
        Guid taskId,
        [Bind(Prefix = nameof(TaskDetailsViewModel.EditTask))]
        EditTaskViewModel model,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (ModelState.IsValid)
        {
            var isUpdated = await projectService.UpdateTaskDetailsAsync(
                new UpdateProjectTaskDetailsRequest(
                    slug,
                    taskId,
                    model.Title,
                    model.Description,
                    model.Priority,
                    user.Id),
                cancellationToken);

            if (!isUpdated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(TaskDetails), new { slug, taskId });
        }

        var task = await projectService.GetTaskAsync(
            slug,
            taskId,
            user.Id,
            cancellationToken);

        if (task is null)
        {
            return NotFound();
        }

        return View(
            nameof(TaskDetails),
            CreateTaskDetailsViewModel(
                task,
                user,
                model,
                openEditMode: true));
    }

    [HttpPost("/projects/{slug}/tasks/{taskId:guid}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTaskStatus(
        string slug,
        Guid taskId,
        Guid boardColumnId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (boardColumnId == Guid.Empty)
        {
            return BadRequest();
        }

        var isUpdated = await projectService.UpdateTaskStatusAsync(
            new UpdateProjectTaskStatusRequest(
                slug,
                taskId,
                boardColumnId,
                user.Id),
            cancellationToken);

        if (!isUpdated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(TaskDetails), new { slug, taskId });
    }

    [HttpPost("/projects/{slug}/tasks/{taskId:guid}/due-date")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTaskDueDate(
        string slug,
        Guid taskId,
        DateOnly? dueDate,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        DateTimeOffset? dueAtUtc = null;
        if (dueDate.HasValue)
        {
            var localDueDate = DateTime.SpecifyKind(
                dueDate.Value.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Unspecified);
            var utcDueDate = TimeZoneInfo.ConvertTimeToUtc(
                localDueDate,
                ThailandTimeZone);
            dueAtUtc = new DateTimeOffset(utcDueDate);
        }

        var isUpdated = await projectService.UpdateTaskDueDateAsync(
            new UpdateProjectTaskDueDateRequest(
                slug,
                taskId,
                dueAtUtc,
                user.Id),
            cancellationToken);

        if (!isUpdated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(TaskDetails), new { slug, taskId });
    }

    [HttpPost("/projects/{slug}/name")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProjectName(
        string slug,
        [Bind(Prefix = nameof(KanbanBoardViewModel.EditProject))]
        EditProjectViewModel model,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (ModelState.IsValid)
        {
            var isUpdated = await projectService.UpdateProjectNameAsync(
                new UpdateProjectNameRequest(
                    slug,
                    model.Name,
                    user.Id),
                cancellationToken);

            if (!isUpdated)
            {
                return NotFound();
            }

            TempData["BoardMessage"] = $"แก้ไขชื่อโปรเจกต์เป็น “{model.Name.Trim()}” เรียบร้อยแล้ว";
            return RedirectToAction(nameof(Board), new { slug });
        }

        var board = await projectService.GetBoardAsync(
            slug,
            user.Id,
            cancellationToken);

        if (board is null)
        {
            return NotFound();
        }

        return View(
            nameof(Board),
            CreateBoardViewModel(
                board,
                user,
                editProject: model,
                openEditProjectModal: true));
    }

    [HttpPost("/projects/{slug}/tasks")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTask(
        string slug,
        [Bind(Prefix = nameof(KanbanBoardViewModel.CreateTask))]
        CreateTaskViewModel model,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (ModelState.IsValid)
        {
            var createdTask = await projectService.CreateTaskAsync(
                new CreateProjectTaskRequest(
                    slug,
                    model.BoardColumnId,
                    model.Title,
                    model.Description,
                    model.Priority,
                    user.Id),
                cancellationToken);

            if (createdTask is null)
            {
                return NotFound();
            }

            TempData["BoardMessage"] = $"เพิ่มงาน “{createdTask.Title}” เรียบร้อยแล้ว";
            return RedirectToAction(nameof(Board), new { slug });
        }

        var board = await projectService.GetBoardAsync(
            slug,
            user.Id,
            cancellationToken);

        if (board is null)
        {
            return NotFound();
        }

        return View(
            nameof(Board),
            CreateBoardViewModel(
                board,
                user,
                model,
                openCreateTaskModal: true));
    }

    [HttpPost("/projects/{slug}/invites")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInvite(
        string slug,
        [Bind(Prefix = nameof(KanbanBoardViewModel.CreateInvite))]
        CreateInviteViewModel model,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (ModelState.IsValid)
        {
            var invite = await projectService.CreateInviteAsync(
                new CreateProjectInviteRequest(
                    slug,
                    model.ExpirationDays,
                    model.MaxUses,
                    user.Id),
                cancellationToken);

            if (invite is null)
            {
                return NotFound();
            }

            TempData["GeneratedInviteCode"] = invite.Code;
            TempData["GeneratedInviteExpiresAt"] = invite.ExpiresAtUtc
                .ToOffset(TimeSpan.FromHours(7))
                .ToString("dd MMM yyyy, HH:mm");
            TempData["GeneratedInviteMaxUses"] = invite.MaxUses.ToString();

            return RedirectToAction(nameof(Board), new { slug });
        }

        var board = await projectService.GetBoardAsync(
            slug,
            user.Id,
            cancellationToken);

        if (board is null)
        {
            return NotFound();
        }

        return View(
            nameof(Board),
            CreateBoardViewModel(
                board,
                user,
                createInvite: model,
                openInviteModal: true));
    }

    private static KanbanBoardViewModel CreateBoardViewModel(
        ProjectBoard board,
        ApplicationUser currentUser,
        CreateTaskViewModel? createTask = null,
        bool openCreateTaskModal = false,
        EditProjectViewModel? editProject = null,
        bool openEditProjectModal = false,
        CreateInviteViewModel? createInvite = null,
        string? generatedInviteCode = null,
        string? generatedInviteExpiresAt = null,
        int? generatedInviteMaxUses = null,
        bool openInviteModal = false)
    {
        var currentUserName = string.IsNullOrWhiteSpace(currentUser.DisplayName)
            ? currentUser.Email ?? "สมาชิก"
            : currentUser.DisplayName;

        var columns = board.Columns
            .Select(column => new KanbanColumnViewModel(
                column.Name,
                GetColumnTone(column.Key),
                column.Tasks
                    .Select(task => new KanbanTaskCardViewModel(
                        task.Title,
                        task.Description ?? string.Empty,
                        GetPriorityLabel(task.Priority),
                        GetPriorityTone(task.Priority),
                        CreateInitials(task.AssigneeName),
                        GetMemberTone(board.Members, task.AssigneeName),
                        task.DueAtUtc?.ToLocalTime().ToString("d MMM") ?? string.Empty,
                        Priority: task.Priority == TaskPriority.High ? "High" : null,
                        IsCompleted: column.Key == "done",
                        Id: task.Id,
                        AssigneeName: task.AssigneeName))
                    .ToArray(),
                column.Id,
                column.Key))
            .ToArray();

        createTask ??= new CreateTaskViewModel
        {
            BoardColumnId = columns.FirstOrDefault()?.Id ?? Guid.Empty
        };

        editProject ??= new EditProjectViewModel
        {
            Name = board.Name
        };

        createInvite ??= new CreateInviteViewModel();

        var canEditProject = board.Members.Any(member =>
            member.UserId == currentUser.Id
            && member.Role == ProjectRole.Owner);

        return new KanbanBoardViewModel(
            board.Name,
            board.Description ?? "จัดการและติดตามงานของทีมในที่เดียว",
            currentUserName,
            currentUser.Email ?? string.Empty,
            CreateInitials(currentUserName),
            columns,
            board.Id,
            board.Slug,
            board.Members
                .Select((member, index) => new KanbanMemberViewModel(
                    member.DisplayName,
                    CreateInitials(member.DisplayName),
                    MemberTones[index % MemberTones.Length],
                    member.Role.ToString()))
                .ToArray(),
            createTask,
            openCreateTaskModal,
            editProject,
            canEditProject,
            openEditProjectModal,
            createInvite,
            generatedInviteCode,
            generatedInviteExpiresAt,
            generatedInviteMaxUses,
            openInviteModal);
    }

    private static TaskDetailsViewModel CreateTaskDetailsViewModel(
        ProjectTaskDetails task,
        ApplicationUser currentUser,
        EditTaskViewModel? editTask = null,
        bool openEditMode = false)
    {
        var currentUserName = string.IsNullOrWhiteSpace(currentUser.DisplayName)
            ? currentUser.Email ?? "สมาชิก"
            : currentUser.DisplayName;

        editTask ??= new EditTaskViewModel
        {
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority
        };

        return new TaskDetailsViewModel(
            task.ProjectId,
            task.ProjectSlug,
            task.ProjectName,
            task.TaskId,
            task.Title,
            task.Description ?? "ยังไม่มีรายละเอียดสำหรับงานนี้",
            GetPriorityLabel(task.Priority),
            GetPriorityTone(task.Priority),
            task.ColumnName,
            GetColumnTone(task.ColumnKey),
            task.BoardColumnId,
            task.StatusOptions
                .Select(status => new TaskStatusOptionViewModel(
                    status.BoardColumnId,
                    status.Name,
                    GetColumnTone(status.Key)))
                .ToArray(),
            task.AssigneeName,
            task.AssigneeEmail,
            CreateInitials(task.AssigneeName),
            task.CreatedByName,
            task.DueAtUtc,
            task.CreatedAtUtc,
            task.UpdatedAtUtc,
            editTask,
            openEditMode,
            currentUserName,
            currentUser.Email ?? string.Empty,
            CreateInitials(currentUserName));
    }

    private static string GetColumnTone(string key)
    {
        return key switch
        {
            "todo" => "purple",
            "in-progress" => "blue",
            "done" => "green",
            _ => "slate"
        };
    }

    private static string GetPriorityLabel(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Low => "ต่ำ",
            TaskPriority.High => "สูง",
            _ => "ปานกลาง"
        };
    }

    private static string GetPriorityTone(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Low => "green",
            TaskPriority.High => "coral",
            _ => "amber"
        };
    }

    private static string GetMemberTone(
        IReadOnlyList<ProjectBoardMember> members,
        string displayName)
    {
        var memberIndex = members
            .Select((member, index) => new { member.DisplayName, Index = index })
            .FirstOrDefault(member => member.DisplayName == displayName)
            ?.Index ?? 0;

        return MemberTones[memberIndex % MemberTones.Length];
    }

    private static string CreateInitials(string displayName)
    {
        var initials = string.Concat(
            displayName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part => part[0]))
            .ToUpperInvariant();

        return string.IsNullOrWhiteSpace(initials) ? "U" : initials;
    }
}
