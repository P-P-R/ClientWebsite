using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityStyle.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDataTimeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Date_Time",
                table: "Bookings",
                columns: new[] { "Date", "Time" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_Date_Time",
                table: "Bookings");
        }
    }
}
