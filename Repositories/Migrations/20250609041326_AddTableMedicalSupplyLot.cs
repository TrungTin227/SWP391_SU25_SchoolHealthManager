using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddTableMedicalSupplyLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplyUsages_MedicalSupplies_MedicalSupplyId",
                table: "SupplyUsages");

            migrationBuilder.RenameColumn(
                name: "MedicalSupplyId",
                table: "SupplyUsages",
                newName: "MedicalSupplyLotId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplyUsages_MedicalSupplyId",
                table: "SupplyUsages",
                newName: "IX_SupplyUsages_MedicalSupplyLotId");

            migrationBuilder.CreateTable(
                name: "MedicalSupplyLot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManufactureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalSupplyLot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalSupplyLot_MedicalSupplies_MedicalSupplyId",
                        column: x => x.MedicalSupplyId,
                        principalTable: "MedicalSupplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalSupplyLot_MedicalSupplyId",
                table: "MedicalSupplyLot",
                column: "MedicalSupplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyUsages_MedicalSupplyLot_MedicalSupplyLotId",
                table: "SupplyUsages",
                column: "MedicalSupplyLotId",
                principalTable: "MedicalSupplyLot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplyUsages_MedicalSupplyLot_MedicalSupplyLotId",
                table: "SupplyUsages");

            migrationBuilder.DropTable(
                name: "MedicalSupplyLot");

            migrationBuilder.RenameColumn(
                name: "MedicalSupplyLotId",
                table: "SupplyUsages",
                newName: "MedicalSupplyId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplyUsages_MedicalSupplyLotId",
                table: "SupplyUsages",
                newName: "IX_SupplyUsages_MedicalSupplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyUsages_MedicalSupplies_MedicalSupplyId",
                table: "SupplyUsages",
                column: "MedicalSupplyId",
                principalTable: "MedicalSupplies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
