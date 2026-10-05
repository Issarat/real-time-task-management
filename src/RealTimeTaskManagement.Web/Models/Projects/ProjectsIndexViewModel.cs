using RealTimeTaskManagement.Web.Models.Dashboard;

namespace RealTimeTaskManagement.Web.Models.Projects;

public sealed record ProjectsIndexViewModel(
    string CurrentUserName,
    string CurrentUserEmail,
    string CurrentUserInitials,
    IReadOnlyList<ProjectCardViewModel> Projects,
    CreateProjectViewModel CreateProject,
    JoinProjectViewModel JoinProject,
    bool OpenCreateProjectModal = false,
    bool OpenJoinProjectModal = false)
    : DashboardLayoutViewModel(
        "Project",
        "จัดการทุกโปรเจกต์ของคุณ",
        CurrentUserName,
        CurrentUserEmail,
        CurrentUserInitials,
        "Project",
        "สร้างโปรเจกต์",
        ShowSearch: false,
        ShowNotifications: false,
        PrimaryActionDialogId: "create-project-dialog");

public sealed record ProjectCardViewModel(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string Role,
    int MemberCount,
    DateTimeOffset CreatedAtUtc);
