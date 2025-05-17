using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ClassManagementSystem.Areas.Admin.Pages.Instructors
{
    [Authorize(Policy = "AdminPolicy")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userMgr;
        private readonly RoleManager<IdentityRole> _roleMgr;

        public CreateModel(UserManager<IdentityUser> um, RoleManager<IdentityRole> rm)
        {
            _userMgr = um;
            _roleMgr = rm;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, EmailAddress] public string Email { get; set; } = "";
            [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
            [Required] public string Role { get; set; } = "Instructor"; // Default to Instructor
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
            var res = await _userMgr.CreateAsync(user, Input.Password);
            if (!res.Succeeded)
            {
                foreach(var e in res.Errors) ModelState.AddModelError("", e.Description);
                return Page();
            }

            if (!await _roleMgr.RoleExistsAsync(Input.Role))
                await _roleMgr.CreateAsync(new IdentityRole(Input.Role));

            await _userMgr.AddToRoleAsync(user, Input.Role);

            return RedirectToPage("./Index");
        }
    }
}