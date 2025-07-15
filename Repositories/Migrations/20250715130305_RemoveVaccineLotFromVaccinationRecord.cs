using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVaccineLotFromVaccinationRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_MedicationLots_VaccineLotId",
                table: "VaccinationRecords");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationRecords_VaccineLotId",
                table: "VaccinationRecords");

            migrationBuilder.DropColumn(
                name: "VaccineLotId",
                table: "VaccinationRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "MedicationLotId",
                table: "VaccinationRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_MedicationLotId",
                table: "VaccinationRecords",
                column: "MedicationLotId");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_MedicationLots_MedicationLotId",
                table: "VaccinationRecords",
                column: "MedicationLotId",
                principalTable: "MedicationLots",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_MedicationLots_MedicationLotId",
                table: "VaccinationRecords");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationRecords_MedicationLotId",
                table: "VaccinationRecords");

            migrationBuilder.DropColumn(
                name: "MedicationLotId",
                table: "VaccinationRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "VaccineLotId",
                table: "VaccinationRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_VaccineLotId",
                table: "VaccinationRecords",
                column: "VaccineLotId");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_MedicationLots_VaccineLotId",
                table: "VaccinationRecords",
                column: "VaccineLotId",
                principalTable: "MedicationLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
