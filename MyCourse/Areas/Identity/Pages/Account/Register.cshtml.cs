using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using MyCourse.Models.Options;

namespace MyCourse.Areas.Identity.Pages.Account;
[AllowAnonymous]
[ValidateReCaptcha]
public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IOptionsMonitor<UsersOptions> _usersOptions;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        IOptionsMonitor<UsersOptions> usersOptions,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _usersOptions = usersOptions;
        _roleManager = roleManager;
    }

    [BindProperty]
    public InputModel? Input { get; set; }

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public class InputModel
    {
        [Required(ErrorMessage = "Il nome completo è obbligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Il nome completo deve essere di almeno {2} e di al massimo {1} caratteri.")]
        [Display(Name = "Nome completo")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Deve essere un indirizzo email valido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La password deve essere di almeno {2} e di al massimo {1} caratteri.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Conferma password")]
        [Compare("Password", ErrorMessage = "La password e la conferma password devono corrispondere.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = Input!.Email, Email = Input.Email, FullName = Input.FullName };
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                var assignRoleNames = _usersOptions.CurrentValue.AssignRolesOnRegistration;

                if (assignRoleNames.ContainsKey(Input.Email.ToLower()))
                {
                    foreach (var roleName in assignRoleNames[Input.Email.ToLower()])
                    {
                        IdentityRole role = await _roleManager.FindByNameAsync(roleName);
                        if (role == null)
                        {
                            role = new IdentityRole(roleName);
                            await _roleManager.CreateAsync(role);
                        }

                        result = await _userManager.AddToRoleAsync(user, roleName);
                        if (!result.Succeeded)
                        {
                            _logger.LogError($"Could not assign role {roleName} to user {Input.Email}");
                            ModelState.AddModelError(string.Empty, $"Non è stato possibile assegnare il ruolo {roleName}");
                        }
                    }
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code, returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Conferma il tuo indirizzo email",
                    $"Per favore conferma la tua registrazione <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>cliccando questo link</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
