using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinders.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedConfirmedtoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Confirmed",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confirmed",
                table: "Users");
        }
    }
}
