using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityFieldsToVaccinationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VaccineDoseInfos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "VaccineDoseInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VaccineDoseInfos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "VaccineDoseInfos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VaccineDoseInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VaccineDoseInfos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "VaccineDoseInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VaccineDoseInfos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "VaccineDoseInfos");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VaccineDoseInfos");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VaccineDoseInfos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VaccineDoseInfos");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VaccineDoseInfos");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "VaccineDoseInfos");
        }
    }
}
