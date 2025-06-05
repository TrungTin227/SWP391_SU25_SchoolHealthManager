using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedFieldsToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VaccinationTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "VaccinationTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VaccinationSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "VaccinationSchedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VaccinationRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "VaccinationRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VaccinationCampaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "VaccinationCampaigns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SupplyUsages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "SupplyUsages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Reports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ParentVaccinationRecord",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "ParentVaccinationRecord",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Parents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Parents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ParentMedicationDeliveries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "ParentMedicationDeliveries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "NurseProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "NurseProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Medications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Medications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MedicationLots",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "MedicationLots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MedicalSupplies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "MedicalSupplies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HealthProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "HealthProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HealthEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "HealthEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "FileAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "FileAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EventMedications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "EventMedications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Dispenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Dispenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CounselingAppointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "CounselingAppointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CheckupSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "CheckupSchedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CheckupRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "CheckupRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CheckupCampaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "CheckupCampaigns",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VaccinationTypes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VaccinationTypes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VaccinationSchedules");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VaccinationSchedules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VaccinationRecords");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VaccinationRecords");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VaccinationCampaigns");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VaccinationCampaigns");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SupplyUsages");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SupplyUsages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ParentVaccinationRecord");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ParentVaccinationRecord");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ParentMedicationDeliveries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ParentMedicationDeliveries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "NurseProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "NurseProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MedicationLots");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "MedicationLots");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MedicalSupplies");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "MedicalSupplies");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HealthProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "HealthProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "HealthEvents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "FileAttachments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "FileAttachments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EventMedications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "EventMedications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Dispenses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Dispenses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CounselingAppointments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CounselingAppointments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CheckupSchedules");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CheckupSchedules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CheckupRecords");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CheckupRecords");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CheckupCampaigns");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CheckupCampaigns");
        }
    }
}
