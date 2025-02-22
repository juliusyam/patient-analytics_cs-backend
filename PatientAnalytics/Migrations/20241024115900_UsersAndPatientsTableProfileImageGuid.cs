using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class UsersAndPatientsTableProfileImageGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProfileImageGuid",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProfileImageGuid",
                table: "Patients",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageGuid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileImageGuid",
                table: "Patients");
        }
    }
}
