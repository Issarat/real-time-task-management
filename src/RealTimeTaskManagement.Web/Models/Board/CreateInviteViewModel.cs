using System.ComponentModel.DataAnnotations;

namespace RealTimeTaskManagement.Web.Models.Board;

public sealed class CreateInviteViewModel
{
    [Range(1, 30, ErrorMessage = "อายุ Invite Code ต้องอยู่ระหว่าง 1-30 วัน")]
    public int ExpirationDays { get; set; } = 7;

    [Range(1, 100, ErrorMessage = "จำนวนครั้งที่ใช้ได้ต้องอยู่ระหว่าง 1-100 ครั้ง")]
    public int MaxUses { get; set; } = 10;
}
