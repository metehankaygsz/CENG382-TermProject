using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;
using FeedbackEntity = ClassManagementSystem.Data.Models.Feedback;


namespace ClassManagementSystem.Areas.Instructor.Pages.Feedback
{
    public class CreateModel : PageModel
    {
        private readonly ClassManagementSystem.Data.ApplicationDbContext _context;

        public CreateModel(ClassManagementSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["ClassroomId"] = new SelectList(_context.Classrooms, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public FeedbackEntity Feedback { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Feedbacks.Add(Feedback);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
