using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

public partial class AddPosTerminals : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "OrderId", table: "Invoices", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "IssuedByUserId", table: "Invoices", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "PaidByUserId", table: "Invoices", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "CancelledAtUtc", table: "Invoices", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "CancelledByUserId", table: "Invoices", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<int>(name: "PosPaymentState", table: "Invoices", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<string>(name: "PaymentReferenceNumber", table: "Invoices", type: "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PaymentError", table: "Invoices", type: "character varying(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "PaymentAttemptedAtUtc", table: "Invoices", type: "timestamp with time zone", nullable: true);
        migrationBuilder.CreateTable(
            name: "PosTerminalDefinitions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ConnectionType = table.Column<int>(type: "integer", nullable: false),
                Host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                Port = table.Column<int>(type: "integer", nullable: true),
                SerialPortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                BaudRate = table.Column<int>(type: "integer", nullable: true),
                TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PosTerminalDefinitions", x => x.Id));
        migrationBuilder.CreateIndex(
            name: "IX_PosTerminalDefinitions_IsActive_Name",
            table: "PosTerminalDefinitions",
            columns: new[] { "IsActive", "Name" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_PosTerminalDefinitions_IsActive_Name", table: "PosTerminalDefinitions");
        migrationBuilder.DropTable(name: "PosTerminalDefinitions");
        migrationBuilder.DropColumn(name: "OrderId", table: "Invoices");
        migrationBuilder.DropColumn(name: "IssuedByUserId", table: "Invoices");
        migrationBuilder.DropColumn(name: "PaidByUserId", table: "Invoices");
        migrationBuilder.DropColumn(name: "CancelledAtUtc", table: "Invoices");
        migrationBuilder.DropColumn(name: "CancelledByUserId", table: "Invoices");
        migrationBuilder.DropColumn(name: "PosPaymentState", table: "Invoices");
        migrationBuilder.DropColumn(name: "PaymentReferenceNumber", table: "Invoices");
        migrationBuilder.DropColumn(name: "PaymentError", table: "Invoices");
        migrationBuilder.DropColumn(name: "PaymentAttemptedAtUtc", table: "Invoices");
    }
}
