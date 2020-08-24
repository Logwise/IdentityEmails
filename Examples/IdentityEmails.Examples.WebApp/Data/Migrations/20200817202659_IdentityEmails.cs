using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityEmails.Examples.WebApp.Data.Migrations
{
    public partial class IdentityEmails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUserEmails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(maxLength: 256, nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: true),
                    LoginProviderKey = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserEmails_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserEmails_AspNetUserLogins_LoginProvider_LoginProviderKey",
                        columns: x => new { x.LoginProvider, x.LoginProviderKey },
                        principalTable: "AspNetUserLogins",
                        principalColumns: new[] { "LoginProvider", "ProviderKey" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserEmails_Email",
                table: "AspNetUserEmails",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserEmails_UserId",
                table: "AspNetUserEmails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserEmails_LoginProvider_LoginProviderKey",
                table: "AspNetUserEmails",
                columns: new[] { "LoginProvider", "LoginProviderKey" },
                unique: true,
                filter: "[LoginProvider] IS NOT NULL AND [LoginProviderKey] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserEmails");
        }
    }
}
