using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachApp.Migrations
{
    /// <inheritdoc />
    public partial class Add_NutritionPlanTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNutritionPlanTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_AppNutritionPlanTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppNutritionTemplateMeals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NutritionPlanTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNutritionTemplateMeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppNutritionTemplateMeals_AppNutritionPlanTemplates_NutritionPlanTemplateId",
                        column: x => x.NutritionPlanTemplateId,
                        principalTable: "AppNutritionPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppNutritionTemplateItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NutritionTemplateMealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNutritionTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppNutritionTemplateItems_AppNutritionTemplateMeals_NutritionTemplateMealId",
                        column: x => x.NutritionTemplateMealId,
                        principalTable: "AppNutritionTemplateMeals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppNutritionPlanTemplates_TenantId_Name",
                table: "AppNutritionPlanTemplates",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNutritionTemplateItems_FoodId",
                table: "AppNutritionTemplateItems",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNutritionTemplateItems_NutritionTemplateMealId",
                table: "AppNutritionTemplateItems",
                column: "NutritionTemplateMealId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNutritionTemplateMeals_NutritionPlanTemplateId",
                table: "AppNutritionTemplateMeals",
                column: "NutritionPlanTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppNutritionTemplateItems");

            migrationBuilder.DropTable(
                name: "AppNutritionTemplateMeals");

            migrationBuilder.DropTable(
                name: "AppNutritionPlanTemplates");
        }
    }
}
