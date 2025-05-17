using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;

namespace ClassManagementSystem.Areas.Admin.Pages.Terms
{
    public class IndexModel : PageModel
    {
        private readonly ClassManagementSystem.Data.ApplicationDbContext _context;

        public IndexModel(ClassManagementSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Term> Term { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Term = await _context.Terms.ToListAsync();
        }
    }
}
