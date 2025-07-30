using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddRawFieldsAndNewDetailsToHealthEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStock",
                table: "MedicalSupplies");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalNotes",
                table: "HealthEvents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrlsRaw",
                table: "HealthEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventCode",
                table: "HealthEvents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstAidAt",
                table: "HealthEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstAidDescription",
                table: "HealthEvents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FirstResponderId",
                table: "HealthEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InjuredBodyPartsRaw",
                table: "HealthEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReferredToHospital",
                table: "HealthEvents",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "HealthEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ParentArrivalAt",
                table: "HealthEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentNotificationMethod",
                table: "HealthEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentNotificationNote",
                table: "HealthEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ParentNotifiedAt",
                table: "HealthEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentReceivedBy",
                table: "HealthEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentSignatureUrl",
                table: "HealthEvents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReferralDepartureTime",
                table: "HealthEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralHospital",
                table: "HealthEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralTransportBy",
                table: "HealthEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "HealthEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "HealthEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Symptoms",
                table: "HealthEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WitnessesRaw",
                table: "HealthEvents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthEvents_FirstResponderId",
                table: "HealthEvents",
                column: "FirstResponderId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthEvents_NurseProfiles_FirstResponderId",
                table: "HealthEvents",
                column: "FirstResponderId",
                principalTable: "NurseProfiles",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthEvents_NurseProfiles_FirstResponderId",
                table: "HealthEvents");

            migrationBuilder.DropIndex(
                name: "IX_HealthEvents_FirstResponderId",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "AdditionalNotes",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "AttachmentUrlsRaw",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "EventCode",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "FirstAidAt",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "FirstAidDescription",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "FirstResponderId",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "InjuredBodyPartsRaw",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "IsReferredToHospital",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ParentArrivalAt",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ParentNotificationMethod",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ParentNotificationNote",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ParentNotifiedAt",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ParentReceivedBy",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ParentSignatureUrl",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ReferralDepartureTime",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ReferralHospital",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ReferralTransportBy",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "Symptoms",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "WitnessesRaw",
                table: "HealthEvents");

            migrationBuilder.AddColumn<int>(
                name: "CurrentStock",
                table: "MedicalSupplies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
