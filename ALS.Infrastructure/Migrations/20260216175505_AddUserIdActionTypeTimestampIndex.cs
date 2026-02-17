using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ALS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdActionTypeTimestampIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_UserId_ActionType_Timestamp",
                table: "AuditEvents",
                columns: new[] { "UserId", "ActionType", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditEvents_UserId_ActionType_Timestamp",
                table: "AuditEvents");
        }
    }
}
