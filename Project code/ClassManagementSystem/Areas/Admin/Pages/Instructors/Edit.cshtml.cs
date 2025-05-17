using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ClassManagementSystem.Areas.Admin.Pages.Instructors
{
    [Authorize(Policy = "AdminPolicy")]
    public class EditModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userMgr;
        private readonly RoleManager<IdentityRole> _roleMgr;

        public EditModel(UserManager<IdentityUser> um, RoleManager<IdentityRole> rm)
        {
            _userMgr = um;
            _roleMgr = rm;
        }

        [BindProperty] public InputModel Input { get; set; } = new();
        public IList<string> AllRoles { get; set; } = new List<string>();

        public class InputModel
        {
            public string Id { get; set; } = "";
            [Required, EmailAddress] public string Email { get; set; } = "";
            public IList<string> Roles { get; set; } = new List<string>();
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await _userMgr.FindByIdAsync(id);
            if (user == null) return NotFound();

            Input = new InputModel {
                Id = user.Id,
                Email = user.Email!,
                Roles = (await _userMgr.GetRolesAsync(user)).ToList()
            };

            AllRoles = _roleMgr.Roles.Select(r=>r.Name!).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userMgr.FindByIdAsync(Input.Id);
            if (user == null) return NotFound();

            user.Email = Input.Email;
            user.UserName = Input.Email;
            await _userMgr.UpdateAsync(user);

            var current = await _userMgr.GetRolesAsync(user);
            var toAdd   = Input.Roles.Except(current);
            var toRemove= current.Except(Input.Roles);

            await _userMgr.AddToRolesAsync(user, toAdd);
            await _userMgr.RemoveFromRolesAsync(user, toRemove);

            return RedirectToPage("./Index");
        }
    }
}