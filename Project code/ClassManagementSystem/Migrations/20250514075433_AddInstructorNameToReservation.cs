using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorNameToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StudentName",
                table: "Reservations",
                newName: "InstructorName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InstructorName",
                table: "Reservations",
                newName: "StudentName");
        }
    }
}
