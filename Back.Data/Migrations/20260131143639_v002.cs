using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Data.Migrations
{
    /// <inheritdoc />
    public partial class v002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CUSTOMER",
                columns: table => new
                {
                    C_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    C_FIRSTNAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    C_LASTNAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    C_EMAIL = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    C_PHONE = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    C_ADDRESS = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    C_CITY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    C_COUNTRY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    C_CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUSTOMER", x => x.C_ID);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT",
                columns: table => new
                {
                    P_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    P_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    P_DESCRIPTION = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    P_PRICE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    P_STOCK = table.Column<int>(type: "int", nullable: false),
                    P_CATEGORY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    P_CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT", x => x.P_ID);
                });

            migrationBuilder.CreateTable(
                name: "ORDERS",
                columns: table => new
                {
                    O_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    O_ORDER_DATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    O_TOTAL_AMOUNT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    O_STATUS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    O_SHIPPING_ADDRESS = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    O_CUSTOMER_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERS", x => x.O_ID);
                    table.ForeignKey(
                        name: "FK_ORDERS_CUSTOMER_O_CUSTOMER_ID",
                        column: x => x.O_CUSTOMER_ID,
                        principalTable: "CUSTOMER",
                        principalColumn: "C_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ORDER_DETAIL",
                columns: table => new
                {
                    OD_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OD_QUANTITY = table.Column<int>(type: "int", nullable: false),
                    OD_UNIT_PRICE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OD_LINE_TOTAL = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OD_ORDER_ID = table.Column<int>(type: "int", nullable: false),
                    OD_PRODUCT_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_DETAIL", x => x.OD_ID);
                    table.ForeignKey(
                        name: "FK_ORDER_DETAIL_ORDERS_OD_ORDER_ID",
                        column: x => x.OD_ORDER_ID,
                        principalTable: "ORDERS",
                        principalColumn: "O_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ORDER_DETAIL_PRODUCT_OD_PRODUCT_ID",
                        column: x => x.OD_PRODUCT_ID,
                        principalTable: "PRODUCT",
                        principalColumn: "P_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_DETAIL_OD_ORDER_ID",
                table: "ORDER_DETAIL",
                column: "OD_ORDER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_DETAIL_OD_PRODUCT_ID",
                table: "ORDER_DETAIL",
                column: "OD_PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_O_CUSTOMER_ID",
                table: "ORDERS",
                column: "O_CUSTOMER_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ORDER_DETAIL");

            migrationBuilder.DropTable(
                name: "ORDERS");

            migrationBuilder.DropTable(
                name: "PRODUCT");

            migrationBuilder.DropTable(
                name: "CUSTOMER");
        }
    }
}
