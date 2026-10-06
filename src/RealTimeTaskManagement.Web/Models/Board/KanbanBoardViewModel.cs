using RealTimeTaskManagement.Web.Models.Dashboard;

namespace RealTimeTaskManagement.Web.Models.Board;

public sealed record KanbanBoardViewModel(
    string ProjectName,
    string ProjectDescription,
    string CurrentUserName,
    string CurrentUserEmail,
    string CurrentUserInitials,
    IReadOnlyList<KanbanColumnViewModel> Columns,
    Guid ProjectId = default,
    string ProjectSlug = "",
    IReadOnlyList<KanbanMemberViewModel>? Members = null,
    CreateTaskViewModel? CreateTask = null,
    bool OpenCreateTaskModal = false,
    EditProjectViewModel? EditProject = null,
    bool CanEditProject = false,
    bool OpenEditProjectModal = false,
    CreateInviteViewModel? CreateInvite = null,
    string? GeneratedInviteCode = null,
    string? GeneratedInviteExpiresAt = null,
    int? GeneratedInviteMaxUses = null,
    bool OpenInviteModal = false)
    : DashboardLayoutViewModel(
        "Kanban",
        "จัดการทุกงานในที่เดียว",
        CurrentUserName,
        CurrentUserEmail,
        CurrentUserInitials,
        "Kanban",
        "เพิ่มงานใหม่",
        ShowSearch: true,
        ShowNotifications: true,
        PrimaryActionDialogId: "create-task-dialog");

public sealed record KanbanColumnViewModel(
    string Name,
    string Tone,
    IReadOnlyList<KanbanTaskCardViewModel> Tasks,
    Guid Id = default,
    string Key = "");

public sealed record KanbanMemberViewModel(
    string DisplayName,
    string Initials,
    string Tone,
    string Role);

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
    bool IsCompleted = false,
    Guid Id = default,
    string AssigneeName = "");
