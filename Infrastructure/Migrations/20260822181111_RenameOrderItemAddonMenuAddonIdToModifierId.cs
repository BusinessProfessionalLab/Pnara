using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrderItemAddonMenuAddonIdToModifierId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MenuAddonId",
                table: "OrderItemAddons",
                newName: "ModifierId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItemAddons_MenuAddonId",
                table: "OrderItemAddons",
                newName: "IX_OrderItemAddons_ModifierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModifierId",
                table: "OrderItemAddons",
                newName: "MenuAddonId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItemAddons_ModifierId",
                table: "OrderItemAddons",
                newName: "IX_OrderItemAddons_MenuAddonId");
        }
    }
}
