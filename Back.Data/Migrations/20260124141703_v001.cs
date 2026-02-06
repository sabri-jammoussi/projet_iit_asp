using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Data.Migrations
{
    /// <inheritdoc />
    public partial class v001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACCOUNT",
                columns: table => new
                {
                    A_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    A_FIRSTNAME = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    A_LASTNAME = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    A_EMAIL = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    A_PASSWORD_HASH = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    A_PASSWORD_SALT = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    A_ROLE = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACCOUNT", x => x.A_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACCOUNT");
        }
    }
}
