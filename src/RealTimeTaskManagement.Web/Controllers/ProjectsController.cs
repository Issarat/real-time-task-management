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

        return View(CreateBoardViewModel(board, user));
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

    private static KanbanBoardViewModel CreateBoardViewModel(
        ProjectBoard board,
        ApplicationUser currentUser,
        CreateTaskViewModel? createTask = null,
        bool openCreateTaskModal = false)
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
            openCreateTaskModal);
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
