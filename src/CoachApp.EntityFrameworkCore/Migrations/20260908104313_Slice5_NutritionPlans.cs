using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachApp.Migrations
{
    /// <inheritdoc />
    public partial class Slice5_NutritionPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNutritionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TraineeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNutritionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppMeals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NutritionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMeals_AppNutritionPlans_NutritionPlanId",
                        column: x => x.NutritionPlanId,
                        principalTable: "AppNutritionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppMealItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMealItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMealItems_AppMeals_MealId",
                        column: x => x.MealId,
                        principalTable: "AppMeals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppMealItems_FoodId",
                table: "AppMealItems",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMealItems_MealId",
                table: "AppMealItems",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMeals_NutritionPlanId",
                table: "AppMeals",
                column: "NutritionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNutritionPlans_TenantId_TraineeId",
                table: "AppNutritionPlans",
                columns: new[] { "TenantId", "TraineeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMealItems");

            migrationBuilder.DropTable(
                name: "AppMeals");

            migrationBuilder.DropTable(
                name: "AppNutritionPlans");
        }
    }
}
