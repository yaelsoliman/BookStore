using BulkyWeb.Models;
using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Category
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
       
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<Category> Categories { get; set; }
        public void OnGet()
        {
            Categories=_context.Categories.ToList();
        }
    }
}
