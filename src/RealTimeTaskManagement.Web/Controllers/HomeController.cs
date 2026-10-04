using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimeTaskManagement.Infrastructure.Identity;
using RealTimeTaskManagement.Web.Models;
using RealTimeTaskManagement.Web.Models.Board;
using RealTimeTaskManagement.Web.Models.Projects;

namespace RealTimeTaskManagement.Web.Controllers;

public sealed class HomeController(UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        var user = await userManager.GetUserAsync(User);
        var displayName = string.IsNullOrWhiteSpace(user?.DisplayName)
            ? User.Identity.Name ?? "สมาชิก"
            : user.DisplayName;

        return View(CreateProjectsPage(
            displayName,
            user?.Email ?? User.Identity.Name ?? string.Empty));
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
        string currentUserEmail)
    {
        var initials = CreateInitials(currentUserName);
        return new ProjectsIndexViewModel(
            currentUserName,
            currentUserEmail,
            initials);
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
