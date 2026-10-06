using System.ComponentModel.DataAnnotations;
using RealTimeTaskManagement.Web.Models.Dashboard;

namespace RealTimeTaskManagement.Web.Models.Account;

public sealed record ProfileViewModel(
    string DisplayName,
    string Email,
    string Initials,
    DateTimeOffset CreatedAtUtc,
    EditProfileViewModel EditProfile,
    PasswordChangeViewModel PasswordChange,
    bool OpenEditMode = false,
    bool OpenPasswordForm = false)
    : DashboardLayoutViewModel(
        "โปรไฟล์",
        "จัดการข้อมูลบัญชีของคุณ",
        DisplayName,
        Email,
        Initials,
        "Profile",
        string.Empty,
        ShowSearch: false,
        ShowNotifications: false,
        PrimaryActionDialogId: null);

public sealed class EditProfileViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อที่แสดง")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "ชื่อที่แสดงต้องมี 2-100 ตัวอักษร")]
    [RegularExpression(@".*\S.*", ErrorMessage = "กรุณากรอกชื่อที่แสดง")]
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class PasswordChangeViewModel
{
    [Required(ErrorMessage = "กรุณากรอกรหัสผ่านปัจจุบัน")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกรหัสผ่านใหม่")]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "รหัสผ่านใหม่ต้องมีอย่างน้อย 8 ตัวอักษร")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณายืนยันรหัสผ่านใหม่")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "รหัสผ่านใหม่และการยืนยันรหัสผ่านไม่ตรงกัน")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
