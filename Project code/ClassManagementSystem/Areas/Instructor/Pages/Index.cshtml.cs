using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;

namespace ClassManagementSystem.Areas.Instructor.Pages
{
    [Authorize(Policy = "InstructorPolicy")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        public int TotalReservations  { get; set; }
        public int ApprovedCount      { get; set; }
        public int PendingCount       { get; set; }
        public int RejectedCount      { get; set; }
        public string NextHolidayName { get; set; } = "-";
        public DateTime? NextHolidayDate { get; set; }

        public async Task OnGetAsync()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return;
            }

            // Fetch this instructor's reservations
            var myRes = await _db.Reservations
                .Where(r => r.InstructorName == userName)
                .ToListAsync();

            TotalReservations = myRes.Count;
            ApprovedCount     = myRes.Count(r => r.IsApproved == true);
            PendingCount      = myRes.Count(r => r.IsApproved == null);
            RejectedCount     = myRes.Count(r => r.IsApproved == false);

            // Next holiday
            var today = DateTime.Today;
            var nextH = await _db.Holidays
                .Where(h => h.Date.Date >= today)
                .OrderBy(h => h.Date)
                .FirstOrDefaultAsync();
            if (nextH != null)
            {
                NextHolidayName = nextH.Name;
                NextHolidayDate = nextH.Date.Date;
            }
        }
    }
}