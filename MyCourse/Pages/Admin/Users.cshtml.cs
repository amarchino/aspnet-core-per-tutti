using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MyCourse.Models.Entities;
using MyCourse.Models.InputModels.Users;

namespace MyCourse.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly ILogger<UsersModel> _logger;
        private readonly UserManager<ApplicationUser> userManager;

        [BindProperty]
        public UserRoleInputModel Input { get; set; }

        public UsersModel(ILogger<UsersModel> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            this.userManager = userManager;
        }

        public IActionResult OnGet()
        {
            ViewData["Title"] = "Gestione utenti";
            return Page();
        }

        public async Task<IActionResult> OnPostAssignAsync()
        {
            if(!ModelState.IsValid)
            {
                return OnGet();
            }
            ApplicationUser user = await userManager.FindByEmailAsync(Input.Email);
            if(user == null)
            {
                ModelState.AddModelError(nameof(Input.Email), $"L'indirizzo email {Input.Email} non corrisponde a nessun utente");
                return OnGet();
            }
            IList<Claim> claims = await userManager.GetClaimsAsync(user);
            Claim roleClaim = new (ClaimTypes.Role, Input.Role.ToString());
            if(claims.Any(claim => claim.Type == roleClaim.Type && claim.Value == roleClaim.Value))
            {
                ModelState.AddModelError(nameof(Input.Role), $"Il ruolo {Input.Role} è già assegnato all'utente {Input.Email}");
                return OnGet();
            }

            IdentityResult result = await userManager.AddClaimAsync(user, roleClaim);
            if(!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, $"L'operazione è fallita: {result.Errors.FirstOrDefault()?.Description}");
                return OnGet();
            }

            TempData["ConfirmationMessage"] = $"Il ruolo {Input.Role} è stato assegnato all'utente {Input.Email}";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRevokeAsync()
        {
            if(!ModelState.IsValid)
            {
                return OnGet();
            }
            ApplicationUser user = await userManager.FindByEmailAsync(Input.Email);
            if(user == null)
            {
                ModelState.AddModelError(nameof(Input.Email), $"L'indirizzo email {Input.Email} non corrisponde a nessun utente");
                return OnGet();
            }
            IList<Claim> claims = await userManager.GetClaimsAsync(user);
            Claim roleClaim = new (ClaimTypes.Role, Input.Role.ToString());
            if(!claims.Any(claim => claim.Type == roleClaim.Type && claim.Value == roleClaim.Value))
            {
                ModelState.AddModelError(nameof(Input.Role), $"Il ruolo {Input.Role} non era assegnato all'utente {Input.Email}");
                return OnGet();
            }

            IdentityResult result = await userManager.RemoveClaimAsync(user, roleClaim);
            if(!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, $"L'operazione è fallita: {result.Errors.FirstOrDefault()?.Description}");
                return OnGet();
            }

            TempData["ConfirmationMessage"] = $"Il ruolo {Input.Role} è stato revocato all'utente {Input.Email}";
            return RedirectToPage();
        }
    }
}
