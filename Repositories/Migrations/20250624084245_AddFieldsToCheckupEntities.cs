using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToCheckupEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowUpNeeded",
                table: "CheckupRecords");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "CheckupSchedules",
                newName: "ParentConsentStatus");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "CheckupRecords",
                newName: "Remarks");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentReceivedAt",
                table: "CheckupSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "CheckupSchedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                table: "CheckupSchedules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SpecialNotes",
                table: "CheckupSchedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExaminedAt",
                table: "CheckupRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ExaminedByNurseId",
                table: "CheckupRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CheckupRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "CheckupCampaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CheckupCampaigns",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "CheckupCampaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CheckupCampaigns",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CheckupSchedules_RecordId",
                table: "CheckupSchedules",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckupRecords_ExaminedByNurseId",
                table: "CheckupRecords",
                column: "ExaminedByNurseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckupRecords_Users_ExaminedByNurseId",
                table: "CheckupRecords",
                column: "ExaminedByNurseId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckupSchedules_CheckupRecords_RecordId",
                table: "CheckupSchedules",
                column: "RecordId",
                principalTable: "CheckupRecords",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckupRecords_Users_ExaminedByNurseId",
                table: "CheckupRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_CheckupSchedules_CheckupRecords_RecordId",
                table: "CheckupSchedules");

            migrationBuilder.DropIndex(
                name: "IX_CheckupSchedules_RecordId",
                table: "CheckupSchedules");

            migrationBuilder.DropIndex(
                name: "IX_CheckupRecords_ExaminedByNurseId",
                table: "CheckupRecords");

            migrationBuilder.DropColumn(
                name: "ConsentReceivedAt",
                table: "CheckupSchedules");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "CheckupSchedules");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "CheckupSchedules");

            migrationBuilder.DropColumn(
                name: "SpecialNotes",
                table: "CheckupSchedules");

            migrationBuilder.DropColumn(
                name: "ExaminedAt",
                table: "CheckupRecords");

            migrationBuilder.DropColumn(
                name: "ExaminedByNurseId",
                table: "CheckupRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CheckupRecords");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "CheckupCampaigns");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "CheckupCampaigns");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "CheckupCampaigns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CheckupCampaigns");

            migrationBuilder.RenameColumn(
                name: "ParentConsentStatus",
                table: "CheckupSchedules",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "CheckupRecords",
                newName: "Notes");

            migrationBuilder.AddColumn<bool>(
                name: "FollowUpNeeded",
                table: "CheckupRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
