using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    public partial class ERPConsent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "erp_consent",
                table: "profile",
                type: "TEXT",
                nullable: false,
                defaultValue: "Disabled");

            migrationBuilder.AddColumn<bool>(
                name: "non_con",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "erp_consent",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "non_con",
                table: "profile");
        }

    }
}
