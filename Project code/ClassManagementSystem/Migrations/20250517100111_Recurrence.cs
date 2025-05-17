using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class Recurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecursOn",
                table: "Reservations",
                newName: "RecurringDayOfWeek");

            migrationBuilder.AddColumn<DateTime>(
                name: "RecurrenceEndDate",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecurrenceStartDate",
                table: "Reservations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecurrenceEndDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RecurrenceStartDate",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "RecurringDayOfWeek",
                table: "Reservations",
                newName: "RecursOn");
        }
    }
}
