using System.ComponentModel.DataAnnotations;

namespace RealTimeTaskManagement.Web.Models.Projects;

public sealed class CreateProjectViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อโปรเจกต์")]
    [StringLength(
        120,
        MinimumLength = 2,
        ErrorMessage = "ชื่อโปรเจกต์ต้องมี 2-120 ตัวอักษร")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "คำอธิบายต้องไม่เกิน 500 ตัวอักษร")]
    public string? Description { get; set; }
}
