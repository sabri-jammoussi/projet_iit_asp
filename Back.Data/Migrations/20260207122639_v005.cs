using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Data.Migrations
{
    /// <inheritdoc />
    public partial class v005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ACCOUNT",
                columns: new[] { "A_ID", "A_EMAIL", "A_FIRSTNAME", "A_LASTNAME", "A_PASSWORD_HASH", "A_PASSWORD_SALT", "A_ROLE" },
                values: new object[] { 1, "Admin@Admin.tn", "Admin", "Admin", new byte[] { 129, 164, 223, 120, 195, 92, 99, 136, 236, 187, 14, 255, 77, 29, 211, 121, 13, 179, 3, 115, 236, 254, 51, 116, 89, 114, 105, 117, 72, 10, 237, 177, 116, 178, 97, 6, 164, 41, 211, 181, 14, 23, 219, 252, 178, 185, 200, 78, 231, 25, 79, 62, 87, 10, 34, 32, 6, 82, 254, 105, 34, 245, 238, 247 }, new byte[] { 249, 57, 186, 187, 152, 62, 234, 69, 141, 26, 180, 39, 49, 166, 60, 235 }, (short)1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ACCOUNT",
                keyColumn: "A_ID",
                keyValue: 1);
        }
    }
}
