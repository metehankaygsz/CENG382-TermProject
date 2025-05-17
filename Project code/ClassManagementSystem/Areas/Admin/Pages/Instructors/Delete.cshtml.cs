using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace ClassManagementSystem.Areas.Admin.Pages.Instructors
{
    [Authorize(Policy = "AdminPolicy")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userMgr;
        public DeleteModel(UserManager<IdentityUser> um) => _userMgr = um;

        [BindProperty] public IdentityUser UserToDelete { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var u = await _userMgr.FindByIdAsync(id);
            if (u == null) return NotFound();
            UserToDelete = u;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var u = await _userMgr.FindByIdAsync(id);
            if (u != null) await _userMgr.DeleteAsync(u);
            return RedirectToPage("./Index");
        }
    }
}