using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;

namespace ClassManagementSystem.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public int InstructorCount { get; set; }
        public int ClassroomCount { get; set; }
        public int PendingRequestCount { get; set; }
        public int FeedbackCount { get; set; }
        public int UserCount { get; set; }

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            InstructorCount = await _db.Instructors.CountAsync();
            ClassroomCount = await _db.Classrooms.CountAsync();
            PendingRequestCount = await _db.Reservations.Where(r => !r.IsApproved.HasValue).CountAsync();
            FeedbackCount = await _db.Feedbacks.CountAsync();
            UserCount = await _db.Users.CountAsync();
        }
    }
}
