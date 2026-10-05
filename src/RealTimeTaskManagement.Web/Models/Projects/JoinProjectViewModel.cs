using System.ComponentModel.DataAnnotations;

namespace RealTimeTaskManagement.Web.Models.Projects;

public sealed class JoinProjectViewModel
{
    [Required(ErrorMessage = "กรุณากรอก Invite Code")]
    [StringLength(
        32,
        MinimumLength = 6,
        ErrorMessage = "Invite Code ต้องมี 6-32 ตัวอักษร")]
    [RegularExpression(
        "^[A-Za-z0-9-]+$",
        ErrorMessage = "Invite Code ใช้ได้เฉพาะตัวอักษร ตัวเลข และเครื่องหมาย -")]
    public string InviteCode { get; set; } = string.Empty;
}
