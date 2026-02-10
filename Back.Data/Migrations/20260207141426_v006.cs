using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Data.Migrations
{
    /// <inheritdoc />
    public partial class v006 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ORDER_DETAIL_ORDERS_OD_ORDER_ID",
                table: "ORDER_DETAIL");

            migrationBuilder.RenameColumn(
                name: "OD_ORDER_ID",
                table: "ORDER_DETAIL",
                newName: "O_ID");

            migrationBuilder.RenameIndex(
                name: "IX_ORDER_DETAIL_OD_ORDER_ID",
                table: "ORDER_DETAIL",
                newName: "IX_ORDER_DETAIL_O_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ORDER_DETAIL_ORDERS_O_ID",
                table: "ORDER_DETAIL",
                column: "O_ID",
                principalTable: "ORDERS",
                principalColumn: "O_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ORDER_DETAIL_ORDERS_O_ID",
                table: "ORDER_DETAIL");

            migrationBuilder.RenameColumn(
                name: "O_ID",
                table: "ORDER_DETAIL",
                newName: "OD_ORDER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_ORDER_DETAIL_O_ID",
                table: "ORDER_DETAIL",
                newName: "IX_ORDER_DETAIL_OD_ORDER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ORDER_DETAIL_ORDERS_OD_ORDER_ID",
                table: "ORDER_DETAIL",
                column: "OD_ORDER_ID",
                principalTable: "ORDERS",
                principalColumn: "O_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
