using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHealthEventAndVaccinationRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_SessionStudents_SessionStudentId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_Students_StudentId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_VaccinationSchedules_ScheduleId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_VaccinationTypes_VaccineTypeId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationSchedules_VaccinationCampaigns_CampaignId",
                table: "VaccinationSchedules");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationRecords_ScheduleId",
                table: "VaccinationRecords");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationRecords_VaccineTypeId",
                table: "VaccinationRecords");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "VaccinationRecords");

            migrationBuilder.DropColumn(
                name: "VaccineTypeId",
                table: "VaccinationRecords");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "VaccinationRecords",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CheckupRecordId",
                table: "HealthEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "CounselingAppointments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_AdministeredDate",
                table: "VaccinationRecords",
                column: "AdministeredDate");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_SessionStudent_Date",
                table: "VaccinationRecords",
                columns: new[] { "SessionStudentId", "AdministeredDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_VaccinatedAt",
                table: "VaccinationRecords",
                column: "VaccinatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HealthEvents_CheckupRecordId",
                table: "HealthEvents",
                column: "CheckupRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthEvents_CheckupRecords_CheckupRecordId",
                table: "HealthEvents",
                column: "CheckupRecordId",
                principalTable: "CheckupRecords",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_SessionStudents_SessionStudentId",
                table: "VaccinationRecords",
                column: "SessionStudentId",
                principalTable: "SessionStudents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_Students_StudentId",
                table: "VaccinationRecords",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationSchedules_VaccinationCampaigns_CampaignId",
                table: "VaccinationSchedules",
                column: "CampaignId",
                principalTable: "VaccinationCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthEvents_CheckupRecords_CheckupRecordId",
                table: "HealthEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_SessionStudents_SessionStudentId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecords_Students_StudentId",
                table: "VaccinationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationSchedules_VaccinationCampaigns_CampaignId",
                table: "VaccinationSchedules");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationRecords_AdministeredDate",
                table: "VaccinationRecords");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationRecords_SessionStudent_Date",
                table: "VaccinationRecords");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationRecords_VaccinatedAt",
                table: "VaccinationRecords");

            migrationBuilder.DropIndex(
                name: "IX_HealthEvents_CheckupRecordId",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "CheckupRecordId",
                table: "HealthEvents");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "VaccinationRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduleId",
                table: "VaccinationRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VaccineTypeId",
                table: "VaccinationRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "CounselingAppointments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_ScheduleId",
                table: "VaccinationRecords",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_VaccineTypeId",
                table: "VaccinationRecords",
                column: "VaccineTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_SessionStudents_SessionStudentId",
                table: "VaccinationRecords",
                column: "SessionStudentId",
                principalTable: "SessionStudents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecords_Students_StudentId",
                table: "VaccinationRecords",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_VaccinationSchedules_VaccinationCampaigns_CampaignId",
                table: "VaccinationSchedules",
                column: "CampaignId",
                principalTable: "VaccinationCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
