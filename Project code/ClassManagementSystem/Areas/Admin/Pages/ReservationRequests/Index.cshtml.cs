using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;
using Microsoft.Extensions.Logging; // Add if not present

namespace ClassManagementSystem.Areas.Admin.Pages.ReservationRequests
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<IndexModel> _logger; // Add logger

        public IndexModel(ApplicationDbContext db, ILogger<IndexModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        // view‐model wrapping a Reservation plus flags
        public class ReservationView
        {
            public Reservation Reservation { get; set; } = default!;
            public bool IsHoliday        { get; set; }
            public bool HasConflict      { get; set; }
        }

        public IList<ReservationView> Reservations { get; set; } = new List<ReservationView>();

        [BindProperty] public int Id { get; set; }
        [BindProperty] public string Action { get; set; } = "";

        public List<DateTime> Holidays { get; set; } = new();
        public List<Reservation> AllReservations { get; set; } = new();

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            Holidays = await _db.Holidays.Select(h => h.Date).ToListAsync();
            AllReservations = await _db.Reservations
                .Include(r => r.Term)
                .Include(r => r.Classroom)
                .ToListAsync();

            var grouped = AllReservations
                .GroupBy(r => new {
                    r.ClassroomId,
                    r.TermId,
                    DayOfWeek = r.Date.HasValue ? r.Date.Value.DayOfWeek : DayOfWeek.Sunday,
                    r.StartTime,
                    r.EndTime
                })
                .Select(g => {
                    var first = g.First();
                    var dateOnly = first.Date?.Date ?? DateTime.MinValue;
                    var isHoliday = Holidays.Contains(dateOnly);
                    var hasConflict = AllReservations.Any(o =>
                        o.Id != first.Id &&
                        o.Date?.Date == dateOnly &&
                        o.ClassroomId == first.ClassroomId &&
                        o.StartTime < first.EndTime &&
                        first.StartTime < o.EndTime
                    );
                    return new ReservationView {
                        Reservation = first,
                        IsHoliday = isHoliday,
                        HasConflict = hasConflict
                    };
                })
                .OrderBy(v => v.Reservation.Date)
                .ThenBy(v => v.Reservation.StartTime);

            var totalCount = grouped.Count();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            Reservations = grouped
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var reservation = await _db.Reservations.FindAsync(Id);
            if (reservation == null) return NotFound();

            // Find all reservations in the same group
            var group = _db.Reservations
                .Where(r =>
                    r.ClassroomId == reservation.ClassroomId &&
                    r.TermId == reservation.TermId &&
                    r.StartTime == reservation.StartTime &&
                    r.EndTime == reservation.EndTime &&
                    r.Date.HasValue && reservation.Date.HasValue
                )
                .AsEnumerable()
                .Where(r => r.Date.Value.DayOfWeek == reservation.Date.Value.DayOfWeek)
                .ToList();

            if (Action == "approve")
            {
                group.ForEach(r => r.IsApproved = true);
                _logger.LogInformation("Reservation(s) {Ids} approved by {Admin}", string.Join(",", group.Select(r => r.Id)), User.Identity!.Name);
            }
            else if (Action == "reject")
            {
                group.ForEach(r => r.IsApproved = false);
                _logger.LogInformation("Reservation(s) {Ids} rejected by {Admin}", string.Join(",", group.Select(r => r.Id)), User.Identity!.Name);
            }
            else if (Action == "delete")
            {
                _db.Reservations.RemoveRange(group);
                _logger.LogInformation("Reservation(s) {Ids} deleted by {Admin}", string.Join(",", group.Select(r => r.Id)), User.Identity!.Name);
            }

            if (Action != "delete")
                _db.Reservations.UpdateRange(group);

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}