using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddParentMedicationDeliveryAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentMedicationDeliveryId",
                table: "FileAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileAttachments_ParentMedicationDeliveryId",
                table: "FileAttachments",
                column: "ParentMedicationDeliveryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAttachments_ParentMedicationDeliveries_ParentMedicationDeliveryId",
                table: "FileAttachments",
                column: "ParentMedicationDeliveryId",
                principalTable: "ParentMedicationDeliveries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAttachments_ParentMedicationDeliveries_ParentMedicationDeliveryId",
                table: "FileAttachments");

            migrationBuilder.DropIndex(
                name: "IX_FileAttachments_ParentMedicationDeliveryId",
                table: "FileAttachments");

            migrationBuilder.DropColumn(
                name: "ParentMedicationDeliveryId",
                table: "FileAttachments");
        }
    }
}
