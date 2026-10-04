using RealTimeTaskManagement.Web.Models.Dashboard;

namespace RealTimeTaskManagement.Web.Models.Board;

public sealed record KanbanBoardViewModel(
    string ProjectName,
    string ProjectDescription,
    string CurrentUserName,
    string CurrentUserEmail,
    string CurrentUserInitials,
    IReadOnlyList<KanbanColumnViewModel> Columns)
    : DashboardLayoutViewModel(
        "Kanban",
        "จัดการทุกงานในที่เดียว",
        CurrentUserName,
        CurrentUserEmail,
        CurrentUserInitials,
        "Kanban",
        "เพิ่มงานใหม่",
        ShowSearch: true,
        ShowNotifications: true);

public sealed record KanbanColumnViewModel(
    string Name,
    string Tone,
    IReadOnlyList<KanbanTaskCardViewModel> Tasks);

public sealed record KanbanTaskCardViewModel(
    string Title,
    string Description,
    string Tag,
    string TagTone,
    string AssigneeInitials,
    string AssigneeTone,
    string DueDate,
    int? Progress = null,
    string? Priority = null,
    int CommentCount = 0,
    bool IsCompleted = false);
