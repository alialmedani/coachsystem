using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachApp.Migrations
{
    /// <inheritdoc />
    public partial class TemplateFidelity_ScheduledDayAndNutritionTargets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "ScheduledDay",
                table: "AppWorkoutTemplateDays",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetCalories",
                table: "AppNutritionPlanTemplates",
                type: "decimal(9,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetCarbsG",
                table: "AppNutritionPlanTemplates",
                type: "decimal(9,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetFatG",
                table: "AppNutritionPlanTemplates",
                type: "decimal(9,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetProteinG",
                table: "AppNutritionPlanTemplates",
                type: "decimal(9,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduledDay",
                table: "AppWorkoutTemplateDays");

            migrationBuilder.DropColumn(
                name: "TargetCalories",
                table: "AppNutritionPlanTemplates");

            migrationBuilder.DropColumn(
                name: "TargetCarbsG",
                table: "AppNutritionPlanTemplates");

            migrationBuilder.DropColumn(
                name: "TargetFatG",
                table: "AppNutritionPlanTemplates");

            migrationBuilder.DropColumn(
                name: "TargetProteinG",
                table: "AppNutritionPlanTemplates");
        }
    }
}
