using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;

namespace ClassManagementSystem.Areas.Instructor.Pages.Feedback
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        // Existing list
        public IList<ClassManagementSystem.Data.Models.Feedback> Feedbacks { get; set; } = Array.Empty<ClassManagementSystem.Data.Models.Feedback>();

        // For “New Feedback” form
        [BindProperty] public ClassManagementSystem.Data.Models.Feedback Input { get; set; } = new();
        public SelectList Classrooms { get; set; } = default!;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            var userName = User.Identity?.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                var query = _db.Feedbacks
                    .Where(f => f.InstructorName == userName)
                    .Include(f => f.Classroom)
                    .OrderByDescending(f => f.SubmittedOn);

                var totalCount = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

                Feedbacks = await query
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
            }

            await PopulateClassroomsAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Challenge();

            Input.InstructorName = userName;
            Input.SubmittedOn    = DateTime.UtcNow;

            // simple validation
            if (Input.ClassroomId == 0)
                ModelState.AddModelError(nameof(Input.ClassroomId), "Please select a classroom.");
            if (Input.Rating < 1 || Input.Rating > 5)
                ModelState.AddModelError(nameof(Input.Rating), "Rating must be between 1 and 5.");
            if (string.IsNullOrWhiteSpace(Input.Comments))
                ModelState.AddModelError(nameof(Input.Comments), "Comments cannot be empty.");

            if (!ModelState.IsValid)
            {
                await PopulateClassroomsAsync();
                // re-render the full page including modal errors
                return Page();
            }

            _db.Feedbacks.Add(Input);
            await _db.SaveChangesAsync();
            return RedirectToPage(); // reload to show new entry
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var feedback = await _db.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _db.Feedbacks.Remove(feedback);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        private async Task PopulateClassroomsAsync()
        {
            var rooms = await _db.Classrooms
                .OrderBy(c => c.Name)
                .ToListAsync();

            Classrooms = new SelectList(rooms, nameof(Classroom.Id), nameof(Classroom.Name));
        }
    }
}