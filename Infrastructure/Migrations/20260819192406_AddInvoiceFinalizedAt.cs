using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceFinalizedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_Status_IssuedAtUtc",
                table: "Invoices");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAtUtc",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status_FinalizedAtUtc",
                table: "Invoices",
                columns: new[] { "Status", "FinalizedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_Status_FinalizedAtUtc",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "FinalizedAtUtc",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status_IssuedAtUtc",
                table: "Invoices",
                columns: new[] { "Status", "IssuedAtUtc" });
        }
    }
}
