using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSomeTablePD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicationName",
                table: "ParentMedicationDeliveries");

            migrationBuilder.DropColumn(
                name: "QuantityDelivered",
                table: "ParentMedicationDeliveries");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReceivedBy",
                table: "ParentMedicationDeliveries",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ParentMedicationDeliveries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "ParentMedicationDeliveryDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentMedicationDeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    DosageInstruction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnedQuantity = table.Column<int>(type: "int", nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ParentMedicationDeliveryDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentMedicationDeliveryDetail_ParentMedicationDeliveries_ParentMedicationDeliveryId",
                        column: x => x.ParentMedicationDeliveryId,
                        principalTable: "ParentMedicationDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationUsageRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TakenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsTaken = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NurseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_MedicationUsageRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationUsageRecord_ParentMedicationDeliveryDetail_DeliveryDetailId",
                        column: x => x.DeliveryDetailId,
                        principalTable: "ParentMedicationDeliveryDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicationUsageRecord_Users_NurseId",
                        column: x => x.NurseId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationUsageRecord_DeliveryDetailId",
                table: "MedicationUsageRecord",
                column: "DeliveryDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationUsageRecord_NurseId",
                table: "MedicationUsageRecord",
                column: "NurseId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentMedicationDeliveryDetail_ParentMedicationDeliveryId",
                table: "ParentMedicationDeliveryDetail",
                column: "ParentMedicationDeliveryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicationUsageRecord");

            migrationBuilder.DropTable(
                name: "ParentMedicationDeliveryDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReceivedBy",
                table: "ParentMedicationDeliveries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ParentMedicationDeliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicationName",
                table: "ParentMedicationDeliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "QuantityDelivered",
                table: "ParentMedicationDeliveries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
