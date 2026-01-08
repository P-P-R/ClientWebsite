using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CityStyle.Web.Data;
using Microsoft.EntityFrameworkCore;
using CityStyle.Web.Services;
using Microsoft.Extensions.Caching.Memory;

namespace CityStyle.Web.Pages;

public class BokaModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;
    private readonly IMemoryCache _cache;


    public BokaModel(AppDbContext db, EmailService email, IMemoryCache cache)
    {
    _db = db;
    _email = email;
    _cache = cache;
    }


    [BindProperty] public string Service { get; set; } = "";
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Phone { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public DateOnly Date { get; set; }
    [BindProperty] public TimeOnly Time { get; set; }

    public bool BookingSaved { get; set; }
    public List<TimeOnly> AvailableTimes { get; set; } = new();

    private List<TimeOnly> GetAvailableTimes(DateOnly date)
    {
    var dow = date.DayOfWeek;
    var isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;

    var open = isWeekend ? new TimeOnly(11, 0) : new TimeOnly(10, 0);
    var close = isWeekend ? new TimeOnly(15, 0) : new TimeOnly(18, 0);

    var slot = TimeSpan.FromMinutes(30);

    var takenTimes = _db.Bookings
        .Where(b => b.Date == date)
        .Select(b => b.Time)
        .ToHashSet();

    var result = new List<TimeOnly>();
    for (var t = open; t <= close; t = t.Add(slot))
    {
        if (!takenTimes.Contains(t))
            result.Add(t);
    }

    return result;
    }

    public void OnGet(string? service)
    {
        Service = service ?? "";
        if (Date == default)
            Date = DateOnly.FromDateTime(DateTime.Today);

        AvailableTimes = GetAvailableTimes(Date);
    }

    public IActionResult OnPost()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"book-posts:{ip}";

        // räknare inom en 1-minutsfönster
        var count = _cache.Get<int?>(key) ?? 0;

        if (count >= 10)
        {
        ModelState.AddModelError(string.Empty, "För många försök. Vänta en minut och försök igen.");
        AvailableTimes = GetAvailableTimes(Date == default ? DateOnly.FromDateTime(DateTime.Today) : Date);
        return Page();
        }

        // öka räknaren och låt den nollas efter 1 minut
        _cache.Set(key, count + 1, TimeSpan.FromMinutes(1));

        // 1) Datum får inte vara i dåtid
        if (Date < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(Date), "Du kan inte boka ett datum som redan har varit.");
            return Page();
        }

        // 2) Öppettider
        var dow = Date.DayOfWeek;
        var isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;

        if (!isWeekend)
        {
            // Vardagar 10–18
            if (Time < new TimeOnly(10, 0) || Time > new TimeOnly(18, 0))
            {
                ModelState.AddModelError(nameof(Time), "Vardagar kan bokas mellan 10:00 och 18:00.");
                return Page();
            }
        }
        else
        {
            // Helger 11–15
            if (Time < new TimeOnly(11, 0) || Time > new TimeOnly(15, 0))
            {
                ModelState.AddModelError(nameof(Time), "Helger kan bokas mellan 11:00 och 15:00.");
                return Page();
            }
        }

        // 3) Dubbelbokningskoll (snäll feltext)
        var taken = _db.Bookings.Any(b => b.Date == Date && b.Time == Time);
        if (taken)
        {
            ModelState.AddModelError(nameof(Time), "Den tiden är redan bokad. Välj en annan tid.");
            return Page();
        }

        // 4) Spara bokningen
        var booking = new Booking
        {
            ServiceName = Service,
            CustomerName = Name,
            Phone = Phone,
            Email = Email,
            Date = Date,
            Time = Time,
            CancelToken = Guid.NewGuid().ToString("N")
        };

       try
        {
            _db.Bookings.Add(booking);
            _db.SaveChanges();
        }
            catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Time), "Den tiden är redan bokad. Välj en annan tid.");
            return Page();
        }

        // 🔗 Bygg avbokningslänk
        var cancelUrl = Url.Page(
            "/Cancel",
            pageHandler: null,
            values: new { token = booking.CancelToken },
            protocol: Request.Scheme
        );

        // ✉️ Skicka mail (kraschsäkert)
            try
        {
        _email.SendBookingEmail(
        booking.Email,
        "Bokningsbekräftelse – City Style",
        $"Hej {booking.CustomerName}!\n\n" +
        $"Du har bokat:\n" +
        $"{booking.ServiceName}\n" +
        $"{booking.Date} {booking.Time:HH\\:mm}\n\n" +
        $"Avboka här:\n{cancelUrl}\n\n" +
        $"Välkommen!\nCity Style"
    );
    }
        catch
    {
        // Mail misslyckades – men bokningen är sparad
    }

        BookingSaved = true;
    return Page();

    }
}
