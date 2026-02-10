using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Data.Migrations
{
    /// <inheritdoc />
    public partial class v004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "A_ID",
                table: "CUSTOMER",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CUSTOMER_ACCOUNT_ID",
                table: "CUSTOMER",
                column: "A_ID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CUSTOMER_ACCOUNT_A_ID",
                table: "CUSTOMER",
                column: "A_ID",
                principalTable: "ACCOUNT",
                principalColumn: "A_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CUSTOMER_ACCOUNT_A_ID",
                table: "CUSTOMER");

            migrationBuilder.DropIndex(
                name: "IX_CUSTOMER_ACCOUNT_ID",
                table: "CUSTOMER");

            migrationBuilder.DropColumn(
                name: "A_ID",
                table: "CUSTOMER");
        }
    }
}
