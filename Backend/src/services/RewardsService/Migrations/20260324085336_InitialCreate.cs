using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardsService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RewardAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthUserId = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Tier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Silver"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RewardCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PointsCost = table.Column<int>(type: "int", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false, defaultValue: -1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Redemptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthUserId = table.Column<int>(type: "int", nullable: false),
                    RewardCatalogId = table.Column<int>(type: "int", nullable: false),
                    PointsSpent = table.Column<int>(type: "int", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Redemptions_RewardCatalog_RewardCatalogId",
                        column: x => x.RewardCatalogId,
                        principalTable: "RewardCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_RewardCatalogId",
                table: "Redemptions",
                column: "RewardCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardAccounts_AuthUserId",
                table: "RewardAccounts",
                column: "AuthUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Redemptions");

            migrationBuilder.DropTable(
                name: "RewardAccounts");

            migrationBuilder.DropTable(
                name: "RewardCatalog");
        }
    }
}
