using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimeTaskManagement.Application.Projects;
using RealTimeTaskManagement.Infrastructure.Identity;
using RealTimeTaskManagement.Web.Models;
using RealTimeTaskManagement.Web.Models.Board;
using RealTimeTaskManagement.Web.Models.Projects;

namespace RealTimeTaskManagement.Web.Controllers;

public sealed class HomeController(
    UserManager<ApplicationUser> userManager,
    IProjectService projectService) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var displayName = string.IsNullOrWhiteSpace(user.DisplayName)
            ? user.Email ?? "สมาชิก"
            : user.DisplayName;
        var projects = await projectService.GetForUserAsync(user.Id);

        return View(CreateProjectsPage(
            displayName,
            user.Email ?? string.Empty,
            projects));
    }

    [Authorize]
    [HttpPost("/projects/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProject(
        [Bind(Prefix = nameof(ProjectsIndexViewModel.CreateProject))]
        CreateProjectViewModel model,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            var currentProjects = await projectService.GetForUserAsync(
                user.Id,
                cancellationToken);
            var displayName = string.IsNullOrWhiteSpace(user.DisplayName)
                ? user.Email ?? "สมาชิก"
                : user.DisplayName;

            return View("Index", CreateProjectsPage(
                displayName,
                user.Email ?? string.Empty,
                currentProjects,
                model,
                openCreateProjectModal: true));
        }

        var createdProject = await projectService.CreateAsync(
            new CreateProjectRequest(
                model.Name,
                model.Description,
                user.Id),
            cancellationToken);

        TempData["ProjectCreatedMessage"] =
            $"สร้างโปรเจกต์ “{createdProject.Name}” เรียบร้อยแล้ว";

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost("/projects/join")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinProject(
        [Bind(Prefix = nameof(ProjectsIndexViewModel.JoinProject))]
        JoinProjectViewModel model,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        JoinProjectResult? joinResult = null;
        if (ModelState.IsValid)
        {
            joinResult = await projectService.JoinAsync(
                new JoinProjectRequest(model.InviteCode, user.Id),
                cancellationToken);

            if (joinResult.Status == JoinProjectStatus.Joined)
            {
                TempData["ProjectJoinedMessage"] =
                    $"เข้าร่วมโปรเจกต์ “{joinResult.ProjectName}” เรียบร้อยแล้ว";

                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(
                $"{nameof(ProjectsIndexViewModel.JoinProject)}.{nameof(JoinProjectViewModel.InviteCode)}",
                CreateJoinProjectError(joinResult));
        }

        var currentProjects = await projectService.GetForUserAsync(
            user.Id,
            cancellationToken);
        var displayName = string.IsNullOrWhiteSpace(user.DisplayName)
            ? user.Email ?? "สมาชิก"
            : user.DisplayName;

        return View("Index", CreateProjectsPage(
            displayName,
            user.Email ?? string.Empty,
            currentProjects,
            joinProject: model,
            openJoinProjectModal: true));
    }

    [HttpGet("/privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    private static ProjectsIndexViewModel CreateProjectsPage(
        string currentUserName,
        string currentUserEmail,
        IReadOnlyList<ProjectSummary> projects,
        CreateProjectViewModel? createProject = null,
        JoinProjectViewModel? joinProject = null,
        bool openCreateProjectModal = false,
        bool openJoinProjectModal = false)
    {
        var initials = CreateInitials(currentUserName);
        return new ProjectsIndexViewModel(
            currentUserName,
            currentUserEmail,
            initials,
            projects
                .Select(project => new ProjectCardViewModel(
                    project.Id,
                    project.Name,
                    project.Slug,
                    project.Description ?? "ยังไม่มีคำอธิบาย",
                    project.Role.ToString(),
                    project.MemberCount,
                    project.CreatedAtUtc))
                .ToArray(),
            createProject ?? new CreateProjectViewModel(),
            joinProject ?? new JoinProjectViewModel(),
            openCreateProjectModal,
            openJoinProjectModal);
    }

    private static string CreateJoinProjectError(JoinProjectResult result)
    {
        return result.Status switch
        {
            JoinProjectStatus.AlreadyMember =>
                $"คุณเป็นสมาชิกของโปรเจกต์ “{result.ProjectName}” อยู่แล้ว",
            JoinProjectStatus.Expired => "Invite Code นี้หมดอายุแล้ว",
            JoinProjectStatus.Revoked => "Invite Code นี้ถูกยกเลิกแล้ว",
            JoinProjectStatus.UsageLimitReached =>
                "Invite Code นี้มีผู้ใช้งานครบจำนวนแล้ว",
            _ => "ไม่พบ Invite Code กรุณาตรวจสอบแล้วลองอีกครั้ง"
        };
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

    private static KanbanBoardViewModel CreateDemoBoard(
        string currentUserName,
        string currentUserEmail)
    {
        var initials = string.Concat(
            currentUserName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part => part[0]))
            .ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(initials))
        {
            initials = "U";
        }

        return new KanbanBoardViewModel(
            "Product Launch Q4",
            "วางแผนและติดตามงานสำหรับการเปิดตัวผลิตภัณฑ์ครั้งใหม่",
            currentUserName,
            currentUserEmail,
            initials,
            [
                new KanbanColumnViewModel(
                    "Backlog",
                    "slate",
                    [
                        new KanbanTaskCardViewModel(
                            "วิเคราะห์คู่แข่งในตลาด",
                            "สรุปจุดแข็งและโอกาสของผลิตภัณฑ์",
                            "Research",
                            "coral",
                            "PK",
                            "coral",
                            "18 ต.ค.",
                            CommentCount: 2),
                        new KanbanTaskCardViewModel(
                            "รวบรวม User feedback",
                            "",
                            "UX",
                            "amber",
                            "JT",
                            "amber",
                            "20 ต.ค.",
                            CommentCount: 1),
                        new KanbanTaskCardViewModel(
                            "กำหนดกลุ่มเป้าหมาย",
                            "",
                            "Marketing",
                            "purple",
                            "AM",
                            "purple",
                            "22 ต.ค.")
                    ]),
                new KanbanColumnViewModel(
                    "To do",
                    "purple",
                    [
                        new KanbanTaskCardViewModel(
                            "ออกแบบ Landing page",
                            "เตรียมหน้าเว็บสำหรับแคมเปญใหม่",
                            "Design",
                            "blue",
                            "NS",
                            "blue",
                            "3 วัน",
                            Progress: 60,
                            CommentCount: 3),
                        new KanbanTaskCardViewModel(
                            "เขียน Product announcement",
                            "",
                            "Content",
                            "pink",
                            "ML",
                            "pink",
                            "24 ต.ค."),
                        new KanbanTaskCardViewModel(
                            "ตรวจสอบ Pricing plan",
                            "",
                            "Review",
                            "amber",
                            "TP",
                            "green",
                            "25 ต.ค.")
                    ]),
                new KanbanColumnViewModel(
                    "In progress",
                    "blue",
                    [
                        new KanbanTaskCardViewModel(
                            "พัฒนา Checkout flow",
                            "เชื่อมต่อระบบชำระเงินและ coupon",
                            "Dev",
                            "blue",
                            "NS",
                            "blue",
                            "3 วัน",
                            Progress: 78,
                            Priority: "High"),
                        new KanbanTaskCardViewModel(
                            "ติดตั้ง Analytics events",
                            "ติดตาม conversion funnel",
                            "Data",
                            "green",
                            "TP",
                            "green",
                            "28 ต.ค.")
                    ]),
                new KanbanColumnViewModel(
                    "Done",
                    "green",
                    [
                        new KanbanTaskCardViewModel(
                            "สรุป Design system",
                            "",
                            "Design",
                            "blue",
                            "AM",
                            "purple",
                            "",
                            IsCompleted: true),
                        new KanbanTaskCardViewModel(
                            "เตรียม Staging environment",
                            "",
                            "Dev",
                            "blue",
                            "NS",
                            "blue",
                            "",
                            IsCompleted: true),
                        new KanbanTaskCardViewModel(
                            "วางแผน Launch campaign",
                            "",
                            "Marketing",
                            "purple",
                            "AM",
                            "purple",
                            "",
                            IsCompleted: true)
                    ])
            ]);
    }
}
