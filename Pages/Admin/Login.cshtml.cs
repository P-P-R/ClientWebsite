using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace CityStyle.Web.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly IMemoryCache _cache;

    public LoginModel(IConfiguration config, IMemoryCache cache)
    {
        _config = config;
        _cache = cache;
    }
    private readonly IConfiguration _config;

    [BindProperty] public string Password { get; set; } = "";
    public string Error { get; set; } = "";

    public void OnGet()
    {
    }

public IActionResult OnPost()
    {
    var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var key = $"admin-login-fails:{ip}";

    var fails = _cache.Get<int?>(key) ?? 0;

    if (fails >= 5)
    {
        Error = "För många försök. Vänta 1 minut och försök igen.";
        return Page();
    }

    var correct = (_config["Admin:Password"] ?? "").Trim();
    var entered = (Password ?? "").Trim();

    // TEMP: skriv ut i terminalen för att se vad som faktiskt används
    Console.WriteLine($"ADMIN PW config='{correct}' (len {correct.Length}) entered-len={entered.Length}");

    if (!string.IsNullOrEmpty(correct) && entered == correct)
    {
        _cache.Remove(key);
        HttpContext.Session.SetString("IsAdmin", "true");
        return RedirectToPage("/Admin/Bokningar");
    }

    fails++;
    _cache.Set(key, fails, TimeSpan.FromMinutes(1));

    Error = "Fel lösenord.";
    return Page();
    }

}
