using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinders.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguredConfirmationCodeFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ConfirmationCodes_UserId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationCodes_UserId",
                table: "ConfirmationCodes",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfirmationCodes_Users_UserId",
                table: "ConfirmationCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfirmationCodes_Users_UserId",
                table: "ConfirmationCodes");

            migrationBuilder.DropIndex(
                name: "IX_ConfirmationCodes_UserId",
                table: "ConfirmationCodes");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ConfirmationCodes_UserId",
                table: "Users",
                column: "UserId",
                principalTable: "ConfirmationCodes",
                principalColumn: "CodeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
