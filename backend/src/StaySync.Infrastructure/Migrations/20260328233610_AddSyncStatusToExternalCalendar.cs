using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaySync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncStatusToExternalCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastSyncErrorMessage",
                table: "ExternalCalendars",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastSyncStatus",
                table: "ExternalCalendars",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSyncErrorMessage",
                table: "ExternalCalendars");

            migrationBuilder.DropColumn(
                name: "LastSyncStatus",
                table: "ExternalCalendars");
        }
    }
}
