using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCourse.Customizations.Authorization;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.InputModels.Users;

namespace MyCourse.Pages.Admin
{
    [AuthorizeRole(Role.Administrator)]
    public class UsersModel : PageModel
    {
        private readonly ILogger<UsersModel> _logger;
        private readonly UserManager<ApplicationUser> userManager;

        [BindProperty]
        public UserRoleInputModel? Input { get; set; }
        public IList<ApplicationUser> Users { get; private set; } = new List<ApplicationUser>();
        [BindProperty(SupportsGet = true)]
        public Role InRole { get; set; }

        public UsersModel(ILogger<UsersModel> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            this.userManager = userManager;
        }

        public async Task<IActionResult> OnGet()
        {
            ViewData["Title"] = "Gestione utenti";
            Claim claim = new (ClaimTypes.Role, InRole.ToString());
            Users = await userManager.GetUsersForClaimAsync(claim);
            return Page();
        }

        public async Task<IActionResult> OnPostAssignAsync()
        {
            if(!ModelState.IsValid)
            {
                return await OnGet();
            }
            ApplicationUser user = await userManager.FindByEmailAsync(Input!.Email);
            if(user == null)
            {
                ModelState.AddModelError(nameof(Input.Email), $"L'indirizzo email {Input.Email} non corrisponde a nessun utente");
                return await OnGet();
            }
            IList<Claim> claims = await userManager.GetClaimsAsync(user);
            Claim roleClaim = new (ClaimTypes.Role, Input.Role.ToString());
            if(claims.Any(claim => claim.Type == roleClaim.Type && claim.Value == roleClaim.Value))
            {
                ModelState.AddModelError(nameof(Input.Role), $"Il ruolo {Input.Role} è già assegnato all'utente {Input.Email}");
                return await OnGet();
            }

            IdentityResult result = await userManager.AddClaimAsync(user, roleClaim);
            if(!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, $"L'operazione è fallita: {result.Errors.FirstOrDefault()?.Description}");
                return await OnGet();
            }

            TempData["ConfirmationMessage"] = $"Il ruolo {Input.Role} è stato assegnato all'utente {Input.Email}";
            return RedirectToPage(new { inrole = (int)InRole });
        }

        public async Task<IActionResult> OnPostRevokeAsync()
        {
            if(!ModelState.IsValid)
            {
                return await OnGet();
            }
            ApplicationUser user = await userManager.FindByEmailAsync(Input!.Email);
            if(user == null)
            {
                ModelState.AddModelError(nameof(Input.Email), $"L'indirizzo email {Input.Email} non corrisponde a nessun utente");
                return await OnGet();
            }
            IList<Claim> claims = await userManager.GetClaimsAsync(user);
            Claim roleClaim = new (ClaimTypes.Role, Input.Role.ToString());
            if(!claims.Any(claim => claim.Type == roleClaim.Type && claim.Value == roleClaim.Value))
            {
                ModelState.AddModelError(nameof(Input.Role), $"Il ruolo {Input.Role} non era assegnato all'utente {Input.Email}");
                return await OnGet();
            }

            IdentityResult result = await userManager.RemoveClaimAsync(user, roleClaim);
            if(!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, $"L'operazione è fallita: {result.Errors.FirstOrDefault()?.Description}");
                return await OnGet();
            }

            TempData["ConfirmationMessage"] = $"Il ruolo {Input.Role} è stato revocato all'utente {Input.Email}";
            return RedirectToPage(new { inrole = (int)InRole });
        }
    }
}
