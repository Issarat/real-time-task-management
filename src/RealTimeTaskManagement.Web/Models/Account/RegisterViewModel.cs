using System.ComponentModel.DataAnnotations;

namespace RealTimeTaskManagement.Web.Models.Account;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อที่แสดง")]
    [StringLength(100, ErrorMessage = "ชื่อต้องไม่เกิน 100 ตัวอักษร")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกอีเมล")]
    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
    [StringLength(
        100,
        ErrorMessage = "รหัสผ่านต้องมีอย่างน้อย {2} ตัวอักษร",
        MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "รหัสผ่านทั้งสองช่องไม่ตรงกัน")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool AcceptPrivacyPolicy { get; set; }

    public string ReturnUrl { get; set; } = "/";
}
