using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;

namespace ClassManagementSystem.Areas.Admin.Pages.Feedback
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        public IList<ClassManagementSystem.Data.Models.Feedback> FeedbackList { get; set; } = default!;

        [BindProperty] public int DeleteId { get; set; }

        public async Task OnGetAsync()
        {
            FeedbackList = await _db.Feedbacks
                .Include(f => f.Classroom)
                .OrderByDescending(f => f.SubmittedOn)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var fb = await _db.Feedbacks.FindAsync(DeleteId);
            if (fb != null)
            {
                _db.Feedbacks.Remove(fb);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}