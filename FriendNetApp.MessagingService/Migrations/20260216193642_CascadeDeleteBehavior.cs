using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FriendNetApp.MessagingService.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_UserReplicas_User1Id",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_UserReplicas_User2Id",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_UserReplicas_SenderId",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_UserReplicas_User1Id",
                table: "Chats",
                column: "User1Id",
                principalTable: "UserReplicas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_UserReplicas_User2Id",
                table: "Chats",
                column: "User2Id",
                principalTable: "UserReplicas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_UserReplicas_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "UserReplicas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_UserReplicas_User1Id",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_UserReplicas_User2Id",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_UserReplicas_SenderId",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_UserReplicas_User1Id",
                table: "Chats",
                column: "User1Id",
                principalTable: "UserReplicas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_UserReplicas_User2Id",
                table: "Chats",
                column: "User2Id",
                principalTable: "UserReplicas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_UserReplicas_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "UserReplicas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
