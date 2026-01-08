using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CityStyle.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CityStyle.Web.Pages.Admin;

public class BokningarModel : PageModel
{
    private readonly AppDbContext _db;

    public BokningarModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Booking> Items { get; set; } = new();

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
            return RedirectToPage("/Admin/Login");

        var today = DateOnly.FromDateTime(DateTime.Today);

        Items = _db.Bookings
            .Where(b => b.Date >= today)
            .OrderBy(b => b.Date)
            .ThenBy(b => b.Time)
            .ToList();

        return Page();
    }

    public IActionResult OnPostCancel(int id)
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
            return RedirectToPage("/Admin/Login");

        var booking = _db.Bookings.FirstOrDefault(b => b.Id == id);
        if (booking != null)
        {
            _db.Bookings.Remove(booking);
            _db.SaveChanges();
        }

        return RedirectToPage();
    }
}
