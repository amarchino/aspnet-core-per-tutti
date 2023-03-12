using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MyCourse.Models.InputModels.Users;

namespace MyCourse.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly ILogger<UsersModel> _logger;
        public UserRoleInputModel Input { get; set; }

        public UsersModel(ILogger<UsersModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            ViewData["Title"] = "Gestione utenti";
            return Page();
        }

        public async Task<IActionResult> OnPostAssignAsync()
        {
            if(ModelState.IsValid)
            {
                // TODO
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRevokeAsync()
        {
            if(ModelState.IsValid)
            {
                // TODO
            }
            return RedirectToPage();
        }
    }
}
