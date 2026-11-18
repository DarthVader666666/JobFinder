using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinders.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenamedJobSearchRequestsToSearchResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobSearchRequests_Users_UserId",
                table: "JobSearchRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobSearchRequests",
                table: "JobSearchRequests");

            migrationBuilder.RenameTable(
                name: "JobSearchRequests",
                newName: "SearchResults");

            migrationBuilder.RenameIndex(
                name: "IX_JobSearchRequests_UserId",
                table: "SearchResults",
                newName: "IX_SearchResults_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchResults",
                table: "SearchResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchResults_Users_UserId",
                table: "SearchResults",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchResults_Users_UserId",
                table: "SearchResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchResults",
                table: "SearchResults");

            migrationBuilder.RenameTable(
                name: "SearchResults",
                newName: "JobSearchRequests");

            migrationBuilder.RenameIndex(
                name: "IX_SearchResults_UserId",
                table: "JobSearchRequests",
                newName: "IX_JobSearchRequests_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobSearchRequests",
                table: "JobSearchRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JobSearchRequests_Users_UserId",
                table: "JobSearchRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
