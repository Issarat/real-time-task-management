using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimeTaskManagement.Infrastructure.Identity;
using RealTimeTaskManagement.Web.Models.Account;

namespace RealTimeTaskManagement.Web.Controllers;

public sealed class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
{
    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = GetSafeReturnUrl(returnUrl)
        });
    }

    [AllowAnonymous]
    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.ReturnUrl = GetSafeReturnUrl(model.ReturnUrl);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return LocalRedirect(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "บัญชีถูกล็อกชั่วคราว กรุณาลองใหม่ภายหลัง");
            return View(model);
        }

        if (result.RequiresTwoFactor)
        {
            ModelState.AddModelError(
                string.Empty,
                "บัญชีนี้เปิดใช้การยืนยันสองขั้นตอน ซึ่งระบบยังไม่รองรับ");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "อีเมลหรือรหัสผ่านไม่ถูกต้อง");
        return View(model);
    }

    [AllowAnonymous]
    [HttpGet("/register")]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        return View(new RegisterViewModel
        {
            ReturnUrl = GetSafeReturnUrl(returnUrl)
        });
    }

    [AllowAnonymous]
    [HttpPost("/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.ReturnUrl = GetSafeReturnUrl(model.ReturnUrl);

        if (!model.AcceptPrivacyPolicy)
        {
            ModelState.AddModelError(
                nameof(model.AcceptPrivacyPolicy),
                "กรุณายอมรับนโยบายความเป็นส่วนตัว");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var user = new ApplicationUser
        {
            DisplayName = model.DisplayName.Trim(),
            UserName = email,
            Email = email
        };

        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(model.ReturnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, LocalizeIdentityError(error));
        }

        return View(model);
    }

    [Authorize]
    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await signInManager.SignOutAsync();
        return LocalRedirect(GetSafeReturnUrl(returnUrl));
    }

    [AllowAnonymous]
    [HttpGet("/access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private string GetSafeReturnUrl(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Action(nameof(HomeController.Index), "Home")!;
    }

    private static string LocalizeIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            "DuplicateEmail" or "DuplicateUserName" => "อีเมลนี้ถูกใช้งานแล้ว",
            "InvalidEmail" or "InvalidUserName" => "รูปแบบอีเมลไม่ถูกต้อง",
            "PasswordTooShort" => "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร",
            "PasswordRequiresDigit" => "รหัสผ่านต้องมีตัวเลขอย่างน้อย 1 ตัว",
            "PasswordRequiresLower" => "รหัสผ่านต้องมีตัวพิมพ์เล็กอย่างน้อย 1 ตัว",
            "PasswordRequiresUpper" => "รหัสผ่านต้องมีตัวพิมพ์ใหญ่อย่างน้อย 1 ตัว",
            "PasswordRequiresNonAlphanumeric" => "รหัสผ่านต้องมีอักขระพิเศษอย่างน้อย 1 ตัว",
            _ => "ไม่สามารถสร้างบัญชีได้ กรุณาตรวจสอบข้อมูลแล้วลองใหม่"
        };
    }
}
