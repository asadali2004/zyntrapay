using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminService.Migrations
{
    /// <summary>
    /// Creates the initial AdminService schema for audit action tracking.
    /// </summary>
    public partial class InitialUpdate : Migration
    {
        /// <summary>
        /// Applies initial admin action table, indexes, and defaults.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminAuthUserId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetUserId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminActions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminActions_AdminAuthUserId",
                table: "AdminActions",
                column: "AdminAuthUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminActions_TargetUserId",
                table: "AdminActions",
                column: "TargetUserId");
        }

        /// <summary>
        /// Reverts initial AdminService schema objects.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminActions");
        }
    }
}
