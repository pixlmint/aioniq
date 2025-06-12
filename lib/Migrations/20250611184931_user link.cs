using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aioniq_api.Migrations
{
    /// <inheritdoc />
    public partial class userlink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserTokens_UserTokensId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserTokensId",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserTokens");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserTokensId",
                table: "Users",
                column: "UserTokensId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserTokens_UserTokensId",
                table: "Users",
                column: "UserTokensId",
                principalTable: "UserTokens",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
