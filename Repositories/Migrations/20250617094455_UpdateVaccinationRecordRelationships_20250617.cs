using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVaccinationRecordRelationships_20250617 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationLots_Medications_MedicationId",
                table: "MedicationLots");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_MedicationLots_VaccineLotId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_Users_VaccinatedUserId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_VaccinationCampaigns_CampaignId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_VaccinationTypes_VaccineTypeId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationSchedules_Students_StudentId",
                table: "VaccinationSchedules");

            migrationBuilder.DropColumn(
                name: "ScheduleType",
                table: "VaccinationSchedules");

            migrationBuilder.DropColumn(
                name: "ConsentSigned",
                table: "VaccinationRecords");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "VaccinationSchedules",
                newName: "VaccinationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_VaccinationSchedules_StudentId",
                table: "VaccinationSchedules",
                newName: "IX_VaccinationSchedules_VaccinationTypeId");

            migrationBuilder.RenameColumn(
                name: "VaccinatedUserId",
                table: "VaccinationRecords",
                newName: "VaccinatedById");

            migrationBuilder.RenameColumn(
                name: "VaccinatedBy",
                table: "VaccinationRecords",
                newName: "SessionStudentId");

            migrationBuilder.RenameColumn(
                name: "CampaignId",
                table: "VaccinationRecords",
                newName: "ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_VaccinationRecords_VaccinatedUserId",
                table: "VaccinationRecords",
                newName: "IX_VaccinationRecords_VaccinatedById");

            migrationBuilder.RenameIndex(
                name: "IX_VaccinationRecords_CampaignId",
                table: "VaccinationRecords",
                newName: "IX_VaccinationRecords_ScheduleId");

            migrationBuilder.AlterColumn<Guid>(
                name: "MedicationId",
                table: "MedicationLots",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "MedicationLots",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VaccineTypeId",
                table: "MedicationLots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SessionStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaccinationScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentNotifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentSignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentSignature = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConsentDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionStudents_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionStudents_VaccinationSchedules_VaccinationScheduleId",
                        column: x => x.VaccinationScheduleId,
                        principalTable: "VaccinationSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_SessionStudentId",
                table: "VaccinationRecords",
                column: "SessionStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationLots_LotNumber",
                table: "MedicationLots",
                column: "LotNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationLots_VaccineTypeId",
                table: "MedicationLots",
                column: "VaccineTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionStudents_ScheduleId_StudentId_Unique",
                table: "SessionStudents",
                columns: new[] { "VaccinationScheduleId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionStudents_StudentId",
                table: "SessionStudents",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionStudents_VaccinationScheduleId",
                table: "SessionStudents",
                column: "VaccinationScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationLots_Medications_MedicationId",
                table: "MedicationLots",
                column: "MedicationId",
                principalTable: "Medications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationLots_VaccinationTypes_VaccineTypeId",
                table: "MedicationLots",
                column: "VaccineTypeId",
                principalTable: "VaccinationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_MedicationLots_VaccineLotId",
                table: "VaccinationRecords",
                column: "VaccineLotId",
                principalTable: "MedicationLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_SessionStudents_SessionStudentId",
                table: "VaccinationRecords",
                column: "SessionStudentId",
                principalTable: "SessionStudents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_Users_VaccinatedById",
                table: "VaccinationRecords",
                column: "VaccinatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_VaccinationSchedules_ScheduleId",
                table: "VaccinationRecords",
                column: "ScheduleId",
                principalTable: "VaccinationSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_VaccinationTypes_VaccineTypeId",
                table: "VaccinationRecords",
                column: "VaccineTypeId",
                principalTable: "VaccinationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationSchedules_VaccinationTypes_VaccinationTypeId",
                table: "VaccinationSchedules",
                column: "VaccinationTypeId",
                principalTable: "VaccinationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationLots_Medications_MedicationId",
                table: "MedicationLots");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationLots_VaccinationTypes_VaccineTypeId",
                table: "MedicationLots");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_MedicationLots_VaccineLotId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_SessionStudents_SessionStudentId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_Users_VaccinatedById",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_VaccinationSchedules_ScheduleId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_VaccinationTypes_VaccineTypeId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationSchedules_VaccinationTypes_VaccinationTypeId",
                table: "VaccinationSchedules");

            migrationBuilder.DropTable(
                name: "SessionStudents");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationRecords_SessionStudentId",
                table: "VaccinationRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicationLots_LotNumber",
                table: "MedicationLots");

            migrationBuilder.DropIndex(
                name: "IX_MedicationLots_VaccineTypeId",
                table: "MedicationLots");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "MedicationLots");

            migrationBuilder.DropColumn(
                name: "VaccineTypeId",
                table: "MedicationLots");

            migrationBuilder.RenameColumn(
                name: "VaccinationTypeId",
                table: "VaccinationSchedules",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_VaccinationSchedules_VaccinationTypeId",
                table: "VaccinationSchedules",
                newName: "IX_VaccinationSchedules_StudentId");

            migrationBuilder.RenameColumn(
                name: "VaccinatedById",
                table: "VaccinationRecords",
                newName: "VaccinatedUserId");

            migrationBuilder.RenameColumn(
                name: "SessionStudentId",
                table: "VaccinationRecords",
                newName: "VaccinatedBy");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "VaccinationRecords",
                newName: "CampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_VaccinationRecords_VaccinatedById",
                table: "VaccinationRecords",
                newName: "IX_VaccinationRecords_VaccinatedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_VaccinationRecords_ScheduleId",
                table: "VaccinationRecords",
                newName: "IX_VaccinationRecords_CampaignId");

            migrationBuilder.AddColumn<string>(
                name: "ScheduleType",
                table: "VaccinationSchedules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ConsentSigned",
                table: "VaccinationRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "MedicationId",
                table: "MedicationLots",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationLots_Medications_MedicationId",
                table: "MedicationLots",
                column: "MedicationId",
                principalTable: "Medications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_MedicationLots_VaccineLotId",
                table: "VaccinationRecords",
                column: "VaccineLotId",
                principalTable: "MedicationLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_Users_VaccinatedUserId",
                table: "VaccinationRecords",
                column: "VaccinatedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_VaccinationCampaigns_CampaignId",
                table: "VaccinationRecords",
                column: "CampaignId",
                principalTable: "VaccinationCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_VaccinationTypes_VaccineTypeId",
                table: "VaccinationRecords",
                column: "VaccineTypeId",
                principalTable: "VaccinationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationSchedules_Students_StudentId",
                table: "VaccinationSchedules",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
