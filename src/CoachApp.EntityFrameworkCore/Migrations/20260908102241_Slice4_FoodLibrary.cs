using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachApp.Migrations
{
    /// <inheritdoc />
    public partial class Slice4_FoodLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppFoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ServingSize = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    ServingUnit = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Calories = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    ProteinG = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    CarbsG = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    FatG = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
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
                    table.PrimaryKey("PK_AppFoods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppFoods_TenantId_Name",
                table: "AppFoods",
                columns: new[] { "TenantId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppFoods");
        }
    }
}
