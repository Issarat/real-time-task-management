using System.ComponentModel.DataAnnotations;

namespace RealTimeTaskManagement.Web.Models.Board;

public sealed class EditProjectViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อโปรเจกต์")]
    [StringLength(
        120,
        MinimumLength = 2,
        ErrorMessage = "ชื่อโปรเจกต์ต้องมี 2-120 ตัวอักษร")]
    [RegularExpression(@".*\S.*", ErrorMessage = "กรุณากรอกชื่อโปรเจกต์")]
    public string Name { get; set; } = string.Empty;
}
