using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;

namespace ClassManagementSystem.Areas.Admin.Pages.Classrooms
{
    public class DetailsModel : PageModel
    {
        private readonly ClassManagementSystem.Data.ApplicationDbContext _context;

        public DetailsModel(ClassManagementSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Classroom Classroom { get; set; } = default!;
        public List<Reservation> Reservations { get; set; }
        public List<ClassManagementSystem.Data.Models.Feedback> Feedbacks { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5; // Show 5 per page, change as needed
        public int TotalPages { get; set; }

        public List<Reservation> GroupedReservations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id, int pageNumber = 1)
        {
            if (id == null)
                return NotFound();

            Classroom = await _context.Classrooms.FirstOrDefaultAsync(m => m.Id == id);
            if (Classroom == null)
                return NotFound();

            Reservations = await _context.Reservations
                .Where(r => r.ClassroomId == id)
                .ToListAsync();

            // Group by Day, StartTime, EndTime, Instructor
            GroupedReservations = Reservations
                .GroupBy(r => new {
                    DayOfWeek = r.Date.HasValue ? r.Date.Value.DayOfWeek : DayOfWeek.Sunday,
                    r.StartTime,
                    r.EndTime,
                    r.InstructorName
                })
                .Select(g => g.First())
                .OrderBy(r => r.Date)
                .ThenBy(r => r.StartTime)
                .ToList();

            var allFeedbacks = await _context.Feedbacks
                .Where(f => f.ClassroomId == id)
                .OrderByDescending(f => f.SubmittedOn)
                .ToListAsync();

            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(allFeedbacks.Count / (double)PageSize);
            Feedbacks = allFeedbacks
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }
    }
}
