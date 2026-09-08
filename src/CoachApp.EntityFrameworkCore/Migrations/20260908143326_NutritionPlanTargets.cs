using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachApp.Migrations
{
    /// <inheritdoc />
    public partial class NutritionPlanTargets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TargetCalories",
                table: "AppNutritionPlans",
                type: "decimal(9,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetCarbsG",
                table: "AppNutritionPlans",
                type: "decimal(9,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetFatG",
                table: "AppNutritionPlans",
                type: "decimal(9,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetProteinG",
                table: "AppNutritionPlans",
                type: "decimal(9,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetCalories",
                table: "AppNutritionPlans");

            migrationBuilder.DropColumn(
                name: "TargetCarbsG",
                table: "AppNutritionPlans");

            migrationBuilder.DropColumn(
                name: "TargetFatG",
                table: "AppNutritionPlans");

            migrationBuilder.DropColumn(
                name: "TargetProteinG",
                table: "AppNutritionPlans");
        }
    }
}
