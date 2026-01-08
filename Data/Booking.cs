namespace CityStyle.Web.Data;

public class Booking
{
    public int Id { get; set; }

    public string ServiceName { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string CancelToken { get; set; } = "";


    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Sen: Status, CancellationToken, osv.
}
