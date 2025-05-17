using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;

namespace ClassManagementSystem.Areas.Admin.Pages.Terms
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Term Term { get; set; } = new Term();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Deactivate all other terms
            var allTerms = _context.Terms.Where(t => t.IsActive);
            foreach (var t in allTerms)
            {
                t.IsActive = false;
            }

            // Always mark new term as active
            Term.IsActive = true;

            _context.Terms.Add(Term);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}