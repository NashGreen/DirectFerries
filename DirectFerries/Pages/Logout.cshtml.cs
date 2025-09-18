using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DirectFerries.Pages
{
    public class LogoutModel : PageModel
    {
        //if using ms identity or other auth system would sign out of that here too using SignOutAsync()
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();
            
            return RedirectToPage("/Login");
        }
    }
}

