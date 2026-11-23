using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanadaImmigration.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriberId = table.Column<int>(type: "INTEGER", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    DrawNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    SentAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_DrawNumber",
                table: "NotificationLogs",
                column: "DrawNumber");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_DrawNumber_Email",
                table: "NotificationLogs",
                columns: new[] { "DrawNumber", "Email" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationLogs");
        }
    }
}
