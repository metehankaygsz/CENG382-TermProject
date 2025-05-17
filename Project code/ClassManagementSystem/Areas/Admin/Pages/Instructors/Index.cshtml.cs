using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassManagementSystem.Areas.Admin.Pages.Instructors
{
    [Authorize(Policy = "AdminPolicy")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userMgr;

        public IndexModel(UserManager<IdentityUser> userMgr)
            => _userMgr = userMgr;

        public IList<IdentityUser> Instructors { get; set; } = new List<IdentityUser>();
        public IList<IdentityUser> Admins { get; set; } = new List<IdentityUser>();

        public async Task OnGetAsync()
        {
            Instructors = await _userMgr.GetUsersInRoleAsync("Instructor");
            Admins = await _userMgr.GetUsersInRoleAsync("Admin");
        }
    }
}