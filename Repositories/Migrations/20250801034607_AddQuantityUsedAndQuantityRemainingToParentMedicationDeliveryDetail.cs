using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityUsedAndQuantityRemainingToParentMedicationDeliveryDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantityRemaining",
                table: "ParentMedicationDeliveryDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityUsed",
                table: "ParentMedicationDeliveryDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityRemaining",
                table: "ParentMedicationDeliveryDetails");

            migrationBuilder.DropColumn(
                name: "QuantityUsed",
                table: "ParentMedicationDeliveryDetails");
        }
    }
}
