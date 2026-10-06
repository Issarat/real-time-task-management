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
    [HttpGet("/profile")]
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        return View(CreateProfileViewModel(user));
    }

    [Authorize]
    [HttpPost("/profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(
        [Bind(Prefix = nameof(ProfileViewModel.EditProfile))]
        EditProfileViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (ModelState.IsValid)
        {
            user.DisplayName = model.DisplayName.Trim();
            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["ProfileMessage"] = "บันทึกข้อมูลโปรไฟล์เรียบร้อยแล้ว";
                return RedirectToAction(nameof(Profile));
            }

            ModelState.AddModelError(
                string.Empty,
                "ไม่สามารถบันทึกโปรไฟล์ได้ กรุณาลองใหม่");
        }

        return View(CreateProfileViewModel(user, model, openEditMode: true));
    }

    [Authorize]
    [HttpPost("/profile/password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        [Bind(Prefix = nameof(ProfileViewModel.PasswordChange))]
        PasswordChangeViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (model.CurrentPassword == model.NewPassword
            && !string.IsNullOrEmpty(model.NewPassword))
        {
            ModelState.AddModelError(
                nameof(ProfileViewModel.PasswordChange) + "." + nameof(model.NewPassword),
                "รหัสผ่านใหม่ต้องไม่ซ้ำกับรหัสผ่านปัจจุบัน");
        }

        if (ModelState.IsValid)
        {
            var result = await userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (result.Succeeded)
            {
                await signInManager.RefreshSignInAsync(user);
                TempData["ProfileMessage"] = "เปลี่ยนรหัสผ่านเรียบร้อยแล้ว";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    LocalizeIdentityError(error));
            }
        }

        return View(
            nameof(Profile),
            CreateProfileViewModel(
                user,
                passwordChange: model,
                openPasswordForm: true));
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

    private static ProfileViewModel CreateProfileViewModel(
        ApplicationUser user,
        EditProfileViewModel? editProfile = null,
        bool openEditMode = false,
        PasswordChangeViewModel? passwordChange = null,
        bool openPasswordForm = false)
    {
        var displayName = string.IsNullOrWhiteSpace(user.DisplayName)
            ? user.Email ?? "สมาชิก"
            : user.DisplayName;

        editProfile ??= new EditProfileViewModel
        {
            DisplayName = displayName
        };

        passwordChange ??= new PasswordChangeViewModel();

        return new ProfileViewModel(
            displayName,
            user.Email ?? string.Empty,
            CreateInitials(displayName),
            user.CreatedAtUtc,
            editProfile,
            passwordChange,
            openEditMode,
            openPasswordForm);
    }

    private static string CreateInitials(string displayName)
    {
        var initials = string.Concat(
            displayName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part => part[0]))
            .ToUpperInvariant();

        return string.IsNullOrWhiteSpace(initials) ? "U" : initials;
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
            "PasswordMismatch" => "รหัสผ่านปัจจุบันไม่ถูกต้อง",
            _ => "ไม่สามารถดำเนินการได้ กรุณาตรวจสอบข้อมูลแล้วลองใหม่"
        };
    }
}
