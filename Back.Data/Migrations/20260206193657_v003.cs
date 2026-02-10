using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Data.Migrations
{
    /// <inheritdoc />
    public partial class v003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NOTIFICATION",
                columns: table => new
                {
                    N_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    N_TYPE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    N_TITLE = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    N_MESSAGE = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    N_ORDER_ID = table.Column<int>(type: "int", nullable: true),
                    N_CUSTOMER_ID = table.Column<int>(type: "int", nullable: true),
                    N_CUSTOMER_EMAIL = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    N_IS_READ = table.Column<bool>(type: "bit", nullable: false),
                    N_CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    N_SENT_AT = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATION", x => x.N_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NOTIFICATION");
        }
    }
}
