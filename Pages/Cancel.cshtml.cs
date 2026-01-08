using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CityStyle.Web.Data;

namespace CityStyle.Web.Pages;

public class CancelModel : PageModel
{
    private readonly AppDbContext _db;

    public CancelModel(AppDbContext db)
    {
        _db = db;
    }

    public bool Success { get; set; }

    public IActionResult OnGet(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Index");

        var booking = _db.Bookings.FirstOrDefault(b => b.CancelToken == token);

        if (booking == null)
        {
            Success = false;
            return Page();
        }

        _db.Bookings.Remove(booking);
        _db.SaveChanges();

        Success = true;
        return Page();
    }
}
