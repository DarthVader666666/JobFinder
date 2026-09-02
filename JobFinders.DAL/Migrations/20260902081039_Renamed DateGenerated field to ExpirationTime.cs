using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinders.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenamedDateGeneratedfieldtoExpirationTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateGenerated",
                table: "ConfirmationCodes",
                newName: "ExpirationTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpirationTime",
                table: "ConfirmationCodes",
                newName: "DateGenerated");
        }
    }
}
