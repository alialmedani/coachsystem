using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutSchedulingAndPrescribedValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrescribedReps",
                table: "AppWorkoutLogEntries",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrescribedSets",
                table: "AppWorkoutLogEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrescribedWeightKg",
                table: "AppWorkoutLogEntries",
                type: "decimal(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ScheduledDay",
                table: "AppWorkoutDays",
                type: "tinyint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkoutDays_WorkoutPlanId_ScheduledDay",
                table: "AppWorkoutDays",
                columns: new[] { "WorkoutPlanId", "ScheduledDay" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppWorkoutDays_WorkoutPlanId_ScheduledDay",
                table: "AppWorkoutDays");

            migrationBuilder.DropColumn(
                name: "PrescribedReps",
                table: "AppWorkoutLogEntries");

            migrationBuilder.DropColumn(
                name: "PrescribedSets",
                table: "AppWorkoutLogEntries");

            migrationBuilder.DropColumn(
                name: "PrescribedWeightKg",
                table: "AppWorkoutLogEntries");

            migrationBuilder.DropColumn(
                name: "ScheduledDay",
                table: "AppWorkoutDays");
        }
    }
}
