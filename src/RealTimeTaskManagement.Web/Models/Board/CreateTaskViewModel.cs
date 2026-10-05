using System.ComponentModel.DataAnnotations;
using RealTimeTaskManagement.Domain.Enums;

namespace RealTimeTaskManagement.Web.Models.Board;

public sealed class CreateTaskViewModel
{
    [Required(ErrorMessage = "กรุณาเลือกคอลัมน์")]
    public Guid BoardColumnId { get; set; }

    [Required(ErrorMessage = "กรุณากรอกชื่องาน")]
    [StringLength(
        200,
        MinimumLength = 2,
        ErrorMessage = "ชื่องานต้องมี 2-200 ตัวอักษร")]
    [RegularExpression(@".*\S.*", ErrorMessage = "กรุณากรอกชื่องาน")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "รายละเอียดต้องไม่เกิน 2,000 ตัวอักษร")]
    public string? Description { get; set; }

    [EnumDataType(typeof(TaskPriority), ErrorMessage = "ระดับความสำคัญไม่ถูกต้อง")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}
