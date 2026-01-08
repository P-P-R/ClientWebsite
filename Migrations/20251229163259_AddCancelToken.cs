using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityStyle.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelToken",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelToken",
                table: "Bookings");
        }
    }
}
