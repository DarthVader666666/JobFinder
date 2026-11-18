using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinders.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedResponsetoSearchResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Response",
                table: "JobSearchRequests",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Response",
                table: "JobSearchRequests");
        }
    }
}
