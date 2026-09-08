using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachApp.Migrations
{
    /// <inheritdoc />
    public partial class Slice0_AccessFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppTrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_AppTrainees_TenantId_Code",
                table: "AppTrainees");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AppTrainees");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AppTrainees");

            migrationBuilder.AddColumn<byte>(
                name: "Goal",
                table: "AppTrainees",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<decimal>(
                name: "HeightCm",
                table: "AppTrainees",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StartWeightKg",
                table: "AppTrainees",
                type: "decimal(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetWeightKg",
                table: "AppTrainees",
                type: "decimal(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AppTrainees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "AppTrainees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AppTrainees_TenantId_UserId",
                table: "AppTrainees",
                columns: new[] { "TenantId", "UserId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppTrainees_TenantId_UserName",
                table: "AppTrainees",
                columns: new[] { "TenantId", "UserName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppTrainees_TenantId_UserId",
                table: "AppTrainees");

            migrationBuilder.DropIndex(
                name: "IX_AppTrainees_TenantId_UserName",
                table: "AppTrainees");

            migrationBuilder.DropColumn(
                name: "Goal",
                table: "AppTrainees");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "AppTrainees");

            migrationBuilder.DropColumn(
                name: "StartWeightKg",
                table: "AppTrainees");

            migrationBuilder.DropColumn(
                name: "TargetWeightKg",
                table: "AppTrainees");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AppTrainees");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "AppTrainees");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AppTrainees",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AppTrainees",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AppTrainingPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TraineeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTrainingPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppTrainees_TenantId_Code",
                table: "AppTrainees",
                columns: new[] { "TenantId", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_AppTrainingPlans_TenantId_Title",
                table: "AppTrainingPlans",
                columns: new[] { "TenantId", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_AppTrainingPlans_TenantId_TraineeId",
                table: "AppTrainingPlans",
                columns: new[] { "TenantId", "TraineeId" });
        }
    }
}
