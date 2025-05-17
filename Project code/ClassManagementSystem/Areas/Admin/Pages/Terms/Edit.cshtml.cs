using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;

namespace ClassManagementSystem.Areas.Admin.Pages.Terms
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Term Term { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Term = await _context.Terms.FindAsync(id);
            if (Term == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // If this term is being set as active, deactivate all others
            if (Term.IsActive)
            {
                var otherTerms = _context.Terms.Where(t => t.Id != Term.Id && t.IsActive);
                foreach (var t in otherTerms)
                {
                    t.IsActive = false;
                }
            }

            _context.Attach(Term).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Terms.AnyAsync(e => e.Id == Term.Id))
                    return NotFound();
                throw;
            }

            return RedirectToPage("./Index");
        }
    }
}