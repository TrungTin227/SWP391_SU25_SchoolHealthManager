using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class CreateCounselingVaccinationAndSupplyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Parents_ParentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_VaccinationSchedules_ScheduleId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentMedicationDeliveries_Parents_DeliveredBy",
                table: "ParentMedicationDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentMedicationDeliveries_Students_StudentId",
                table: "ParentMedicationDeliveries");

            migrationBuilder.DropTable(
                name: "StaffProfiles");

            migrationBuilder.RenameColumn(
                name: "DeliveredBy",
                table: "ParentMedicationDeliveries",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_ParentMedicationDeliveries_DeliveredBy",
                table: "ParentMedicationDeliveries",
                newName: "IX_ParentMedicationDeliveries_ParentId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "MedicalSupplyId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MedicalSupplies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    MinimumStock = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalSupplies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NurseProfiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NurseProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_NurseProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounselingAppointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CheckupRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VaccinationRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounselingAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounselingAppointments_CheckupRecords_CheckupRecordId",
                        column: x => x.CheckupRecordId,
                        principalTable: "CheckupRecords",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CounselingAppointments_NurseProfiles_StaffUserId",
                        column: x => x.StaffUserId,
                        principalTable: "NurseProfiles",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CounselingAppointments_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CounselingAppointments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CounselingAppointments_VaccinationRecords_VaccinationRecordId",
                        column: x => x.VaccinationRecordId,
                        principalTable: "VaccinationRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplyUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HealthEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityUsed = table.Column<int>(type: "int", nullable: false),
                    NurseProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplyUsages_HealthEvents_HealthEventId",
                        column: x => x.HealthEventId,
                        principalTable: "HealthEvents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplyUsages_MedicalSupplies_MedicalSupplyId",
                        column: x => x.MedicalSupplyId,
                        principalTable: "MedicalSupplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplyUsages_NurseProfiles_NurseProfileId",
                        column: x => x.NurseProfileId,
                        principalTable: "NurseProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_MedicalSupplyId",
                table: "Notifications",
                column: "MedicalSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselingAppointments_CheckupRecordId",
                table: "CounselingAppointments",
                column: "CheckupRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselingAppointments_ParentId",
                table: "CounselingAppointments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselingAppointments_StaffUserId",
                table: "CounselingAppointments",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselingAppointments_StudentId",
                table: "CounselingAppointments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselingAppointments_VaccinationRecordId",
                table: "CounselingAppointments",
                column: "VaccinationRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyUsages_HealthEventId",
                table: "SupplyUsages",
                column: "HealthEventId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyUsages_MedicalSupplyId",
                table: "SupplyUsages",
                column: "MedicalSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyUsages_NurseProfileId",
                table: "SupplyUsages",
                column: "NurseProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_MedicalSupplies_MedicalSupplyId",
                table: "Notifications",
                column: "MedicalSupplyId",
                principalTable: "MedicalSupplies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Parents_ParentId",
                table: "Notifications",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_VaccinationSchedules_ScheduleId",
                table: "Notifications",
                column: "ScheduleId",
                principalTable: "VaccinationSchedules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentMedicationDeliveries_Parents_ParentId",
                table: "ParentMedicationDeliveries",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentMedicationDeliveries_Students_StudentId",
                table: "ParentMedicationDeliveries",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_MedicalSupplies_MedicalSupplyId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Parents_ParentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_VaccinationSchedules_ScheduleId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentMedicationDeliveries_Parents_ParentId",
                table: "ParentMedicationDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentMedicationDeliveries_Students_StudentId",
                table: "ParentMedicationDeliveries");

            migrationBuilder.DropTable(
                name: "CounselingAppointments");

            migrationBuilder.DropTable(
                name: "SupplyUsages");

            migrationBuilder.DropTable(
                name: "MedicalSupplies");

            migrationBuilder.DropTable(
                name: "NurseProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_MedicalSupplyId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "MedicalSupplyId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "ParentMedicationDeliveries",
                newName: "DeliveredBy");

            migrationBuilder.RenameIndex(
                name: "IX_ParentMedicationDeliveries_ParentId",
                table: "ParentMedicationDeliveries",
                newName: "IX_ParentMedicationDeliveries_DeliveredBy");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "StaffProfiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_StaffProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Parents_ParentId",
                table: "Notifications",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_VaccinationSchedules_ScheduleId",
                table: "Notifications",
                column: "ScheduleId",
                principalTable: "VaccinationSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentMedicationDeliveries_Parents_DeliveredBy",
                table: "ParentMedicationDeliveries",
                column: "DeliveredBy",
                principalTable: "Parents",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentMedicationDeliveries_Students_StudentId",
                table: "ParentMedicationDeliveries",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
