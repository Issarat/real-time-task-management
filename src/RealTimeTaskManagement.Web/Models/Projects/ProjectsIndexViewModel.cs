using RealTimeTaskManagement.Web.Models.Dashboard;

namespace RealTimeTaskManagement.Web.Models.Projects;

public sealed record ProjectsIndexViewModel(
    string CurrentUserName,
    string CurrentUserEmail,
    string CurrentUserInitials)
    : DashboardLayoutViewModel(
        "Project",
        "จัดการทุกโปรเจกต์ของคุณ",
        CurrentUserName,
        CurrentUserEmail,
        CurrentUserInitials,
        "Kanban",
        "สร้างโปรเจกต์",
        ShowSearch: false,
        ShowNotifications: false);
