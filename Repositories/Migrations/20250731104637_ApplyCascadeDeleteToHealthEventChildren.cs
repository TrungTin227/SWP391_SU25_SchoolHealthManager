using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ApplyCascadeDeleteToHealthEventChildren : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_HealthEvents_HealthEventId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyUsages_HealthEvents_HealthEventId",
                table: "SupplyUsages");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_HealthEvents_HealthEventId",
                table: "Reports",
                column: "HealthEventId",
                principalTable: "HealthEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyUsages_HealthEvents_HealthEventId",
                table: "SupplyUsages",
                column: "HealthEventId",
                principalTable: "HealthEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_HealthEvents_HealthEventId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyUsages_HealthEvents_HealthEventId",
                table: "SupplyUsages");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_HealthEvents_HealthEventId",
                table: "Reports",
                column: "HealthEventId",
                principalTable: "HealthEvents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyUsages_HealthEvents_HealthEventId",
                table: "SupplyUsages",
                column: "HealthEventId",
                principalTable: "HealthEvents",
                principalColumn: "Id");
        }
    }
}
