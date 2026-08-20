using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuAddons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuAddons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAddons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItemAddons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuAddonId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddonName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItemAddons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItemAddons_InvoiceItems_InvoiceItemId",
                        column: x => x.InvoiceItemId,
                        principalTable: "InvoiceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItemAddons_MenuAddons_MenuAddonId",
                        column: x => x.MenuAddonId,
                        principalTable: "MenuAddons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MenuAddonMenuItems",
                columns: table => new
                {
                    MenuAddonId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAddonMenuItems", x => new { x.MenuAddonId, x.MenuItemId });
                    table.ForeignKey(
                        name: "FK_MenuAddonMenuItems_MenuAddons_MenuAddonId",
                        column: x => x.MenuAddonId,
                        principalTable: "MenuAddons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuAddonMenuItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuAddonRecipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuAddonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAddonRecipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuAddonRecipes_MenuAddons_MenuAddonId",
                        column: x => x.MenuAddonId,
                        principalTable: "MenuAddons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuAddonRecipeComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAddonRecipeComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuAddonRecipeComponents_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuAddonRecipeComponents_MenuAddonRecipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "MenuAddonRecipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItemAddons_InvoiceItemId",
                table: "InvoiceItemAddons",
                column: "InvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItemAddons_MenuAddonId",
                table: "InvoiceItemAddons",
                column: "MenuAddonId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuAddonMenuItems_MenuItemId",
                table: "MenuAddonMenuItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuAddonRecipeComponents_IngredientId",
                table: "MenuAddonRecipeComponents",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuAddonRecipeComponents_RecipeId_IngredientId",
                table: "MenuAddonRecipeComponents",
                columns: new[] { "RecipeId", "IngredientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuAddonRecipes_MenuAddonId",
                table: "MenuAddonRecipes",
                column: "MenuAddonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuAddons_Name",
                table: "MenuAddons",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceItemAddons");

            migrationBuilder.DropTable(
                name: "MenuAddonMenuItems");

            migrationBuilder.DropTable(
                name: "MenuAddonRecipeComponents");

            migrationBuilder.DropTable(
                name: "MenuAddonRecipes");

            migrationBuilder.DropTable(
                name: "MenuAddons");
        }
    }
}
