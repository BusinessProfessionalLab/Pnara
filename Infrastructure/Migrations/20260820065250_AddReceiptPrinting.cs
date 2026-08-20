using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptPrinting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrinterDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ConnectionType = table.Column<int>(type: "integer", nullable: false),
                    Host = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    PaperWidth = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptType = table.Column<int>(type: "integer", nullable: false),
                    HeaderText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FooterText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ShowLogo = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPrices = table.Column<bool>(type: "boolean", nullable: false),
                    ShowDiscount = table.Column<bool>(type: "boolean", nullable: false),
                    ShowTax = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPaymentMethod = table.Column<bool>(type: "boolean", nullable: false),
                    ShowChannel = table.Column<bool>(type: "boolean", nullable: false),
                    FontSize = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptPrinterMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptType = table.Column<int>(type: "integer", nullable: false),
                    PrinterDefinitionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptPrinterMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptPrinterMappings_PrinterDefinitions_PrinterDefinition~",
                        column: x => x.PrinterDefinitionId,
                        principalTable: "PrinterDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrinterDefinitions_Name",
                table: "PrinterDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrinterMappings_PrinterDefinitionId",
                table: "ReceiptPrinterMappings",
                column: "PrinterDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrinterMappings_ReceiptType",
                table: "ReceiptPrinterMappings",
                column: "ReceiptType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptTemplates_ReceiptType",
                table: "ReceiptTemplates",
                column: "ReceiptType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptPrinterMappings");

            migrationBuilder.DropTable(
                name: "ReceiptTemplates");

            migrationBuilder.DropTable(
                name: "PrinterDefinitions");
        }
    }
}
