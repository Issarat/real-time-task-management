using RealTimeTaskManagement.Web.Models.Dashboard;

namespace RealTimeTaskManagement.Web.Models.Board;

public sealed record TaskDetailsViewModel(
    Guid ProjectId,
    string ProjectSlug,
    string ProjectName,
    Guid TaskId,
    string Title,
    string Description,
    string PriorityLabel,
    string PriorityTone,
    string ColumnName,
    string ColumnTone,
    Guid BoardColumnId,
    IReadOnlyList<TaskStatusOptionViewModel> StatusOptions,
    string AssigneeName,
    string AssigneeEmail,
    string AssigneeInitials,
    string CreatedByName,
    DateTimeOffset? DueAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    EditTaskViewModel EditTask,
    bool OpenEditMode,
    string CurrentUserName,
    string CurrentUserEmail,
    string CurrentUserInitials)
    : DashboardLayoutViewModel(
        "รายละเอียดงาน",
        ProjectName,
        CurrentUserName,
        CurrentUserEmail,
        CurrentUserInitials,
        "Kanban",
        string.Empty,
        ShowSearch: false,
        ShowNotifications: false,
        PrimaryActionDialogId: null);

public sealed record TaskStatusOptionViewModel(
    Guid BoardColumnId,
    string Name,
    string Tone);
