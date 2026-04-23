using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddERPAndBarkFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "bark_pitch",
                table: "profile",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bark_proto",
                table: "profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "headshot_url",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "high_bark_var",
                table: "profile",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "low_bark_var",
                table: "profile",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ooc_notes",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bark_pitch",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "bark_proto",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "headshot_url",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "high_bark_var",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "low_bark_var",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "ooc_notes",
                table: "profile");
        }
    }
}
