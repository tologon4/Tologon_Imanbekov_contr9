using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace contr9.Migrations
{
    /// <inheritdoc />
    public partial class AddedService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    UserAccount = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceUsers_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "OrganizationName" },
                values: new object[,]
                {
                    { 1, "Aknet" },
                    { 2, "Sever Electrocity" },
                    { 3, "Gov Fines" }
                });

            migrationBuilder.InsertData(
                table: "ServiceUsers",
                columns: new[] { "Id", "Balance", "ServiceId", "UserAccount" },
                values: new object[,]
                {
                    { 1, 1800m, 1, "18092003" },
                    { 2, 6300m, 2, "14061963" },
                    { 3, 2700m, 3, "27072003" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceUsers_ServiceId",
                table: "ServiceUsers",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceUsers");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
