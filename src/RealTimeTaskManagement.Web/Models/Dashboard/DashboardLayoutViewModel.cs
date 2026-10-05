namespace RealTimeTaskManagement.Web.Models.Dashboard;

public abstract record DashboardLayoutViewModel(
    string PageTitle,
    string PageSubtitle,
    string CurrentUserName,
    string CurrentUserEmail,
    string CurrentUserInitials,
    string ActiveNavigation,
    string PrimaryActionLabel,
    bool ShowSearch,
    bool ShowNotifications,
    string? PrimaryActionDialogId);
