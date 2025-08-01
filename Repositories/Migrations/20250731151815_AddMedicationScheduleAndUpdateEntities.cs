using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationScheduleAndUpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationUsageRecord_ParentMedicationDeliveryDetail_DeliveryDetailId",
                table: "MedicationUsageRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationUsageRecord_Users_NurseId",
                table: "MedicationUsageRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentMedicationDeliveryDetail_ParentMedicationDeliveries_ParentMedicationDeliveryId",
                table: "ParentMedicationDeliveryDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParentMedicationDeliveryDetail",
                table: "ParentMedicationDeliveryDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicationUsageRecord",
                table: "MedicationUsageRecord");

            migrationBuilder.DropIndex(
                name: "IX_MedicationUsageRecord_NurseId",
                table: "MedicationUsageRecord");

            migrationBuilder.DropColumn(
                name: "NurseId",
                table: "MedicationUsageRecord");

            migrationBuilder.RenameTable(
                name: "ParentMedicationDeliveryDetail",
                newName: "ParentMedicationDeliveryDetails");

            migrationBuilder.RenameTable(
                name: "MedicationUsageRecord",
                newName: "MedicationUsageRecords");

            migrationBuilder.RenameIndex(
                name: "IX_ParentMedicationDeliveryDetail_ParentMedicationDeliveryId",
                table: "ParentMedicationDeliveryDetails",
                newName: "IX_ParentMedicationDeliveryDetails_ParentMedicationDeliveryId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicationUsageRecord_DeliveryDetailId",
                table: "MedicationUsageRecords",
                newName: "IX_MedicationUsageRecords_DeliveryDetailId");

            migrationBuilder.AlterColumn<string>(
                name: "DosageInstruction",
                table: "ParentMedicationDeliveryDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "MedicationScheduleId",
                table: "MedicationUsageRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParentMedicationDeliveryDetails",
                table: "ParentMedicationDeliveryDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicationUsageRecords",
                table: "MedicationUsageRecords",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MedicationSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentMedicationDeliveryDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Dosage = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_MedicationSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationSchedules_ParentMedicationDeliveryDetails_ParentMedicationDeliveryDetailId",
                        column: x => x.ParentMedicationDeliveryDetailId,
                        principalTable: "ParentMedicationDeliveryDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationUsageRecords_CheckedBy",
                table: "MedicationUsageRecords",
                column: "CheckedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationUsageRecords_MedicationScheduleId",
                table: "MedicationUsageRecords",
                column: "MedicationScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationSchedules_ParentMedicationDeliveryDetailId",
                table: "MedicationSchedules",
                column: "ParentMedicationDeliveryDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationUsageRecords_MedicationSchedules_MedicationScheduleId",
                table: "MedicationUsageRecords",
                column: "MedicationScheduleId",
                principalTable: "MedicationSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationUsageRecords_ParentMedicationDeliveryDetails_DeliveryDetailId",
                table: "MedicationUsageRecords",
                column: "DeliveryDetailId",
                principalTable: "ParentMedicationDeliveryDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationUsageRecords_Users_CheckedBy",
                table: "MedicationUsageRecords",
                column: "CheckedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentMedicationDeliveryDetails_ParentMedicationDeliveries_ParentMedicationDeliveryId",
                table: "ParentMedicationDeliveryDetails",
                column: "ParentMedicationDeliveryId",
                principalTable: "ParentMedicationDeliveries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationUsageRecords_MedicationSchedules_MedicationScheduleId",
                table: "MedicationUsageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationUsageRecords_ParentMedicationDeliveryDetails_DeliveryDetailId",
                table: "MedicationUsageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationUsageRecords_Users_CheckedBy",
                table: "MedicationUsageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentMedicationDeliveryDetails_ParentMedicationDeliveries_ParentMedicationDeliveryId",
                table: "ParentMedicationDeliveryDetails");

            migrationBuilder.DropTable(
                name: "MedicationSchedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParentMedicationDeliveryDetails",
                table: "ParentMedicationDeliveryDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicationUsageRecords",
                table: "MedicationUsageRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicationUsageRecords_CheckedBy",
                table: "MedicationUsageRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicationUsageRecords_MedicationScheduleId",
                table: "MedicationUsageRecords");

            migrationBuilder.DropColumn(
                name: "MedicationScheduleId",
                table: "MedicationUsageRecords");

            migrationBuilder.RenameTable(
                name: "ParentMedicationDeliveryDetails",
                newName: "ParentMedicationDeliveryDetail");

            migrationBuilder.RenameTable(
                name: "MedicationUsageRecords",
                newName: "MedicationUsageRecord");

            migrationBuilder.RenameIndex(
                name: "IX_ParentMedicationDeliveryDetails_ParentMedicationDeliveryId",
                table: "ParentMedicationDeliveryDetail",
                newName: "IX_ParentMedicationDeliveryDetail_ParentMedicationDeliveryId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicationUsageRecords_DeliveryDetailId",
                table: "MedicationUsageRecord",
                newName: "IX_MedicationUsageRecord_DeliveryDetailId");

            migrationBuilder.AlterColumn<string>(
                name: "DosageInstruction",
                table: "ParentMedicationDeliveryDetail",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NurseId",
                table: "MedicationUsageRecord",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParentMedicationDeliveryDetail",
                table: "ParentMedicationDeliveryDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicationUsageRecord",
                table: "MedicationUsageRecord",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationUsageRecord_NurseId",
                table: "MedicationUsageRecord",
                column: "NurseId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationUsageRecord_ParentMedicationDeliveryDetail_DeliveryDetailId",
                table: "MedicationUsageRecord",
                column: "DeliveryDetailId",
                principalTable: "ParentMedicationDeliveryDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationUsageRecord_Users_NurseId",
                table: "MedicationUsageRecord",
                column: "NurseId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentMedicationDeliveryDetail_ParentMedicationDeliveries_ParentMedicationDeliveryId",
                table: "ParentMedicationDeliveryDetail",
                column: "ParentMedicationDeliveryId",
                principalTable: "ParentMedicationDeliveries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
