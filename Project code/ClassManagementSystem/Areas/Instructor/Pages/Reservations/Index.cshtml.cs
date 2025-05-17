using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;

namespace ClassManagementSystem.Areas.Instructor.Pages.Reservations
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext db, ILogger<IndexModel> logger)
        {
            _db     = db;
            _logger = logger;
        }

        // Display
        public IList<Reservation> Reservations { get; set; } = new List<Reservation>();
        public IList<DateTime>    Holidays     { get; set; } = new List<DateTime>();
        public HashSet<int>       Conflicts    { get; set; } = new();

        // Create binding
        [BindProperty] public Reservation Input { get; set; } = new();
        public SelectList Terms      { get; set; } = default!;
        public SelectList Classrooms { get; set; } = default!;

        // Details binding
        public Reservation DetailsItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Challenge(); // not logged in

            // load holidays
            Holidays = await _db.Holidays
                .Select(h => h.Date.Date)
                .ToListAsync();

            // only this instructor's reservations
            Reservations = await _db.Reservations
                .Where(r => r.InstructorName == userName)
                .Include(r => r.Term)
                .Include(r => r.Classroom)
                .ToListAsync();

            // detect conflicts across this instructor's own reservations
            Conflicts = Reservations
                .GroupBy(r => (r.Date, r.ClassroomId))
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Select(r => r.Id!.Value))
                .ToHashSet();

            await PopulateListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetCreateAsync(string date, string start, string end)
        {
            // pre-fill times
            Input.Date      = DateTime.Parse(date);
            Input.StartTime = TimeSpan.Parse(start.Substring(11));
            Input.EndTime   = TimeSpan.Parse(end.Substring(11));
            await PopulateListsAsync();
            return new PartialViewResult {
                ViewName = "_CreatePartial",
                ViewData = ViewData
            };
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                Input.InstructorName = User.Identity!.Name!;

                if (Input.Date == default) ModelState.AddModelError("Input.Date","Date required");
                if (Input.StartTime == default) ModelState.AddModelError("Input.StartTime","Start required");
                if (Input.EndTime == default) ModelState.AddModelError("Input.EndTime","End required");
                if (Input.StartTime >= Input.EndTime)
                    ModelState.AddModelError("Input.EndTime","End must be after start");

                if (!ModelState.IsValid)
                {
                    await PopulateListsAsync();
                    return new PartialViewResult {
                      ViewName = "_CreatePartial",
                      ViewData = ViewData
                    };
                }

                // Example: Add this logic after validation and before saving
                var term = await _db.Terms.FindAsync(Input.TermId);
                if (term == null)
                {
                    ModelState.AddModelError(nameof(Input.TermId), "Invalid term.");
                    return Page();
                }

                // Ensure the selected date is within the term's range
                if (!Input.Date.HasValue || Input.Date.Value < term.StartDate.Date || Input.Date.Value > term.EndDate.Date)
                {
                    ModelState.AddModelError("Input.Date", $"Date must be within the selected term ({term.StartDate:yyyy-MM-dd} to {term.EndDate:yyyy-MM-dd}).");
                    await PopulateListsAsync();
                    return new PartialViewResult {
                        ViewName = "_CreatePartial",
                        ViewData = ViewData
                    };
                }

                // Find all dates in the term that match the selected day of week
                var startDate = term.StartDate.Date;
                var endDate = term.EndDate.Date;

                if (!Input.Date.HasValue)
                {
                    ModelState.AddModelError("Input.Date", "Date is required.");
                    await PopulateListsAsync();
                    return new PartialViewResult {
                      ViewName = "_CreatePartial",
                      ViewData = ViewData
                    };
                }

                var selectedDayOfWeek = Input.Date.Value.DayOfWeek;

                var reservations = new List<Reservation>();
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == selectedDayOfWeek)
                    {
                        reservations.Add(new Reservation
                        {
                            TermId = Input.TermId,
                            ClassroomId = Input.ClassroomId,
                            InstructorName = Input.InstructorName,
                            Date = date,
                            StartTime = Input.StartTime,
                            EndTime = Input.EndTime,
                            // ...other fields...
                        });
                    }
                }

                _db.Reservations.AddRange(reservations);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Reservation created by {User} on {Date} {Start}–{End} in room {RoomId}",
                    Input.InstructorName,
                    Input.Date, Input.StartTime, Input.EndTime,
                    Input.ClassroomId);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation for {User}", User.Identity!.Name);
                throw;
            }
        }

        public async Task<IActionResult> OnGetDetailsAsync(int id)
        {
            var userName = User.Identity?.Name;
            var r = await _db.Reservations
                .Include(x => x.Term)
                .Include(x => x.Classroom)
                .FirstOrDefaultAsync(x => x.Id == id && x.InstructorName == userName);
            if (r == null) return NotFound();
            DetailsItem = r;
            return new PartialViewResult {
              ViewName = "_DetailsPartial",
              ViewData = ViewData
            };
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var r = await _db.Reservations.FindAsync(id);
                if (r != null)
                {
                    _db.Reservations.Remove(r);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Reservation {Id} deleted by {User}", id, User.Identity!.Name);
                }
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reservation {Id} for {User}", id, User.Identity!.Name);
                throw;
            }
        }

        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            var userName = User.Identity?.Name;
            var r = await _db.Reservations
                .FirstOrDefaultAsync(x => x.Id == id && x.InstructorName == userName);
            if (r == null) return NotFound();
            Input = r;
            await PopulateListsAsync();
            return new PartialViewResult {
                ViewName = "_EditPartial",
                ViewData = ViewData
            };
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var userName = User.Identity?.Name;
            var r = await _db.Reservations
                .FirstOrDefaultAsync(x => x.Id == Input.Id && x.InstructorName == userName);
            if (r == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateListsAsync();
                return new PartialViewResult {
                    ViewName = "_EditPartial",
                    ViewData = ViewData
                };
            }

            // Update fields
            r.Date = Input.Date;
            r.StartTime = Input.StartTime;
            r.EndTime = Input.EndTime;
            r.TermId = Input.TermId;
            r.ClassroomId = Input.ClassroomId;
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        private async Task PopulateListsAsync()
        {
            var activeTerms = await _db.Terms
                .Where(t => t.IsActive)
                .OrderBy(t => t.StartDate)
                .ToListAsync();
            Terms = new(activeTerms, nameof(Term.Id), nameof(Term.Name));

            var rooms = await _db.Classrooms
                .OrderBy(c => c.Name)
                .ToListAsync();
            Classrooms = new(rooms, nameof(Classroom.Id), nameof(Classroom.Name));
        }
    }
}