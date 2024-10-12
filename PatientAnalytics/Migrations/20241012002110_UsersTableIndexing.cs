using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class UsersTableIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Username_Email",
                table: "Users",
                columns: new[] { "Username", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username_Email",
                table: "Users");
        }
    }
}
