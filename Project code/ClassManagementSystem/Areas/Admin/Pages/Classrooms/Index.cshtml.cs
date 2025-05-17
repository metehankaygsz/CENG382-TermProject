using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;
using FeedbackModel = ClassManagementSystem.Data.Models.Feedback;

namespace ClassManagementSystem.Areas.Admin.Pages.Classrooms
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        public List<Classroom> Classrooms { get; set; } = new();
        public List<FeedbackModel> Feedbacks { get; set; } = new();

        public async Task OnGetAsync()
        {
            Classrooms = await _db.Classrooms.ToListAsync();
            Feedbacks = await _db.Feedbacks.ToListAsync();
        }
    }
}